param([string]$Servidor, [string]$Base)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$configPath = Join-Path $root 'DAL/App.local.config'
if (!(Test-Path -LiteralPath $configPath)) { throw 'Configurá DAL/App.local.config antes de instalar comprobantes.' }
[xml]$config = Get-Content -Raw -LiteralPath $configPath
$builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $config.configuration.connectionStrings.add.connectionString
if ($Servidor) { $builder['Data Source'] = $Servidor }
if ($Base) { $builder['Initial Catalog'] = $Base }
$database = $builder.InitialCatalog
if ($database -notmatch '^[A-Za-z0-9_]+$' -or $database -in @('master','model','msdb','tempdb')) { throw 'La base de destino no es válida.' }
$connection = New-Object System.Data.SqlClient.SqlConnection $builder.ConnectionString
$transaction = $null
try {
    $connection.Open()
    $command = $connection.CreateCommand()
    $command.CommandTimeout = 180
    $command.CommandText = "SELECT CASE WHEN OBJECT_ID('dbo.v_IntegridadPN2_Instalada','V') IS NOT NULL THEN 1 ELSE 0 END"
    if ([int]$command.ExecuteScalar() -ne 1) { throw 'Primero instalá PN2 y su integridad.' }
    $command.CommandText = 'SELECT COUNT(*) FROM dbo.v_IntegridadPN1_Errores'
    if ([int]$command.ExecuteScalar() -ne 0) { throw 'La integridad presenta inconsistencias.' }
    $command.CommandText = "SELECT CONVERT(NVARCHAR(4000),SERVERPROPERTY('InstanceDefaultBackupPath'))"
    $backupDirectory = [string]$command.ExecuteScalar()
    if (!$backupDirectory) { throw 'SQL Server no informó una carpeta de respaldos.' }
    $backupPath = $backupDirectory.TrimEnd('\','/') + '\' + $database + '_AntesComprobantes_' + [DateTime]::UtcNow.ToString('yyyyMMdd_HHmmss') + '_' + [guid]::NewGuid().ToString('N') + '.bak'
    $command.CommandText = 'BACKUP DATABASE [' + $database + '] TO DISK=@ruta WITH COPY_ONLY,CHECKSUM'
    [void]$command.Parameters.Add('@ruta',[System.Data.SqlDbType]::NVarChar,4000)
    $command.Parameters['@ruta'].Value = $backupPath
    [void]$command.ExecuteNonQuery()
    $command.CommandText = 'RESTORE VERIFYONLY FROM DISK=@ruta WITH CHECKSUM'
    [void]$command.ExecuteNonQuery(); $command.Dispose()
    Write-Host "Respaldo verificado: $backupPath"
    $transaction = $connection.BeginTransaction()
    $sql = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'StoredProcedures/ComprobantesAutomaticos.sql')).Replace('USE [HemovidaGest];','')
    foreach ($batch in [regex]::Split($sql,'(?im)^\s*GO\s*(?:--[^\r\n]*)?$')) {
        if (!$batch.Trim()) { continue }
        $command = $connection.CreateCommand(); $command.Transaction = $transaction; $command.CommandTimeout = 180; $command.CommandText = $batch
        try { [void]$command.ExecuteNonQuery() } finally { $command.Dispose() }
    }
    $command = $connection.CreateCommand(); $command.Transaction = $transaction
    $command.CommandText = 'SELECT COUNT(*) FROM dbo.v_IntegridadPN1_Errores'
    try { if ([int]$command.ExecuteScalar() -ne 0) { throw 'La integridad no se conservó; se revierte la instalación.' } } finally { $command.Dispose() }
    $transaction.Commit(); $transaction.Dispose(); $transaction = $null
    Write-Host "Comprobantes automáticos instalados en $database."
} catch {
    if ($transaction) { try { $transaction.Rollback() } catch {} }
    throw
} finally {
    if ($transaction) { $transaction.Dispose() }
    $connection.Dispose()
}
