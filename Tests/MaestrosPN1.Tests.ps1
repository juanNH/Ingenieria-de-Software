param([string]$Config = (Join-Path $PSScriptRoot '../DAL/App.local.config'), [switch]$Legacy)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
[xml]$taskConfig = Get-Content -Raw -LiteralPath $Config
$taskBuilder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $taskConfig.configuration.connectionStrings.add.connectionString
$taskDatabase = 'CodexMaestrosTest_' + [Guid]::NewGuid().ToString('N').Substring(0,12)
$taskBuilder['Initial Catalog'] = 'master'
$taskAdmin = New-Object System.Data.SqlClient.SqlConnection $taskBuilder.ConnectionString
$taskConnection = $null
$taskTransaction = $null
function Invoke-TaskSql([string]$Sql) {
    $taskCommand = $taskConnection.CreateCommand()
    if ($taskTransaction) { $taskCommand.Transaction = $taskTransaction }
    $taskCommand.CommandTimeout = 180
    $taskCommand.CommandText = $Sql
    try { return $taskCommand.ExecuteScalar() } finally { $taskCommand.Dispose() }
}
function Assert-TaskEquals($Actual, $Expected, [string]$Description) {
    if ($Actual -ne $Expected) { throw "$Description. Esperado: $Expected; recibido: $Actual" }
}
try {
    $taskAdmin.Open()
    $taskCommand = $taskAdmin.CreateCommand()
    $taskCommand.CommandText = "CREATE DATABASE [$taskDatabase]"
    [void]$taskCommand.ExecuteNonQuery()
    $taskCommand.Dispose()
    $taskBuilder['Initial Catalog'] = $taskDatabase
    $taskConnection = New-Object System.Data.SqlClient.SqlConnection $taskBuilder.ConnectionString
    $taskConnection.Open()
    $taskScripts = @('00_tecni_salud_schema.sql','StoredProcedures/01_AuthUsuario.sql','StoredProcedures/02_Bitacora.sql',
        'StoredProcedures/03_Auditoria.sql','StoredProcedures/04_Permisos.sql','StoredProcedures/05_IntegridadPN1.sql',
        'StoredProcedures/06_MaestrosPN1.sql','StoredProcedures/07_PN1.sql','StoredProcedures/08_PN2_Catalogos.sql',
        'StoredProcedures/09_PN2.sql','StoredProcedures/10_IntegridadPN2.sql')
    if ($Legacy) { $taskScripts = $taskScripts | Where-Object { $_ -ne 'StoredProcedures/06_MaestrosPN1.sql' } }
    foreach ($taskScript in $taskScripts) {
        $taskSql = (Get-Content -Raw -Encoding UTF8 -LiteralPath (Join-Path $taskRoot ('Database/' + $taskScript))).Replace('HemovidaGest',$taskDatabase)
        if ($Legacy -and $taskScript -eq '00_tecni_salud_schema.sql') {
            $taskSql = [regex]::Replace($taskSql,'(?s)CREATE TABLE dbo\.ComponenteTransfusional.*?INSERT dbo\.FactorRh\(codigo,descripcion\) VALUES.*?\r?\nGO\s*','')
            $taskSql = [regex]::Replace($taskSql,'(?m)^\s*CONSTRAINT FK_Donacion_Componente FOREIGN KEY.*?\r?\n','')
            $taskSql = [regex]::Replace($taskSql,'(?m)^\s*CONSTRAINT FK_Unidad_(Componente|Grupo|Rh) FOREIGN KEY.*?\r?\n','')
        }
        if ($Legacy -and $taskScript -eq 'StoredProcedures/09_PN2.sql') {
            $taskSql = $taskSql.Replace('CONSTRAINT FK_Entrega_Destino FOREIGN KEY (destino) REFERENCES dbo.DestinoEntrega(codigo),',"CONSTRAINT CK_Entrega_Destino CHECK (destino IS NULL OR destino IN ('TRANSFUNDIDA','DEVUELTA','DESCARTADA')),")
            $taskSql = $taskSql.Replace('             LEFT JOIN dbo.DestinoEntrega d ON d.codigo=e.destino','').Replace("d.estado_operativo='TRANSFUNDIDA'","e.destino='TRANSFUNDIDA'")
        }
        foreach ($taskBatch in [regex]::Split($taskSql,'(?im)^\s*GO\s*(?:--[^\r\n]*)?$')) {
            if ($taskBatch.Trim()) {
                try { [void](Invoke-TaskSql $taskBatch) }
                catch { throw "Error instalando $taskScript : $($_.Exception.Message)" }
            }
        }
    }
    if ($Legacy) {
        $taskId = [int](Invoke-TaskSql "SELECT id_usuario FROM dbo.Usuario WHERE nombre_usuario='admin'")
        [void](Invoke-TaskSql @"
BEGIN TRANSACTION; EXEC dbo.sp_IntegridadPN1_Exigir;
INSERT dbo.Donante(documento,nombre,apellido,id_usuario_alta) VALUES('LEGACY-001','Datos','Previos',$taskId);
DECLARE @d INT=SCOPE_IDENTITY(),@f DATETIME=GETDATE(),@v DATE=DATEADD(day,30,GETDATE());
INSERT dbo.Donacion(id_donante,fecha_donacion,cantidad_unidades,tipo_componente,fecha_vencimiento,id_usuario_responsable)
VALUES(@d,@f,1,'Componente historico',@v,$taskId);
DECLARE @donacion INT=SCOPE_IDENTITY();
INSERT dbo.Unidad(codigo_identificacion,id_donacion,tipo_componente,fecha_vencimiento,grupo_sanguineo,factor_rh,id_usuario_ultima_modificacion)
VALUES('LEGACY-UNIT',@donacion,'Componente historico',@v,'A','+',$taskId);
INSERT dbo.SolicitudTransfusional(paciente_receptor,tipo_componente,cantidad_requerida,id_usuario_solicitante,grupo_receptor,rh_receptor)
VALUES(N'Paciente histórico',N'Componente PN2 histórico',1,$taskId,'Z','L');
EXEC dbo.sp_IntegridadPN1_Sellar $taskId; COMMIT;
"@)
        $taskHash = [string](Invoke-TaskSql "SELECT dvh FROM dbo.Unidad WHERE codigo_identificacion='LEGACY-UNIT'")
        $taskTransaction = $taskConnection.BeginTransaction()
        foreach ($taskScript in @('06_MaestrosPN1.sql','07_PN1.sql','09_PN2.sql')) {
            $taskSql = (Get-Content -Raw -Encoding UTF8 -LiteralPath (Join-Path $taskRoot ('Database/StoredProcedures/' + $taskScript))).Replace('HemovidaGest',$taskDatabase).Replace("USE [$taskDatabase];",'')
            foreach ($taskBatch in [regex]::Split($taskSql,'(?im)^\s*GO\s*(?:--[^\r\n]*)?$')) { if ($taskBatch.Trim()) { [void](Invoke-TaskSql $taskBatch) } }
        }
        Assert-TaskEquals (Invoke-TaskSql "SELECT dvh FROM dbo.Unidad WHERE codigo_identificacion='LEGACY-UNIT'") $taskHash 'DVH histórico preservado'
        Assert-TaskEquals (Invoke-TaskSql "SELECT COUNT(*) FROM dbo.ComponenteTransfusional WHERE codigo='Componente historico' AND activo=0") 1 'Componente histórico conservado'
        Assert-TaskEquals (Invoke-TaskSql "SELECT COUNT(*) FROM dbo.ComponenteTransfusional WHERE codigo=N'Componente PN2 histórico' AND activo=0") 1 'Componente histórico de PN2 conservado'
        Assert-TaskEquals (Invoke-TaskSql "SELECT COUNT(*) FROM dbo.GrupoSanguineo WHERE codigo='Z' AND activo=0") 1 'Grupo histórico de PN2 conservado'
        Assert-TaskEquals (Invoke-TaskSql 'SELECT COUNT(*) FROM dbo.v_IntegridadPN1_Errores') 0 'Integridad tras migrar datos históricos'
        [void](Invoke-TaskSql "EXEC dbo.sp_MaestroPN1_Listar 'RH',0")
        $taskTransaction.Commit()
        $taskTransaction.Dispose()
        $taskTransaction = $null
    }
    if (!$Legacy) {
        # Simula la versión anterior de maestros, con su huella ya aceptada.
        [void](Invoke-TaskSql @'
BEGIN TRANSACTION; EXEC dbo.sp_IntegridadPN1_Exigir;
DECLARE @u INT=(SELECT id_usuario FROM dbo.Usuario WHERE nombre_usuario='admin'),@f DATE=DATEADD(day,30,GETDATE());
INSERT dbo.Donante(documento,nombre,apellido,id_usuario_alta) VALUES('UPGRADE-001','Previo','Prueba',@u);
DECLARE @d INT=SCOPE_IDENTITY();
INSERT dbo.Donacion(id_donante,fecha_donacion,cantidad_unidades,tipo_componente,fecha_vencimiento,id_usuario_responsable)
VALUES(@d,GETDATE(),1,'Concentrado de eritrocitos',@f,@u);
DECLARE @donacion INT=SCOPE_IDENTITY();
INSERT dbo.Unidad(codigo_identificacion,id_donacion,tipo_componente,fecha_vencimiento,grupo_sanguineo,factor_rh,estado_operativo,id_usuario_ultima_modificacion)
VALUES('UPGRADE-UNIT',@donacion,'Concentrado de eritrocitos',@f,'O','+','TRANSFUNDIDA',@u);
DECLARE @unidad INT=SCOPE_IDENTITY();
INSERT dbo.SolicitudTransfusional(paciente_receptor,tipo_componente,cantidad_requerida,id_usuario_solicitante) VALUES('Paciente previo','Concentrado de eritrocitos',1,@u);
DECLARE @sol INT=SCOPE_IDENTITY();
INSERT dbo.Reserva(id_solicitud_transfusional,id_unidad,estado_reserva,id_usuario_responsable) VALUES(@sol,@unidad,'ENTREGADA',@u);
DECLARE @res INT=SCOPE_IDENTITY();
INSERT dbo.Entrega(id_reserva,id_usuario_responsable,destino,fecha_destino,id_usuario_destino) VALUES(@res,@u,'TRANSFUNDIDA',GETDATE(),@u);
EXEC dbo.sp_IntegridadPN1_Sellar @u; COMMIT;
'@)
        $taskEntregaHash = [string](Invoke-TaskSql "SELECT e.dvh FROM dbo.Entrega e JOIN dbo.Reserva r ON r.id_reserva=e.id_reserva JOIN dbo.Unidad u ON u.id_unidad=r.id_unidad WHERE u.codigo_identificacion='UPGRADE-UNIT'")
        [void](Invoke-TaskSql @'
ALTER TABLE dbo.Entrega DROP CONSTRAINT FK_Entrega_Destino;
ALTER TABLE dbo.Entrega ADD CONSTRAINT CK_Entrega_Destino CHECK(destino IS NULL OR destino IN ('TRANSFUNDIDA','DEVUELTA','DESCARTADA'));
DROP TABLE dbo.DestinoEntrega;
UPDATE dbo.MaestrosPN1Control SET huella=CONVERT(VARCHAR(64),HASHBYTES('SHA2_256',CONVERT(NVARCHAR(MAX),
 (SELECT id,codigo,descripcion,activo FROM dbo.ComponenteTransfusional ORDER BY id FOR JSON PATH,INCLUDE_NULL_VALUES) + N'|'+
 (SELECT id,codigo,descripcion,activo FROM dbo.GrupoSanguineo ORDER BY id FOR JSON PATH,INCLUDE_NULL_VALUES) + N'|'+
 (SELECT id,codigo,descripcion,activo FROM dbo.FactorRh ORDER BY id FOR JSON PATH,INCLUDE_NULL_VALUES) + N'|'+
 COALESCE((SELECT id_auditoria,entidad,id_entidad,accion,id_usuario_actor,identificador_usuario_actor,fecha_evento,estado_anterior_json,estado_nuevo_json,cambios_json FROM dbo.Auditoria WHERE entidad IN ('MaestroComponente','MaestroGrupo','MaestroRh') ORDER BY id_auditoria FOR JSON PATH,INCLUDE_NULL_VALUES),N'[]'))),2) WHERE id=1;
'@)
        $taskTransaction = $taskConnection.BeginTransaction()
        foreach ($taskScript in @('06_MaestrosPN1.sql','07_PN1.sql','09_PN2.sql')) {
            $taskSql = (Get-Content -Raw -Encoding UTF8 -LiteralPath (Join-Path $taskRoot ('Database/StoredProcedures/' + $taskScript))).Replace('HemovidaGest',$taskDatabase).Replace("USE [$taskDatabase];",'')
            foreach ($taskBatch in [regex]::Split($taskSql,'(?im)^\s*GO\s*(?:--[^\r\n]*)?$')) { if ($taskBatch.Trim()) { [void](Invoke-TaskSql $taskBatch) } }
        }
        [void](Invoke-TaskSql "EXEC dbo.sp_MaestroPN1_Listar 'DESTINO',0")
        Assert-TaskEquals (Invoke-TaskSql 'SELECT COUNT(*) FROM dbo.v_IntegridadPN1_Errores') 0 'Integridad tras ampliar maestros'
        Assert-TaskEquals (Invoke-TaskSql "SELECT e.dvh FROM dbo.Entrega e JOIN dbo.Reserva r ON r.id_reserva=e.id_reserva JOIN dbo.Unidad u ON u.id_unidad=r.id_unidad WHERE u.codigo_identificacion='UPGRADE-UNIT'") $taskEntregaHash 'Entrega histórica preservada'
        $taskTransaction.Commit(); $taskTransaction.Dispose(); $taskTransaction = $null
    }
    Assert-TaskEquals (Invoke-TaskSql 'SELECT COUNT(*) FROM dbo.ComponenteTransfusional WHERE activo=1') 3 'Componentes iniciales'
    Assert-TaskEquals (Invoke-TaskSql 'SELECT COUNT(*) FROM dbo.GrupoSanguineo WHERE activo=1') 4 'Grupos iniciales'
    Assert-TaskEquals (Invoke-TaskSql 'SELECT COUNT(*) FROM dbo.FactorRh WHERE activo=1') 2 'Factores iniciales'
    Assert-TaskEquals (Invoke-TaskSql 'SELECT COUNT(*) FROM dbo.DestinoEntrega WHERE activo=1') 3 'Destinos iniciales'
    $taskId = [int](Invoke-TaskSql "SELECT id_usuario FROM dbo.Usuario WHERE nombre_usuario='admin'")
    foreach ($taskItem in @(@('COMPONENTE','Componente prueba'),@('GRUPO','X'),@('RH','?'))) {
        $taskTipo,$taskCodigo = $taskItem
        $taskResult = Invoke-TaskSql "EXEC dbo.sp_MaestroPN1_Guardar '$taskTipo',0,'$taskCodigo',N'Prueba',1,$taskId"
        Assert-TaskEquals $taskResult 'OK' "Alta $taskTipo"
    }
    Assert-TaskEquals (Invoke-TaskSql "EXEC dbo.sp_MaestroPN1_Guardar 'GRUPO',0,'X',N'Duplicado',1,$taskId") 'DUPLICADO' 'Código duplicado'
    $taskDenegado = $false
    try { [void](Invoke-TaskSql "EXEC dbo.sp_MaestroPN1_Guardar 'GRUPO',0,'Y',N'Sin permiso',1,-1") } catch { $taskDenegado = $true }
    Assert-TaskEquals $taskDenegado $true 'Alta de maestro exige autorización'
    Assert-TaskEquals (Invoke-TaskSql "EXEC dbo.sp_Donante_Registrar 'TEST-001','Ana','Prueba',NULL,NULL,NULL,NULL,$taskId") 'OK' 'Alta donante'
    $taskDonante = [int](Invoke-TaskSql "SELECT id_donante FROM dbo.Donante WHERE documento='TEST-001'")
    Assert-TaskEquals (Invoke-TaskSql "DECLARE @f DATETIME=GETDATE(),@v DATE=DATEADD(day,30,GETDATE()); EXEC dbo.sp_Donacion_Registrar $taskDonante,@f,1,'Componente prueba',@v,NULL,$taskId") 'OK' 'Donación con componente nuevo'
    $taskUnidad = [int](Invoke-TaskSql "SELECT TOP(1) id_unidad FROM dbo.Unidad ORDER BY id_unidad DESC")
    Assert-TaskEquals (Invoke-TaskSql "DECLARE @v DATE=DATEADD(day,30,GETDATE()); EXEC dbo.sp_Unidad_Clasificar $taskUnidad,'Componente prueba','X','?',@v,NULL,$taskId") 'OK' 'Clasificación con grupo y Rh nuevos'
    Assert-TaskEquals (Invoke-TaskSql "DECLARE @f DATE=GETDATE(); EXEC dbo.sp_SolicitudTransfusional_Registrar N'Paciente',N'Componente prueba',1,$taskId,N'DOC','X','?',N'Lab',N'Ref',@f,N'Negativo',N'Profesional'") 'OK' 'Solicitud PN2 con valores nuevos'
    Assert-TaskEquals (Invoke-TaskSql "SELECT dbo.fn_PN2_GrupoCandidato('Componente prueba','X','?','X','?')") 0 'Compatibilidad desconocida cerrada'
    $taskGrupo = [int](Invoke-TaskSql "SELECT id FROM dbo.GrupoSanguineo WHERE codigo='X'")
    Assert-TaskEquals (Invoke-TaskSql "EXEC dbo.sp_MaestroPN1_Guardar 'GRUPO',$taskGrupo,'X',N'Actualizado',0,$taskId") 'OK' 'Modificación y baja lógica'
    Assert-TaskEquals (Invoke-TaskSql "SELECT COUNT(*) FROM dbo.GrupoSanguineo WHERE codigo='X' AND activo=0 AND descripcion=N'Actualizado'") 1 'Registro inactivo conservado'
    Assert-TaskEquals (Invoke-TaskSql "DECLARE @v DATE=DATEADD(day,30,GETDATE()); EXEC dbo.sp_Unidad_Clasificar $taskUnidad,'Componente prueba','X','?',@v,NULL,$taskId") 'DATOS_INVALIDOS' 'Valor inactivo rechazado'
    Assert-TaskEquals (Invoke-TaskSql 'SELECT COUNT(*) FROM dbo.v_IntegridadPN1_Errores') 0 'Integridad del proceso'
    $taskSql = (Get-Content -Raw -Encoding UTF8 -LiteralPath (Join-Path $taskRoot 'Database/StoredProcedures/06_MaestrosPN1.sql')).Replace('HemovidaGest',$taskDatabase)
    foreach ($taskBatch in [regex]::Split($taskSql,'(?im)^\s*GO\s*(?:--[^\r\n]*)?$')) { if ($taskBatch.Trim()) { [void](Invoke-TaskSql $taskBatch) } }
    Assert-TaskEquals (Invoke-TaskSql "SELECT COUNT(*) FROM dbo.GrupoSanguineo WHERE codigo='X' AND activo=0") 1 'Migración repetible'
    Assert-TaskEquals (Invoke-TaskSql 'SELECT COUNT(*) FROM dbo.v_IntegridadPN1_Errores') 0 'Integridad después de repetir migración'
    [void](Invoke-TaskSql "UPDATE dbo.DestinoEntrega SET descripcion=N'Alterado' WHERE codigo='TRANSFUNDIDA'")
    $taskBloqueado = $false
    try { [void](Invoke-TaskSql "EXEC dbo.sp_MaestroPN1_Listar 'DESTINO',0") } catch { $taskBloqueado = $true }
    Assert-TaskEquals $taskBloqueado $true 'Alteración externa de destinos detectada'
    [void](Invoke-TaskSql "UPDATE dbo.DestinoEntrega SET descripcion=N'Transfundida' WHERE codigo='TRANSFUNDIDA'")
    [void](Invoke-TaskSql "EXEC dbo.sp_MaestroPN1_Listar 'DESTINO',0")
    [void](Invoke-TaskSql "UPDATE dbo.GrupoSanguineo SET descripcion=N'Alterado' WHERE codigo='X'")
    $taskBloqueado = $false
    try { [void](Invoke-TaskSql "EXEC dbo.sp_MaestroPN1_Listar 'GRUPO',0") } catch { $taskBloqueado = $true }
    Assert-TaskEquals $taskBloqueado $true 'Alteración externa detectada al consultar'
    $taskBloqueado = $false
    try { [void](Invoke-TaskSql "DECLARE @v DATE=DATEADD(day,30,GETDATE()); EXEC dbo.sp_Unidad_Clasificar $taskUnidad,'Componente prueba','A','+',@v,NULL,$taskId") } catch { $taskBloqueado = $true }
    Assert-TaskEquals $taskBloqueado $true 'Alteración externa bloquea clasificación'
    [void](Invoke-TaskSql "UPDATE dbo.GrupoSanguineo SET descripcion=N'Actualizado' WHERE codigo='X'")
    [void](Invoke-TaskSql "EXEC dbo.sp_MaestroPN1_Listar 'GRUPO',0")
    [void](Invoke-TaskSql "UPDATE dbo.Auditoria SET cambios_json=N'[]' WHERE id_auditoria=(SELECT MIN(id_auditoria) FROM dbo.Auditoria WHERE entidad='MaestroGrupo')")
    $taskBloqueado = $false
    try { [void](Invoke-TaskSql "EXEC dbo.sp_MaestroPN1_Listar 'GRUPO',0") } catch { $taskBloqueado = $true }
    Assert-TaskEquals $taskBloqueado $true 'Alteración del historial de maestros detectada'
    Write-Host 'Maestros PN1: instalación, ABM, PN1, PN2, integridad y repetición correctos.'
} finally {
    if ($taskTransaction) { try { $taskTransaction.Rollback() } catch {} ; $taskTransaction.Dispose() }
    if ($taskConnection) { $taskConnection.Dispose() }
    if ($taskAdmin.State -eq 'Open' -and $taskDatabase -match '^CodexMaestrosTest_[0-9a-f]{12}$') {
        $taskCommand = $taskAdmin.CreateCommand()
        $taskCommand.CommandText = "IF DB_ID('$taskDatabase') IS NOT NULL BEGIN ALTER DATABASE [$taskDatabase] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$taskDatabase]; END"
        try { [void]$taskCommand.ExecuteNonQuery() } finally { $taskCommand.Dispose() }
    }
    $taskAdmin.Dispose()
}
