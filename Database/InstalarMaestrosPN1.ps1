param([string]$Servidor, [string]$Base)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$taskConfigPath = Join-Path $taskRoot 'DAL/App.local.config'
if (!(Test-Path -LiteralPath $taskConfigPath)) { throw 'Configurá DAL/App.local.config antes de instalar los maestros.' }
[xml]$taskConfig = Get-Content -Raw -LiteralPath $taskConfigPath
$taskBuilder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $taskConfig.configuration.connectionStrings.add.connectionString
if ($Servidor) { $taskBuilder['Data Source'] = $Servidor }
if ($Base) { $taskBuilder['Initial Catalog'] = $Base }
$taskDatabase = $taskBuilder.InitialCatalog
if ($taskDatabase -notmatch '^[A-Za-z0-9_]+$' -or $taskDatabase -in @('master','model','msdb','tempdb')) {
    throw 'La base de destino debe ser una base de aplicación con nombre válido.'
}
$taskConnection = New-Object System.Data.SqlClient.SqlConnection $taskBuilder.ConnectionString
$taskTransaction = $null
try {
    $taskConnection.Open()
    $taskCommand = $taskConnection.CreateCommand()
    $taskCommand.CommandText = "SELECT CASE WHEN OBJECT_ID('dbo.v_IntegridadPN1_Errores','V') IS NULL THEN -1 ELSE (SELECT COUNT(*) FROM dbo.v_IntegridadPN1_Errores) END"
    if ([int]$taskCommand.ExecuteScalar() -ne 0) { throw 'La integridad PN1 debe estar instalada y sin inconsistencias antes de la migración.' }
    $taskCommand.CommandText = "SELECT CASE WHEN OBJECT_ID('dbo.v_IntegridadPN2_Instalada','V') IS NULL THEN 0 ELSE 1 END"
    $taskHasPN2 = [int]$taskCommand.ExecuteScalar() -eq 1
    $taskCommand.CommandText = "SELECT CASE WHEN OBJECT_ID('dbo.ComprobantePendiente','U') IS NULL THEN 0 ELSE 1 END"
    $taskHasComprobantes = [int]$taskCommand.ExecuteScalar() -eq 1
    $taskCommand.CommandText = "SELECT CONVERT(NVARCHAR(4000),SERVERPROPERTY('InstanceDefaultBackupPath'))"
    $taskBackupDirectory = [string]$taskCommand.ExecuteScalar()
    if (!$taskBackupDirectory) { throw 'SQL Server no informó una carpeta de backups.' }
    $taskBackupName = $taskDatabase + '_AntesMaestros_' + [DateTime]::UtcNow.ToString('yyyyMMdd_HHmmss') + '_' + [Guid]::NewGuid().ToString('N') + '.bak'
    $taskBackupPath = $taskBackupDirectory.TrimEnd('\','/') + '\' + $taskBackupName
    $taskCommand.CommandTimeout = 180
    $taskCommand.CommandText = 'BACKUP DATABASE [' + $taskDatabase + '] TO DISK=@ruta WITH COPY_ONLY,CHECKSUM'
    [void]$taskCommand.Parameters.Add('@ruta',[System.Data.SqlDbType]::NVarChar,4000)
    $taskCommand.Parameters['@ruta'].Value = $taskBackupPath
    [void]$taskCommand.ExecuteNonQuery()
    $taskCommand.CommandText = 'RESTORE VERIFYONLY FROM DISK=@ruta WITH CHECKSUM'
    [void]$taskCommand.ExecuteNonQuery()
    $taskCommand.Dispose()
    Write-Host "Respaldo verificado: $taskBackupPath"
    $taskTransaction = $taskConnection.BeginTransaction()
    $taskScripts = @('06_MaestrosPN1.sql','06a_CompatibilidadTransfusional.sql','06b_TipificacionDonante.sql','07_PN1.sql')
    if ($taskHasPN2) { $taskScripts += '09_PN2.sql' }
    if ($taskHasPN2 -and $taskHasComprobantes) { $taskScripts += '11_ComprobantesAutomaticos.sql' }
    foreach ($taskScript in $taskScripts) {
        $taskScriptPath = Join-Path $PSScriptRoot ('StoredProcedures/' + $taskScript)
        $taskSql = (Get-Content -Raw -Encoding UTF8 -LiteralPath $taskScriptPath).Replace('USE [HemovidaGest];','')
        foreach ($taskBatch in [regex]::Split($taskSql,'(?im)^\s*GO\s*(?:--[^\r\n]*)?$')) {
            if (!$taskBatch.Trim()) { continue }
            $taskCommand = $taskConnection.CreateCommand()
            $taskCommand.Transaction = $taskTransaction
            $taskCommand.CommandTimeout = 180
            $taskCommand.CommandText = $taskBatch
            try { [void]$taskCommand.ExecuteNonQuery() } finally { $taskCommand.Dispose() }
        }
        Write-Host "Aplicado: $taskScript"
    }
    $taskCommand = $taskConnection.CreateCommand()
    $taskCommand.Transaction = $taskTransaction
    $taskCommand.CommandText = 'SELECT COUNT(*) FROM dbo.v_IntegridadPN1_Errores'
    try {
        if ([int]$taskCommand.ExecuteScalar() -ne 0) { throw 'La migración no conserva la integridad PN1.' }
        $taskCommand.CommandText = "EXEC dbo.sp_MaestroPN1_Listar 'RH',0"
        [void]$taskCommand.ExecuteNonQuery()
    } finally { $taskCommand.Dispose() }
    $taskTransaction.Commit()
    $taskTransaction.Dispose()
    $taskTransaction = $null
    Write-Host "Maestros instalados en $($taskBuilder.DataSource) / $taskDatabase. Reiniciá la aplicación para cargar los permisos."
} catch {
    if ($taskTransaction) { try { $taskTransaction.Rollback() } catch {} }
    throw
} finally {
    if ($taskTransaction) { $taskTransaction.Dispose() }
    $taskConnection.Dispose()
}
