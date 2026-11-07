param([string]$Config = (Join-Path $PSScriptRoot '../DAL/App.local.config'), [switch]$PN2, [switch]$Legacy, [switch]$UI)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
[xml]$taskConfig = Get-Content -Raw -LiteralPath $Config
$taskBuilder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $taskConfig.configuration.connectionStrings.add.connectionString
$taskDatabase = 'CodexDonanteTest_' + [Guid]::NewGuid().ToString('N').Substring(0,12)
$taskBuilder['Initial Catalog'] = 'master'
$taskAdmin = New-Object System.Data.SqlClient.SqlConnection $taskBuilder.ConnectionString
$taskConnection = $null
function Sql([string]$Text) {
    $taskCommand = $taskConnection.CreateCommand()
    $taskCommand.CommandText = $Text
    $taskCommand.CommandTimeout = 120
    $taskData = New-Object System.Data.DataSet
    $taskAdapter = New-Object System.Data.SqlClient.SqlDataAdapter $taskCommand
    try { [void]$taskAdapter.Fill($taskData); return ,$taskData }
    finally { $taskAdapter.Dispose(); $taskCommand.Dispose() }
}
function Value([string]$Text) { return (Sql $Text).Tables[0].Rows[0][0] }
function Assert($Actual, $Expected, [string]$Description) {
    if ($Actual -ne $Expected) { throw "$Description. Esperado: $Expected; recibido: $Actual" }
    Write-Host "OK $Description"
}
function Script([string]$File) {
    $taskText = (Get-Content -Raw -Encoding UTF8 -LiteralPath (Join-Path $taskRoot ('Database/' + $File))).Replace('HemovidaGest',$taskDatabase)
    foreach ($taskBatch in [regex]::Split($taskText,'(?im)^\s*GO\s*(?:--[^\r\n]*)?$')) {
        if ($taskBatch.Trim()) { [void](Sql $taskBatch) }
    }
}
function Donate([int]$Donor) {
    Assert (Value "DECLARE @f DATETIME=GETDATE(),@v DATE=DATEADD(day,30,GETDATE()); EXEC dbo.sp_Donacion_Registrar $Donor,@f,1,'Concentrado de eritrocitos',@v,NULL,$taskUser") 'OK' 'Registrar donacion'
    return [int](Value 'SELECT MAX(id_unidad) FROM dbo.Unidad')
}
function Classify([int]$Unit, [string]$Group, [string]$Rh) {
    return Value "DECLARE @v DATE=DATEADD(day,30,GETDATE()); EXEC dbo.sp_Unidad_Clasificar $Unit,'Concentrado de eritrocitos','$Group','$Rh',@v,NULL,$taskUser"
}
try {
    $taskAdmin.Open()
    $taskCommand = $taskAdmin.CreateCommand()
    $taskCommand.CommandText = "CREATE DATABASE [$taskDatabase]"
    [void]$taskCommand.ExecuteNonQuery(); $taskCommand.Dispose()
    $taskBuilder['Initial Catalog'] = $taskDatabase
    $taskConnection = New-Object System.Data.SqlClient.SqlConnection $taskBuilder.ConnectionString
    $taskConnection.Open()
    foreach ($taskFile in @('00_tecni_salud_schema.sql','StoredProcedures/02_Bitacora.sql','StoredProcedures/04_Permisos.sql',
        'StoredProcedures/05_IntegridadPN1.sql','StoredProcedures/06_MaestrosPN1.sql','StoredProcedures/07_PN1.sql')) { Script $taskFile }
    if ($PN2) {
        foreach ($taskFile in @('08_PN2_Catalogos.sql','09_PN2.sql','10_IntegridadPN2.sql')) { Script ('StoredProcedures/' + $taskFile) }
    }
    $taskUser = [int](Value "SELECT id_usuario FROM dbo.Usuario WHERE nombre_usuario='admin'")
    if ($Legacy) {
        # Reproduce las definiciones previas, sin columnas ni campos JSON del donante.
        $taskOldJson = '(SELECT d.id_donante, d.documento, d.nombre, d.apellido, d.fecha_nacimiento, d.telefono, d.email, d.domicilio, d.estado_donante, d.fecha_alta, d.id_usuario_alta FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)'
        $taskNewJson = "JSON_MODIFY(JSON_MODIFY($taskOldJson, '$.grupo_sanguineo', d.grupo_sanguineo), '$.factor_rh', d.factor_rh)"
        $taskDefinition = [string](Value "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.v_IntegridadPN1_Datos'))")
        [void](Sql ([regex]::Replace($taskDefinition.Replace($taskNewJson,$taskOldJson), '(?i)\bCREATE\s+(?:OR\s+ALTER\s+)?VIEW', 'ALTER VIEW')))
        $taskDefinition = [string](Value "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.sp_IntegridadPN1_Restaurar'))")
        $taskDefinition = $taskDefinition.Replace('grupo_sanguineo=j.grupo_sanguineo,factor_rh=j.factor_rh,','').Replace('Donante(grupo_sanguineo,factor_rh,','Donante(').Replace('SELECT j.grupo_sanguineo,j.factor_rh,j.id_donante','SELECT j.id_donante').Replace("WITH(grupo_sanguineo varchar(5) '$.grupo_sanguineo',factor_rh varchar(5) '$.factor_rh',id_donante","WITH(id_donante")
        [void](Sql ([regex]::Replace($taskDefinition, '(?i)\bCREATE\s+(?:OR\s+ALTER\s+)?PROCEDURE', 'ALTER PROCEDURE')))
        [void](Sql 'ALTER TABLE dbo.Donante DROP CONSTRAINT CK_Donante_Tipificacion, FK_Donante_Grupo, FK_Donante_Rh; ALTER TABLE dbo.Donante DROP COLUMN grupo_sanguineo, factor_rh;')
    }
    [void](Sql @"
BEGIN TRANSACTION; EXEC dbo.sp_IntegridadPN1_Exigir;
INSERT dbo.Donante(documento,nombre,apellido,id_usuario_alta) VALUES('HISTORICO','Ana','Historial',$taskUser),('CONFLICTIVO','Luis','Historial',$taskUser),('DESCONOCIDO','Eva','Historial',$taskUser);
INSERT dbo.Donacion(id_donante,fecha_donacion,cantidad_unidades,tipo_componente,fecha_vencimiento,id_usuario_responsable)
SELECT id_donante,GETDATE(),2,'Concentrado de eritrocitos',DATEADD(day,30,GETDATE()),$taskUser FROM dbo.Donante WHERE documento IN ('HISTORICO','CONFLICTIVO');
INSERT dbo.Unidad(codigo_identificacion,id_donacion,tipo_componente,fecha_vencimiento,grupo_sanguineo,factor_rh,id_usuario_ultima_modificacion)
SELECT CONCAT(d.documento,'-',n.n),dn.id_donacion,'Concentrado de eritrocitos',dn.fecha_vencimiento,CASE WHEN d.documento='CONFLICTIVO' AND n.n=2 THEN 'B' ELSE 'A' END,'+',$taskUser
FROM dbo.Donacion dn JOIN dbo.Donante d ON d.id_donante=dn.id_donante CROSS JOIN (VALUES(1),(2)) n(n);
EXEC dbo.sp_IntegridadPN1_Sellar $taskUser; COMMIT;
"@)
    $taskOldAudit = [int](Value 'SELECT MAX(id_auditoria) FROM dbo.Auditoria')
    $taskOldHash = Value "SELECT dvh FROM dbo.Donante WHERE documento='DESCONOCIDO'"
    $taskAuditHashSql = "SELECT CONVERT(VARCHAR(64),HASHBYTES('SHA2_256',(SELECT * FROM dbo.Auditoria WHERE id_auditoria <= $taskOldAudit ORDER BY id_auditoria FOR JSON PATH)),2)"
    $taskOldAuditHash = Value $taskAuditHashSql
    Script 'StoredProcedures/06b_TipificacionDonante.sql'
    Script 'StoredProcedures/07_PN1.sql'
    Assert (Value "SELECT CONCAT(grupo_sanguineo,factor_rh) FROM dbo.Donante WHERE documento='HISTORICO'") 'A+' 'Migrar antecedentes coincidentes'
    Assert (Value "SELECT COUNT(*) FROM dbo.Donante WHERE documento='CONFLICTIVO' AND grupo_sanguineo IS NULL AND factor_rh IS NULL") 1 'No inferir antecedentes contradictorios'
    Assert (Value "SELECT dvh FROM dbo.Donante WHERE documento='DESCONOCIDO'") $taskOldHash 'Conservar hash del donante sin antecedentes'
    Assert (Value $taskAuditHashSql) $taskOldAuditHash 'Conservar auditorias anteriores sin reescribirlas'
    $taskAuditCount = Value 'SELECT COUNT(*) FROM dbo.Auditoria'
    Script 'StoredProcedures/06b_TipificacionDonante.sql'
    Assert (Value 'SELECT COUNT(*) FROM dbo.Auditoria') $taskAuditCount 'Migracion idempotente'
    Assert (Value "SELECT COUNT(*) FROM dbo.Traduccion t JOIN dbo.Etiqueta e ON e.id_etiqueta=t.id_etiqueta JOIN dbo.Idioma i ON i.id_idioma=t.id_idioma WHERE e.clave='UNIT_DONOR_BLOOD_CONFLICT' AND i.codigo IN ('es-AR','en-US')") 2 'Mensaje de conflicto disponible en ambos idiomas'
    if ($PN2) { Script 'StoredProcedures/10_IntegridadPN2.sql' }
    Assert (Value 'SELECT COUNT(*) FROM dbo.v_IntegridadPN1_Errores') 0 'Integridad tras migrar'
    $taskDonor = [int](Value "SELECT id_donante FROM dbo.Donante WHERE documento='DESCONOCIDO'")
    $taskUnit = Donate $taskDonor
    Assert (Classify $taskUnit 'A' '+') 'OK' 'Primera clasificacion'
    Assert (Value "SELECT CONCAT(grupo_sanguineo,factor_rh) FROM dbo.Donante WHERE id_donante=$taskDonor") 'A+' 'Guardar tipificacion en el donante'
    $taskNextUnit = Donate $taskDonor
    Assert (Value "SELECT COUNT(*) FROM dbo.Unidad WHERE id_unidad=$taskNextUnit AND grupo_sanguineo IS NULL AND factor_rh IS NULL AND estado_operativo='EN_REVISION'") 1 'Nueva muestra requiere clasificacion'
    $taskListed = (Sql 'EXEC dbo.sp_Unidad_Listar').Tables[0].Select("id_unidad=$taskNextUnit")[0]
    Assert ($taskListed.grupo_sanguineo_donante + $taskListed.factor_rh_donante) 'A+' 'Proponer datos del donante en nuevas muestras'
    $taskDonorListed = (Sql 'EXEC dbo.sp_Donante_Listar').Tables[0].Select("id_donante=$taskDonor")[0]
    Assert ($taskDonorListed.grupo_sanguineo + $taskDonorListed.factor_rh) 'A+' 'Consultar tipificacion en donantes'
    Assert (Value "EXEC dbo.sp_Unidad_CambiarEstado $taskNextUnit,'LIBERADA',NULL,$taskUser") 'CONFLICTO_ESTADO' 'No liberar muestra por antecedentes solamente'
    $taskAuditCount = Value 'SELECT COUNT(*) FROM dbo.Auditoria'
    Assert (Classify $taskNextUnit 'B' '+') 'CONFLICTO_TIPIFICACION' 'Rechazar grupo contradictorio'
    Assert (Classify $taskNextUnit 'A' '-') 'CONFLICTO_TIPIFICACION' 'Rechazar Rh contradictorio'
    Assert (Value 'SELECT COUNT(*) FROM dbo.Auditoria') $taskAuditCount 'Rechazo sin cambios ni auditorias de datos'
    Assert (Value "SELECT COUNT(*) FROM dbo.Unidad WHERE id_unidad=$taskNextUnit AND grupo_sanguineo IS NULL AND factor_rh IS NULL") 1 'Rechazo sin modificar unidad'
    Assert (Classify $taskNextUnit 'A' '+') 'OK' 'Confirmar nueva muestra coincidente'
    Assert (Value "EXEC dbo.sp_Donante_Actualizar @id_donante=$taskDonor,@documento='DESCONOCIDO',@nombre='Eva editada',@apellido='Historial',@id_usuario=$taskUser") 'OK' 'Editar datos administrativos'
    Assert (Value "SELECT CONCAT(grupo_sanguineo,factor_rh) FROM dbo.Donante WHERE id_donante=$taskDonor") 'A+' 'Edicion conserva tipificacion'
    $taskConflictUnit = [int](Value "SELECT MIN(u.id_unidad) FROM dbo.Unidad u JOIN dbo.Donacion dn ON dn.id_donacion=u.id_donacion JOIN dbo.Donante d ON d.id_donante=dn.id_donante WHERE d.documento='CONFLICTIVO'")
    Assert (Classify $taskConflictUnit 'A' '+') 'CONFLICTO_TIPIFICACION' 'Bloquear historial contradictorio legado'
    Assert (Value "EXEC dbo.sp_Donante_Registrar 'ROLLBACK','Ana','Rollback',NULL,NULL,NULL,NULL,$taskUser") 'OK' 'Registrar donante sin tipificacion'
    $taskRollbackDonor = [int](Value "SELECT id_donante FROM dbo.Donante WHERE documento='ROLLBACK'")
    $taskRollbackUnit = Donate $taskRollbackDonor
    Assert (Classify $taskRollbackUnit 'Z' '+') 'DATOS_INVALIDOS' 'Rechazar grupo invalido'
    [void](Sql "CREATE TRIGGER dbo.tr_Tipificacion_Test ON dbo.MovimientoUnidad AFTER INSERT AS BEGIN IF EXISTS(SELECT 1 FROM inserted WHERE tipo_movimiento='CLASIFICACION') THROW 51199,'TEST_ROLLBACK',1; END;")
    $taskRejected = $false
    try { [void](Classify $taskRollbackUnit 'O' '-') } catch { $taskRejected = $true }
    [void](Sql 'DROP TRIGGER dbo.tr_Tipificacion_Test')
    Assert $taskRejected $true 'Simular falla posterior a la escritura del donante'
    Assert (Value "SELECT COUNT(*) FROM dbo.Donante WHERE id_donante=$taskRollbackDonor AND grupo_sanguineo IS NULL AND factor_rh IS NULL") 1 'Revertir donante ante falla de clasificacion'
    Assert (Value "SELECT COUNT(*) FROM dbo.Unidad WHERE id_unidad=$taskRollbackUnit AND grupo_sanguineo IS NULL AND factor_rh IS NULL") 1 'Revertir unidad ante falla de clasificacion'
    [void](Sql "UPDATE dbo.Donante SET grupo_sanguineo='B' WHERE id_donante=$taskDonor")
    Assert ([int](Value 'SELECT COUNT(*) FROM dbo.v_IntegridadPN1_Errores') -gt 0) $true 'Detectar alteracion de tipificacion'
    $taskRejected = $false
    try { Script 'StoredProcedures/06b_TipificacionDonante.sql' } catch { $taskRejected = $true }
    Assert $taskRejected $true 'Migracion rechaza datos con integridad alterada'
    Assert (Value 'SELECT @@TRANCOUNT') 0 'Rechazo sin transacciones abiertas'
    $taskRevision = (Sql "EXEC dbo.sp_IntegridadPN1_Verificar @id_usuario=$taskUser").Tables[0].Rows[0].revision
    [void](Sql "EXEC dbo.sp_IntegridadPN1_Restaurar @id_usuario=$taskUser,@revision='$taskRevision'")
    Assert (Value "SELECT CONCAT(grupo_sanguineo,factor_rh) FROM dbo.Donante WHERE id_donante=$taskDonor") 'A+' 'Restaurar tipificacion desde historial'
    Assert (Value 'SELECT COUNT(*) FROM dbo.v_IntegridadPN1_Errores') 0 'Integridad final'
    if ($UI) {
        & (Join-Path $PSScriptRoot 'bin/Release/TipificacionDonante.Ui.Tests.exe') $Config $taskDatabase
        if ($LASTEXITCODE -ne 0) { throw 'Fallo el circuito UI/Application/DAL de tipificacion.' }
    }
    Write-Host "Tipificacion de donantes correcta. PN2=$PN2 Legacy=$Legacy"
} finally {
    if ($taskConnection) { $taskConnection.Dispose() }
    if ($taskAdmin.State -eq 'Open' -and $taskDatabase -match '^CodexDonanteTest_[0-9a-f]{12}$') {
        $taskCommand = $taskAdmin.CreateCommand()
        $taskCommand.CommandText = "IF DB_ID('$taskDatabase') IS NOT NULL BEGIN ALTER DATABASE [$taskDatabase] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$taskDatabase]; END"
        try { [void]$taskCommand.ExecuteNonQuery() } finally { $taskCommand.Dispose() }
    }
    $taskAdmin.Dispose()
}
