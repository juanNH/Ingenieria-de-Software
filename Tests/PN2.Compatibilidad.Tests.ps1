param([string]$ConfigPath = (Join-Path $PSScriptRoot '../DAL/App.local.config'))
$ErrorActionPreference = 'Stop'
[xml]$taskConfig = Get-Content -Raw -LiteralPath $ConfigPath
$taskBuilder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $taskConfig.configuration.connectionStrings.add.connectionString
$taskSource = $taskBuilder.InitialCatalog
if ($taskSource -notmatch '^[A-Za-z0-9_]+$') { throw 'Nombre de base inválido.' }
$taskClone = 'CodexPN2Check_' + [Guid]::NewGuid().ToString('N').Substring(0,12)
$taskBuilder['Initial Catalog'] = 'master'
$taskConnection = New-Object System.Data.SqlClient.SqlConnection $taskBuilder.ConnectionString
$taskCreated = $false
function Invoke-TaskSql([string]$Sql, [System.Data.SqlClient.SqlTransaction]$Transaction = $null) {
    $taskCommand = $taskConnection.CreateCommand()
    $taskCommand.CommandTimeout = 180
    $taskCommand.CommandText = $Sql
    if ($Transaction) { $taskCommand.Transaction = $Transaction }
    try { return $taskCommand.ExecuteScalar() } finally { $taskCommand.Dispose() }
}
try {
    $taskConnection.Open()
    [void](Invoke-TaskSql "DBCC CLONEDATABASE ([$taskSource], [$taskClone]) WITH NO_STATISTICS")
    $taskCreated = $true
    [void](Invoke-TaskSql "ALTER DATABASE [$taskClone] SET READ_WRITE")
    $taskConnection.ChangeDatabase($taskClone)

    # La copia de esquema no contiene datos. Se crea un circuito antiguo con datos ficticios
    # y se sella antes de migrar para comprobar la conservación de hashes y versiones.
    [void](Invoke-TaskSql @'
BEGIN TRANSACTION;
INSERT dbo.Usuario(nombre_usuario,email,password_hash) VALUES('prueba_pn2','prueba_pn2@example.invalid','prueba');
DECLARE @user INT=SCOPE_IDENTITY();
INSERT dbo.Donante(documento,nombre,apellido,id_usuario_alta) VALUES('DTEST','Donante','Prueba',@user);
DECLARE @donante INT=SCOPE_IDENTITY();
INSERT dbo.Donacion(id_donante,fecha_donacion,cantidad_unidades,tipo_componente,fecha_vencimiento,id_usuario_responsable)
VALUES(@donante,GETDATE(),2,'Globulos rojos',DATEADD(day,30,GETDATE()),@user);
DECLARE @donacion INT=SCOPE_IDENTITY();
INSERT dbo.Unidad(codigo_identificacion,id_donacion,fecha_vencimiento,id_usuario_ultima_modificacion,tipo_componente,grupo_sanguineo,factor_rh,estado_operativo)
VALUES('TEST-O',@donacion,DATEADD(day,30,GETDATE()),@user,'Globulos rojos','O','-','LIBERADA'),
      ('TEST-B',@donacion,DATEADD(day,30,GETDATE()),@user,'Globulos rojos','B','+','LIBERADA');
INSERT dbo.SolicitudTransfusional(paciente_receptor,tipo_componente,cantidad_requerida,id_usuario_solicitante)
VALUES(N'Receptor prueba',N'Globulos rojos',1,@user);
EXEC dbo.sp_IntegridadPN1_Sellar @user;
COMMIT;
'@)
    if ([int](Invoke-TaskSql 'SELECT COUNT(*) FROM dbo.v_IntegridadPN1_Errores') -ne 0) { throw 'Integridad previa inválida.' }

    $taskTransaction = $taskConnection.BeginTransaction()
    try {
        foreach ($taskFile in @('PN2_Catalogos.sql','PN1.sql','PN2.sql','IntegridadPN2.sql','ComprobantesAutomaticos.sql')) {
            $taskPath = Join-Path $PSScriptRoot ('../Database/StoredProcedures/' + $taskFile)
            $taskSql = (Get-Content -Raw -LiteralPath $taskPath).Replace('USE [HemovidaGest];','')
            foreach ($taskBatch in [regex]::Split($taskSql,'(?im)^\s*GO\s*(?:--[^\r\n]*)?$')) {
                if ($taskBatch.Trim()) { [void](Invoke-TaskSql $taskBatch $taskTransaction) }
            }
        }
        if ([int](Invoke-TaskSql 'SELECT COUNT(*) FROM dbo.v_IntegridadPN1_Errores' $taskTransaction) -ne 0) { throw 'La migración alteró la integridad anterior.' }
        $taskTransaction.Commit()
    } catch { $taskTransaction.Rollback(); throw } finally { $taskTransaction.Dispose() }

    # Solo en la copia se omite la autorización para aislar el flujo de negocio;
    # esta prueba no evalúa permisos ni reemplaza pruebas con usuarios reales.
    [void](Invoke-TaskSql 'CREATE OR ALTER PROCEDURE dbo.sp_IntegridadPN1_Autorizar @id_usuario INT,@permiso VARCHAR(100) AS BEGIN SET NOCOUNT ON; END')
    [void](Invoke-TaskSql 'CREATE OR ALTER PROCEDURE dbo.sp_PN2_Autorizar @id_usuario INT,@permiso VARCHAR(100) AS BEGIN SET NOCOUNT ON; END')
    [void](Invoke-TaskSql @'
IF dbo.fn_PN2_GrupoCandidato(N'Globulos rojos','O','-','A','+')<>1 THROW 51000,'RBC_MATCH',1;
IF dbo.fn_PN2_GrupoCandidato(N'Concentrado de eritrocitos','O','-','A','+')<>1 THROW 51000,'RBC_ALIAS',1;
IF dbo.fn_PN2_GrupoCandidato(N'Globulos rojos','B','+','A','+')<>0 THROW 51000,'RBC_INCOMPATIBLE',1;
IF dbo.fn_PN2_GrupoCandidato(N'Globulos rojos','O','+','A','-')<>0 THROW 51000,'RH_INCOMPATIBLE',1;
IF dbo.fn_PN2_GrupoCandidato(N'Plasma','AB','+','O','-')<>1 THROW 51000,'PLASMA_MATCH',1;
IF dbo.fn_PN2_GrupoCandidato(N'Plasma','O','-','AB','+')<>0 THROW 51000,'PLASMA_INCOMPATIBLE',1;
IF dbo.fn_PN2_GrupoCandidato(N'Otro','O','-','O','-')<>0 THROW 51000,'UNKNOWN_COMPONENT',1;
DECLARE @grupos TABLE(grupo VARCHAR(5)); INSERT @grupos VALUES('O'),('A'),('B'),('AB');
DECLARE @gl TABLE(donante VARCHAR(5),receptor VARCHAR(5));
INSERT @gl VALUES ('O','O'),('O','A'),('O','B'),('O','AB'),('A','A'),('A','AB'),('B','B'),('B','AB'),('AB','AB');
DECLARE @pl TABLE(donante VARCHAR(5),receptor VARCHAR(5));
INSERT @pl VALUES ('O','O'),('A','O'),('B','O'),('AB','O'),('A','A'),('AB','A'),('B','B'),('AB','B'),('AB','AB');
IF EXISTS(SELECT 1 FROM @grupos d CROSS JOIN @grupos r
    WHERE dbo.fn_PN2_GrupoCandidato(N'Globulos rojos',d.grupo,'-',r.grupo,'+') <>
          CASE WHEN EXISTS(SELECT 1 FROM @gl x WHERE x.donante=d.grupo AND x.receptor=r.grupo) THEN 1 ELSE 0 END)
    THROW 51000,'RBC_MATRIX',1;
IF EXISTS(SELECT 1 FROM @grupos d CROSS JOIN @grupos r
    WHERE dbo.fn_PN2_GrupoCandidato(N'Plasma',d.grupo,'+',r.grupo,'-') <>
          CASE WHEN EXISTS(SELECT 1 FROM @pl x WHERE x.donante=d.grupo AND x.receptor=r.grupo) THEN 1 ELSE 0 END)
    THROW 51000,'PLASMA_MATRIX',1;
IF dbo.fn_PN2_GrupoCandidato(N'Plaquetas','A','-','A','-')<>1 OR
   dbo.fn_PN2_GrupoCandidato(N'Plaquetas','O','-','A','+')<>0 THROW 51000,'PLATELET_FILTER',1;
'@)
    $taskUser = [int](Invoke-TaskSql "SELECT id_usuario FROM dbo.Usuario WHERE nombre_usuario='prueba_pn2'")
    $taskRequest = [int](Invoke-TaskSql "SELECT id_solicitud_transfusional FROM dbo.SolicitudTransfusional WHERE paciente_receptor=N'Receptor prueba'")
    $taskCompatible = [int](Invoke-TaskSql "SELECT id_unidad FROM dbo.Unidad WHERE codigo_identificacion='TEST-O'")
    $taskIncompatible = [int](Invoke-TaskSql "SELECT id_unidad FROM dbo.Unidad WHERE codigo_identificacion='TEST-B'")
    if ($null -ne (Invoke-TaskSql "EXEC dbo.sp_PN2_UnidadesCandidatas @id_solicitud=$taskRequest,@id_usuario=$taskUser")) { throw 'Hay candidatos sin estudio validado.' }
    $taskResult = Invoke-TaskSql "EXEC dbo.sp_PN2_ActualizarEstudio @id_solicitud=$taskRequest,@id_usuario=$taskUser,@documento_receptor=N'DOC1',@grupo_receptor='A',@rh_receptor='+',@laboratorio_estudio=N'Lab',@referencia_estudio=N'Informe1',@fecha_estudio='20200101',@resultado_anticuerpos=N'No detectados',@nombre_solicitante=N'Profesional'"
    if ($taskResult -ne 'OK') { throw 'No se actualizó el estudio antiguo.' }
    if ((Invoke-TaskSql "EXEC dbo.sp_PN2_ValidarEstudio @id_solicitud=$taskRequest,@id_usuario=$taskUser") -ne 'OK') { throw 'No se validó el estudio.' }
    if ([int](Invoke-TaskSql "EXEC dbo.sp_PN2_UnidadesCandidatas @id_solicitud=$taskRequest,@id_usuario=$taskUser") -ne $taskCompatible) { throw 'La selección de candidatos es incorrecta.' }
    if ((Invoke-TaskSql "EXEC dbo.sp_Reserva_Registrar @id_solicitud_transfusional=$taskRequest,@id_unidad=$taskIncompatible,@id_usuario=$taskUser,@referencia_compatibilidad=N'Prueba'") -ne 'SIN_DISPONIBILIDAD') { throw 'Se aceptó una unidad incompatible.' }
    if ((Invoke-TaskSql "EXEC dbo.sp_Reserva_Registrar @id_solicitud_transfusional=$taskRequest,@id_unidad=$taskCompatible,@id_usuario=$taskUser,@referencia_compatibilidad=NULL") -ne 'DATOS_INVALIDOS') { throw 'Se aceptó una reserva sin referencia.' }
    if ((Invoke-TaskSql "EXEC dbo.sp_Reserva_Registrar @id_solicitud_transfusional=$taskRequest,@id_unidad=$taskCompatible,@id_usuario=$taskUser,@referencia_compatibilidad=N'Cruce-1'") -ne 'OK') { throw 'Falló la reserva compatible.' }
    $taskReservation = [int](Invoke-TaskSql "SELECT id_reserva FROM dbo.Reserva WHERE id_solicitud_transfusional=$taskRequest AND id_unidad=$taskCompatible")
    if ((Invoke-TaskSql "EXEC dbo.sp_Entrega_Registrar @id_reserva=$taskReservation,@id_usuario=$taskUser") -ne 'OK') { throw 'Falló la entrega.' }
    $taskResult = Invoke-TaskSql "EXEC dbo.sp_SolicitudTransfusional_Registrar @paciente_receptor=N'Otro receptor',@tipo_componente=N'Plasma',@cantidad_requerida=1,@id_usuario=$taskUser,@documento_receptor=N'DOC2',@grupo_receptor='O',@rh_receptor='-',@laboratorio_estudio=N'Lab',@referencia_estudio=N'Informe2',@fecha_estudio='20200101',@resultado_anticuerpos=N'No detectados',@nombre_solicitante=N'Profesional'"
    if ($taskResult -ne 'OK') { throw 'Falló el registro de una solicitud nueva.' }
    if ([int](Invoke-TaskSql "SELECT COUNT(*) FROM dbo.SolicitudTransfusional WHERE paciente_receptor=N'Otro receptor' AND grupo_receptor='O' AND id_usuario_validacion IS NULL") -ne 1) { throw 'La solicitud nueva no quedó pendiente con sus datos.' }
    $taskResult = Invoke-TaskSql "EXEC dbo.sp_SolicitudTransfusional_Registrar @paciente_receptor=N'Sin estudio',@tipo_componente=N'Plasma',@cantidad_requerida=1,@id_usuario=$taskUser,@documento_receptor=N'DOC3',@grupo_receptor=NULL,@rh_receptor=NULL,@laboratorio_estudio=NULL,@referencia_estudio=NULL,@fecha_estudio=NULL,@resultado_anticuerpos=NULL,@nombre_solicitante=N'Profesional'"
    if ($taskResult -ne 'OK') { throw 'No se pudo registrar una solicitud sin estudio.' }
    $taskResult = Invoke-TaskSql "EXEC dbo.sp_SolicitudTransfusional_Registrar @paciente_receptor=N'Incompleto',@tipo_componente=N'Plasma',@cantidad_requerida=1,@id_usuario=$taskUser,@documento_receptor=N'DOC4',@grupo_receptor='O',@rh_receptor=NULL,@laboratorio_estudio=NULL,@referencia_estudio=NULL,@fecha_estudio=NULL,@resultado_anticuerpos=NULL,@nombre_solicitante=N'Profesional'"
    if ($taskResult -ne 'DATOS_INVALIDOS') { throw 'Se aceptó un estudio incompleto.' }
    if ([int](Invoke-TaskSql 'SELECT COUNT(*) FROM dbo.v_IntegridadPN1_Errores') -ne 0) { throw 'Integridad inválida después del flujo.' }
    Write-Host 'Correcto: migración con datos anteriores, integridad, grupos, validación, filtro, reserva y entrega.'
} finally {
    if ($taskCreated) {
        $taskConnection.ChangeDatabase('master')
        [void](Invoke-TaskSql "ALTER DATABASE [$taskClone] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$taskClone]")
    }
    $taskConnection.Dispose()
}
