param([string]$Servidor, [string]$Base)
$ErrorActionPreference = 'Stop'
$taskConfigPath = Join-Path (Split-Path $PSScriptRoot -Parent) 'DAL/App.local.config'
if (!(Test-Path -LiteralPath $taskConfigPath)) { throw 'Configurá DAL/App.local.config antes de instalar compatibilidades.' }
[xml]$taskConfig = Get-Content -Raw -LiteralPath $taskConfigPath
$taskBuilder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $taskConfig.configuration.connectionStrings.add.connectionString
if ($Servidor) { $taskBuilder['Data Source'] = $Servidor }
if ($Base) { $taskBuilder['Initial Catalog'] = $Base }
$taskDatabase = $taskBuilder.InitialCatalog
if ($taskDatabase -notmatch '^[A-Za-z0-9_]+$' -or $taskDatabase -in @('master','model','msdb','tempdb')) {
    throw 'La base de destino debe ser una base de aplicación con nombre válido.'
}
function Batches([string]$Nombre) {
    $taskSql = [IO.File]::ReadAllText((Join-Path $PSScriptRoot ('StoredProcedures/' + $Nombre))).Replace('USE [HemovidaGest];','')
    return [regex]::Split($taskSql,'(?im)^\s*GO\s*(?:--[^\r\n]*)?$') | Where-Object { $_.Trim() }
}
# Seleccionar las tres definiciones mantiene una sola fuente para instalación
# nueva y actualización. No se vuelven a ejecutar otras migraciones de PN2.
$taskPN2 = @(Batches '09_PN2.sql' | Where-Object {
    $_ -match '(?im)^CREATE OR ALTER (FUNCTION dbo\.fn_PN2_GrupoCandidato|PROCEDURE dbo\.sp_PN2_Exigir|PROCEDURE dbo\.sp_PN2_UnidadesCandidatas)\b'
})
if ($taskPN2.Count -ne 3) { throw 'No se encontraron las tres definiciones de compatibilidad PN2.' }
$taskBatches = @(Batches '06a_CompatibilidadTransfusional.sql') + $taskPN2
$taskConnection = New-Object System.Data.SqlClient.SqlConnection $taskBuilder.ConnectionString
$taskTransaction = $null
function Sql([string]$Texto) {
    $taskCommand = $taskConnection.CreateCommand()
    $taskCommand.CommandTimeout = 180; $taskCommand.CommandText = $Texto
    if ($taskTransaction) { $taskCommand.Transaction = $taskTransaction }
    try { return $taskCommand.ExecuteScalar() } finally { $taskCommand.Dispose() }
}
try {
    $taskConnection.Open()
    if ([int](Sql "SELECT CASE WHEN OBJECT_ID('dbo.v_IntegridadPN2_Instalada','V') IS NOT NULL AND OBJECT_ID('dbo.sp_MaestroPN1_Exigir','P') IS NOT NULL THEN 1 ELSE 0 END") -ne 1) {
        throw 'Primero instalá PN2, Maestros y sus controles de integridad.'
    }
    if ([int](Sql 'SELECT COUNT(*) FROM dbo.v_IntegridadPN1_Errores') -ne 0) { throw 'La integridad presenta inconsistencias.' }
    $taskBackupDirectory = [string](Sql "SELECT CONVERT(NVARCHAR(4000),SERVERPROPERTY('InstanceDefaultBackupPath'))")
    if (!$taskBackupDirectory) { throw 'SQL Server no informó una carpeta de respaldos.' }
    $taskBackupPath = $taskBackupDirectory.TrimEnd('\','/') + '\' + $taskDatabase + '_AntesCompatibilidad_' + [DateTime]::UtcNow.ToString('yyyyMMdd_HHmmss') + '_' + [guid]::NewGuid().ToString('N') + '.bak'
    $taskCommand = $taskConnection.CreateCommand()
    $taskCommand.CommandTimeout = 180
    [void]$taskCommand.Parameters.Add('@ruta',[System.Data.SqlDbType]::NVarChar,4000)
    $taskCommand.Parameters['@ruta'].Value = $taskBackupPath
    try {
        $taskCommand.CommandText = 'BACKUP DATABASE [' + $taskDatabase + '] TO DISK=@ruta WITH COPY_ONLY,CHECKSUM'
        [void]$taskCommand.ExecuteNonQuery()
        $taskCommand.CommandText = 'RESTORE VERIFYONLY FROM DISK=@ruta WITH CHECKSUM'
        [void]$taskCommand.ExecuteNonQuery()
    } finally { $taskCommand.Dispose() }
    Write-Host "Respaldo verificado: $taskBackupPath"
    $taskTransaction = $taskConnection.BeginTransaction()
    foreach ($taskBatch in $taskBatches) { [void](Sql $taskBatch) }
    if ([int](Sql 'SELECT COUNT(*) FROM dbo.v_IntegridadPN1_Errores') -ne 0) { throw 'La actualización no conserva la integridad.' }
    [void](Sql 'EXEC dbo.sp_MaestroPN1_Exigir; EXEC dbo.sp_CompatibilidadTransfusional_Exigir;')
    $taskCount = [int](Sql 'SELECT COUNT(*) FROM dbo.CompatibilidadTransfusional WHERE activo=1')
    $taskTransaction.Commit(); $taskTransaction.Dispose(); $taskTransaction = $null
    Write-Host "Compatibilidad instalada en $taskDatabase. Reglas activas: $taskCount. Volvé a abrir la aplicación."
} catch {
    if ($taskTransaction) { try { $taskTransaction.Rollback() } catch {} }
    throw
} finally {
    if ($taskTransaction) { $taskTransaction.Dispose() }
    $taskConnection.Dispose()
}
