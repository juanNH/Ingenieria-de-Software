param([string]$Servidor, [string]$Base)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$taskConfigPath = Join-Path $taskRoot 'DAL/App.local.config'
if (!(Test-Path -LiteralPath $taskConfigPath)) { throw 'Configurá DAL/App.local.config antes de instalar PN2.' }
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
    $taskCommand.CommandText = "SELECT CASE WHEN OBJECT_ID('dbo.v_IntegridadPN1_Errores','V') IS NOT NULL THEN 1 ELSE 0 END"
    if ([int]$taskCommand.ExecuteScalar() -ne 1) { throw 'Primero instalá IntegridadPN1.sql y PN1.sql.' }
    $taskCommand.CommandText = 'SELECT COUNT(*) FROM dbo.v_IntegridadPN1_Errores'
    if ([int]$taskCommand.ExecuteScalar() -ne 0) { throw 'La integridad actual tiene inconsistencias. Revisá Integridad antes de instalar PN2.' }
    $taskCommand.CommandText = "SELECT CONVERT(NVARCHAR(4000),SERVERPROPERTY('InstanceDefaultBackupPath'))"
    $taskBackupDirectory = [string]$taskCommand.ExecuteScalar()
    if (!$taskBackupDirectory) { throw 'SQL Server no informó una carpeta de backups.' }
    $taskBackupName = $taskDatabase + '_AntesPN2_' + [DateTime]::UtcNow.ToString('yyyyMMdd_HHmmss') + '_' + [Guid]::NewGuid().ToString('N') + '.bak'
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
    foreach ($taskScript in @('PN2_Catalogos.sql','PN1.sql','PN2.sql','IntegridadPN2.sql','ComprobantesAutomaticos.sql')) {
        $taskScriptPath = Join-Path $PSScriptRoot ('StoredProcedures/' + $taskScript)
        $taskSql = (Get-Content -Raw -LiteralPath $taskScriptPath).Replace('USE [HemovidaGest];','')
        foreach ($taskBatch in [regex]::Split($taskSql,'(?im)^\s*GO\s*(?:--[^\r\n]*)?$')) {
            if (!$taskBatch.Trim()) { continue }
            $taskCommand = $taskConnection.CreateCommand()
            $taskCommand.Transaction = $taskTransaction
            $taskCommand.CommandTimeout = 180
            $taskCommand.CommandText = $taskBatch
            try { [void]$taskCommand.ExecuteNonQuery() }
            finally { $taskCommand.Dispose() }
        }
        Write-Host "Aplicado: $taskScript"
    }
    $taskCommand = $taskConnection.CreateCommand()
    $taskCommand.Transaction = $taskTransaction
    $taskCommand.CommandText = 'SELECT COUNT(*) FROM dbo.v_IntegridadPN1_Errores'
    try {
        if ([int]$taskCommand.ExecuteScalar() -ne 0) { throw 'La actualización no conserva la integridad. Se revierte la transacción.' }
    } finally { $taskCommand.Dispose() }
    $taskTransaction.Commit()
    $taskTransaction.Dispose()
    $taskTransaction = $null
    Write-Host "PN2 instalado en $($taskBuilder.DataSource) / $taskDatabase. Reiniciá la aplicación o volvé a iniciar sesión para cargar los permisos."
} catch {
    if ($taskTransaction) { try { $taskTransaction.Rollback() } catch {} }
    throw
} finally {
    if ($taskTransaction) { $taskTransaction.Dispose() }
    $taskConnection.Dispose()
}
