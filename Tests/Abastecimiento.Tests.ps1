param([string]$Servidor = '.', [string]$Salida, [string]$UiRunner, [string]$UiCapturas)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$taskDatabase = 'CodexAbast_' + [Guid]::NewGuid().ToString('N').Substring(0, 12)
if ($taskDatabase -notmatch '^CodexAbast_[a-f0-9]{12}$') { throw 'Nombre de base de pruebas invalido.' }
$taskMaster = New-Object System.Data.SqlClient.SqlConnection("Server=$Servidor;Initial Catalog=master;Integrated Security=True;TrustServerCertificate=True")
$taskConnection = $null
$taskCreated = $false
$taskPassed = New-Object 'System.Collections.Generic.List[string]'

function Invoke-AbastSql([string]$Sql, [switch]$Table) {
    $taskCommand = $taskConnection.CreateCommand()
    $taskCommand.CommandText = $Sql
    $taskCommand.CommandTimeout = 180
    try {
        if ($Table) {
            $taskResult = New-Object System.Data.DataTable
            $taskAdapter = New-Object System.Data.SqlClient.SqlDataAdapter($taskCommand)
            try { [void]$taskAdapter.Fill($taskResult) } finally { $taskAdapter.Dispose() }
            return ,$taskResult
        }
        return $taskCommand.ExecuteScalar()
    } finally { $taskCommand.Dispose() }
}
function Assert-Abast([bool]$Condition, [string]$Name) {
    if (!$Condition) { throw "Fallo: $Name" }
    $taskPassed.Add($Name)
}
function Assert-AbastError([string]$Sql, [int]$Number, [string]$Name) {
    $taskObserved = $false
    try { [void](Invoke-AbastSql $Sql -Table) }
    catch {
        $taskError = $_.Exception
        while ($taskError.InnerException) { $taskError = $taskError.InnerException }
        if ($taskError -is [System.Data.SqlClient.SqlException] -and $taskError.Number -eq $Number) { $taskObserved = $true }
        else { throw }
    }
    Assert-Abast $taskObserved $Name
}
try {
    $taskMaster.Open()
    $taskCreate = $taskMaster.CreateCommand()
    try { $taskCreate.CommandText = "CREATE DATABASE [$taskDatabase]"; [void]$taskCreate.ExecuteNonQuery(); $taskCreated = $true }
    finally { $taskCreate.Dispose() }
    $taskConnection = New-Object System.Data.SqlClient.SqlConnection("Server=$Servidor;Initial Catalog=$taskDatabase;Integrated Security=True;TrustServerCertificate=True")
    $taskConnection.Open()
    $taskScripts = @('00_tecni_salud_schema.sql','01_AuthUsuario.sql','02_Bitacora.sql','03_Auditoria.sql','04_Permisos.sql',
        '05_IntegridadPN1.sql','06_MaestrosPN1.sql','06a_CompatibilidadTransfusional.sql','06b_TipificacionDonante.sql',
        '07_PN1.sql','08_PN2_Catalogos.sql','09_PN2.sql','09a_RelacionesSolicitudPN2.sql','10_IntegridadPN2.sql',
        '11_ComprobantesAutomaticos.sql','13_AjusteCantidadDonacion.sql','15_AbastecimientoRiesgo.sql','15a_AbastecimientoUI.sql')
    foreach ($taskScript in $taskScripts) {
        $taskPath = if ($taskScript.StartsWith('00_')) { Join-Path $taskRoot "Database/$taskScript" } else { Join-Path $taskRoot "Database/StoredProcedures/$taskScript" }
        $taskSql = [IO.File]::ReadAllText($taskPath)
        if ($taskScript.StartsWith('00_')) { $taskSql = $taskSql.Substring($taskSql.IndexOf('SET ANSI_NULLS')) }
        $taskSql = [regex]::Replace($taskSql, '(?im)^\s*USE\s+\[?HemovidaGest\]?\s*;?\s*$', '')
        foreach ($taskBatch in [regex]::Split($taskSql, '(?im)^\s*GO\s*(?:--[^\r\n]*)?$')) {
            if ($taskBatch.Trim()) { [void](Invoke-AbastSql $taskBatch) }
        }
    }
    [void](Invoke-AbastSql @'
SET XACT_ABORT ON;
BEGIN TRANSACTION;
INSERT dbo.Usuario(nombre_usuario,email,password_hash) VALUES('abast_reporter','abast_reporter@example.invalid','fixture'),('abast_denied','abast_denied@example.invalid','fixture');
DECLARE @usuario INT=(SELECT id_usuario FROM dbo.Usuario WHERE nombre_usuario='abast_reporter');
INSERT dbo.UsuarioComponentePermiso(id_usuario,id_componente) SELECT @usuario,id_componente FROM dbo.ComponentePermiso WHERE codigo='REPORTE_VER';
INSERT dbo.Donante(documento,nombre,apellido,id_usuario_alta) VALUES('ABAST-FIXTURE','Prueba','Abastecimiento',@usuario);
DECLARE @donante INT=SCOPE_IDENTITY(),@hoy DATE=CONVERT(DATE,GETDATE());
INSERT dbo.Donacion(id_donante,fecha_donacion,cantidad_unidades,tipo_componente,fecha_vencimiento,id_usuario_responsable)
VALUES(@donante,DATEADD(DAY,-60,@hoy),16,N'Concentrado de eritrocitos',DATEADD(DAY,90,@hoy),@usuario);
DECLARE @donacion INT=SCOPE_IDENTITY();
INSERT dbo.Unidad(codigo_identificacion,id_donacion,tipo_componente,fecha_vencimiento,grupo_sanguineo,factor_rh,estado_operativo,id_usuario_ultima_modificacion)
SELECT CONCAT('ABAST-',v.n),@donacion,N'Concentrado de eritrocitos',DATEADD(DAY,v.vence,@hoy),v.grupo,v.rh,v.estado,@usuario
FROM (VALUES(1,0,'A','+','LIBERADA'),(2,7,'A','+','LIBERADA'),(3,8,'A','+','LIBERADA'),(4,-1,'A','+','LIBERADA'),
 (5,4,'A','+','BLOQUEADA'),(6,4,'A','+','RESERVADA'),(7,4,'A','+','EN_REVISION'),(8,4,'A','+','ENTREGADA'),
 (9,30,'A','+','LIBERADA'),(10,20,'O','-','LIBERADA'),(11,7,'B','+','LIBERADA'),
 (13,3,NULL,NULL,'LIBERADA'),(14,3,'A',NULL,'LIBERADA'),(15,4,'A','+','TRANSFUNDIDA'),(16,4,'A','+','DEVUELTA'),(17,4,'A','+','DESCARTADA')) v(n,vence,grupo,rh,estado);
INSERT dbo.Donacion(id_donante,fecha_donacion,cantidad_unidades,tipo_componente,fecha_vencimiento,id_usuario_responsable)
VALUES(@donante,DATEADD(DAY,-60,@hoy),1,N'Plasma fresco congelado',DATEADD(DAY,90,@hoy),@usuario);
INSERT dbo.Unidad(codigo_identificacion,id_donacion,tipo_componente,fecha_vencimiento,grupo_sanguineo,factor_rh,estado_operativo,id_usuario_ultima_modificacion)
VALUES('ABAST-12',SCOPE_IDENTITY(),N'Plasma fresco congelado',DATEADD(DAY,3,@hoy),'AB','-','LIBERADA',@usuario);
INSERT dbo.SolicitudTransfusional(paciente_receptor,tipo_componente,cantidad_requerida,id_usuario_solicitante,fecha_solicitud,documento_receptor,grupo_receptor,rh_receptor,
 laboratorio_estudio,referencia_estudio,fecha_estudio,resultado_anticuerpos,id_usuario_validacion,fecha_validacion,nombre_solicitante)
SELECT v.nombre,v.componente,v.cantidad,@usuario,DATEADD(SECOND,v.segundos,CONVERT(DATETIME,DATEADD(DAY,v.dia,@hoy))),v.documento,v.grupo,v.rh,
 CASE WHEN v.validada=1 THEN N'Lab' END,CASE WHEN v.validada=1 THEN N'Ref' END,CASE WHEN v.validada=1 THEN DATEADD(DAY,-2,@hoy) END,
 CASE WHEN v.validada=1 THEN N'Negativo' END,CASE WHEN v.validada=1 THEN @usuario END,CASE WHEN v.validada=1 THEN DATEADD(DAY,-1,@hoy) END,N'Profesional'
FROM (VALUES(N'A1',N'Concentrado de eritrocitos',6,-4,0,N'MISMO','A','+',1),
 (N'A2',N'Concentrado de eritrocitos',4,-1,3600,N'MISMO','A','+',1),
 (N'Anterior',N'Concentrado de eritrocitos',2,-40,0,N'ANT','A','+',1),
 (N'Sin validar',N'Concentrado de eritrocitos',3,-1,0,N'NV','A','+',0),
 (N'Sin grupo',N'Concentrado de eritrocitos',2,-1,0,N'NG',NULL,NULL,0),
 (N'Limite inicio',N'Plasma fresco congelado',2,-10,0,N'L1','AB','-',1),
 (N'Limite fin',N'Plasma fresco congelado',3,-1,86399,N'L2','AB','-',1),
 (N'Futura',N'Concentrado de eritrocitos',100,1,0,N'FUT','A','+',1)) v(nombre,componente,cantidad,dia,segundos,documento,grupo,rh,validada);
DECLARE @a1 INT=(SELECT id_solicitud_transfusional FROM dbo.SolicitudTransfusional WHERE paciente_receptor=N'A1'),
 @a2 INT=(SELECT id_solicitud_transfusional FROM dbo.SolicitudTransfusional WHERE paciente_receptor=N'A2');
INSERT dbo.Reserva(id_solicitud_transfusional,id_unidad,estado_reserva,id_usuario_responsable)
SELECT CASE WHEN v.n IN(5,9) THEN @a2 ELSE @a1 END,u.id_unidad,v.estado,@usuario
FROM (VALUES(6,'ACTIVA'),(9,'ACTIVA'),(5,'CANCELADA'),(8,'ENTREGADA'),(15,'ENTREGADA'),(16,'ENTREGADA'),(17,'ENTREGADA')) v(n,estado)
JOIN dbo.Unidad u ON u.codigo_identificacion=CONCAT('ABAST-',v.n);
INSERT dbo.Entrega(id_reserva,id_usuario_responsable,destino,fecha_destino,motivo_destino,id_usuario_destino)
SELECT r.id_reserva,@usuario,CASE u.codigo_identificacion WHEN 'ABAST-15' THEN 'TRANSFUNDIDA' WHEN 'ABAST-16' THEN 'DEVUELTA' WHEN 'ABAST-17' THEN 'DESCARTADA' END,
 CASE WHEN u.codigo_identificacion<>'ABAST-8' THEN GETDATE() END,
 CASE WHEN u.codigo_identificacion<>'ABAST-8' THEN N'Destino de prueba' END,
 CASE WHEN u.codigo_identificacion<>'ABAST-8' THEN @usuario END
FROM dbo.Reserva r JOIN dbo.Unidad u ON u.id_unidad=r.id_unidad WHERE r.estado_reserva='ENTREGADA';
EXEC dbo.sp_IntegridadPN1_Sellar @usuario,'BASELINE';
COMMIT;
'@)
    $taskUser = [int](Invoke-AbastSql "SELECT id_usuario FROM dbo.Usuario WHERE nombre_usuario='abast_reporter'")
    $taskDenied = [int](Invoke-AbastSql "SELECT id_usuario FROM dbo.Usuario WHERE nombre_usuario='abast_denied'")
    $taskPrefix = "DECLARE @hoy DATE=CONVERT(DATE,GETDATE()); DECLARE @desde DATE=DATEADD(DAY,-10,@hoy),@hasta DATE=DATEADD(DAY,-1,@hoy); EXEC dbo.sp_PN2_Abastecimiento_Listar @desde=@desde,@hasta=@hasta,@horizonte_dias=7,@id_usuario=$taskUser"
    $taskAuditCount = [int](Invoke-AbastSql 'SELECT COUNT(*) FROM dbo.Auditoria')
    $taskTable = Invoke-AbastSql $taskPrefix -Table
    Assert-Abast ($taskTable.Rows.Count -eq 5) 'Cinco cohortes sin multiplicacion por joins'
    $taskA = @($taskTable.Rows | Where-Object { $_.tipo_componente -eq 'Concentrado de eritrocitos' -and $_.grupo_sanguineo -eq 'A' -and $_.factor_rh -eq '+' })[0]
    Assert-Abast ($taskA.stock_liberado_vigente -eq 3) 'Stock excluye vencidas, reservas activas, estados no liberados y clasificacion incompleta'
    Assert-Abast ($taskA.stock_por_vencer -eq 2) 'Vencimiento incluye hoy y limite exacto, excluye dia posterior'
    Assert-Abast ($taskA.demanda_periodo -eq 10) 'Cada solicitud cuenta una vez incluso para receptor repetido'
    Assert-Abast ($taskA.pendiente_vigente -eq 8) 'Saldo actual incluye anteriores, resta reservas y entregas efectivas; devoluciones y descartes reabren saldo'
    Assert-Abast ($taskA.demanda_diaria_promedio -eq 1 -and $taskA.cobertura_dias -eq 3) 'Promedio y cobertura por diez dias inclusivos'
    Assert-Abast ($taskA.deficit_unidades -eq 5 -and $taskA.prioridad -eq 'REPOSICION_Y_VENCIMIENTO') 'Deficit y prioridad combinada exactos'
    $taskUnknown = @($taskTable.Rows | Where-Object { !$_.cohorte_validada })[0]
    Assert-Abast ($taskUnknown.demanda_periodo -eq 5 -and $taskUnknown.pendiente_vigente -eq 5) 'Solicitudes sin estudio agrupadas solo por componente'
    Assert-Abast ($taskUnknown.stock_liberado_vigente -is [DBNull] -and $taskUnknown.deficit_unidades -is [DBNull] -and $taskUnknown.cobertura_dias -is [DBNull] -and $taskUnknown.prioridad -eq 'VALIDAR_ESTUDIO') 'Sin estudio no se asignan stock, cobertura ni deficit'
    $taskO = @($taskTable.Rows | Where-Object { $_.grupo_sanguineo -eq 'O' })[0]
    Assert-Abast ($taskO.stock_liberado_vigente -eq 1 -and $taskO.demanda_periodo -eq 0 -and $taskO.cobertura_dias -is [DBNull]) 'Sin demanda la cobertura es nula'
    $taskPlasma = @($taskTable.Rows | Where-Object { $_.tipo_componente -eq 'Plasma fresco congelado' })[0]
    Assert-Abast ($taskPlasma.demanda_periodo -eq 5 -and $taskPlasma.stock_liberado_vigente -eq 1) 'Limites inicio y fin incluyen todo el dia y separan componentes'
    $taskFilteredSql = $taskPrefix + ",@tipo_componente=N'Concentrado de eritrocitos',@grupo_sanguineo='A',@factor_rh='+'"
    $taskFiltered = Invoke-AbastSql $taskFilteredSql -Table
    Assert-Abast ($taskFiltered.Rows.Count -eq 1 -and $taskFiltered.Rows[0].demanda_periodo -eq 10) 'Filtros exactos sin compatibilidad cruzada'
    Assert-Abast ([int](Invoke-AbastSql 'SELECT COUNT(*) FROM dbo.Auditoria') -eq $taskAuditCount -and [int](Invoke-AbastSql 'SELECT COUNT(*) FROM dbo.v_IntegridadPN1_Errores') -eq 0) 'Consulta conserva datos y sellos'
    Assert-AbastError ($taskPrefix.Replace("@id_usuario=$taskUser", "@id_usuario=$taskDenied")) 51003 'Denegacion de usuario sin REPORTE_VER'
    Assert-AbastError ($taskPrefix.Replace('@horizonte_dias=7','@horizonte_dias=0')) 51011 'Rechaza horizonte cero'
    Assert-AbastError ($taskPrefix.Replace('@horizonte_dias=7','@horizonte_dias=366')) 51011 'Rechaza horizonte mayor a365'
    Assert-AbastError ($taskPrefix.Replace('@hasta=@hasta','@hasta=@desde').Replace('@desde=@desde','@desde=@hasta')) 51011 'Rechaza fechas invertidas'
    Assert-AbastError ($taskPrefix.Replace('@hasta DATE=DATEADD(DAY,-1,@hoy)','@hasta DATE=DATEADD(DAY,1,@hoy)')) 51011 'Rechaza periodo futuro'
    Assert-Abast ([int](Invoke-AbastSql "SELECT COUNT(*) FROM dbo.Traduccion t JOIN dbo.Etiqueta e ON e.id_etiqueta=t.id_etiqueta JOIN dbo.Idioma i ON i.id_idioma=t.id_idioma WHERE e.clave LIKE 'SUPPLY[_]%' AND i.codigo IN ('es-AR','en-US')") -eq 90) 'Cuarenta ycinco claves completas enes yen'
    [void](Invoke-AbastSql "UPDATE t SET texto=N'Texto personalizado' FROM dbo.Traduccion t JOIN dbo.Etiqueta e ON e.id_etiqueta=t.id_etiqueta JOIN dbo.Idioma i ON i.id_idioma=t.id_idioma WHERE e.clave='SUPPLY_CHART_HELP' AND i.codigo='es-AR'")
    $taskI18n = [IO.File]::ReadAllText((Join-Path $taskRoot 'Database/StoredProcedures/15a_AbastecimientoUI.sql')).Replace('USE [HemovidaGest];','')
    foreach ($taskBatch in [regex]::Split($taskI18n,'(?im)^\s*GO\s*(?:--[^\r\n]*)?$')) { if ($taskBatch.Trim()) { [void](Invoke-AbastSql $taskBatch) } }
    Assert-Abast ((Invoke-AbastSql "SELECT t.texto FROM dbo.Traduccion t JOIN dbo.Etiqueta e ON e.id_etiqueta=t.id_etiqueta JOIN dbo.Idioma i ON i.id_idioma=t.id_idioma WHERE e.clave='SUPPLY_CHART_HELP' AND i.codigo='es-AR'") -eq 'Texto personalizado') 'Reinstalar traducciones conserva personalizaciones'
    $taskTexts = Get-Content -Raw -Encoding UTF8 -LiteralPath (Join-Path $PSScriptRoot 'Abastecimiento.Traducciones.json') | ConvertFrom-Json
    $taskResetTranslation = $taskConnection.CreateCommand()
    try {
        $taskResetTranslation.CommandText="UPDATE t SET texto=@texto FROM dbo.Traduccion t JOIN dbo.Etiqueta e ON e.id_etiqueta=t.id_etiqueta JOIN dbo.Idioma i ON i.id_idioma=t.id_idioma WHERE e.clave='SUPPLY_CHART_HELP' AND i.codigo='es-AR'"
        [void]$taskResetTranslation.Parameters.Add('@texto',[System.Data.SqlDbType]::NVarChar,500)
        $taskResetTranslation.Parameters['@texto'].Value=$taskTexts.'es-AR'.SUPPLY_CHART_HELP
        [void]$taskResetTranslation.ExecuteNonQuery()
    } finally { $taskResetTranslation.Dispose() }
    if ($UiRunner) {
        if (!$UiCapturas) { throw 'Indica UiCapturas para el runner de interfaz.' }
        $taskUiProcess = Start-Process -FilePath $UiRunner -ArgumentList @('--sql', $Servidor, $taskDatabase, "$taskUser", ('"'+[IO.Path]::GetFullPath($UiCapturas)+'"')) -WindowStyle Hidden -Wait -PassThru -RedirectStandardOutput (Join-Path $UiCapturas 'integration.log') -RedirectStandardError (Join-Path $UiCapturas 'integration.err')
        Assert-Abast ($taskUiProcess.ExitCode -eq 0) 'Cadena realApplication Repository DAL SQL einterfaz confixture aislado'
    }
    [void](Invoke-AbastSql "UPDATE dbo.Unidad SET observaciones=N'cambio sin sello' WHERE codigo_identificacion='ABAST-1'")
    Assert-AbastError $taskPrefix 51001 'Denegacion por integridad alterada'
    $taskReport = [ordered]@{ status='passed'; database=$taskDatabase; isolatedFreshDatabase=$true; scriptsInstalled=$taskScripts; checks=$taskPassed.ToArray(); checkCount=$taskPassed.Count; privateConfigurationRead=$false }
    if ($Salida) { $taskReport | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $Salida -Encoding UTF8 }
    $taskReport | ConvertTo-Json -Depth 5
} finally {
    if ($taskConnection) { $taskConnection.Dispose() }
    if ($taskCreated) {
        if ($taskDatabase -notmatch '^CodexAbast_[a-f0-9]{12}$') { throw 'Se rechazo limpiar una base ajena a las pruebas.' }
        $taskDrop = $taskMaster.CreateCommand()
        try { $taskDrop.CommandText="ALTER DATABASE [$taskDatabase] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$taskDatabase]"; [void]$taskDrop.ExecuteNonQuery() }
        finally { $taskDrop.Dispose() }
    }
    $taskMaster.Dispose()
}
