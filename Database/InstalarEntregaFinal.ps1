param([string]$Servidor = '.', [string]$Base = 'HemovidaGest')
$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($Servidor)) { throw 'Indicá una instancia SQL Server.' }
if ($Base -notmatch '^[A-Za-z0-9_]+$' -or $Base -in @('master','model','msdb','tempdb')) {
    throw 'La base de destino debe ser una base de aplicación con nombre válido.'
}
$entregaBuilder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
$entregaBuilder['Data Source'] = $Servidor
$entregaBuilder['Initial Catalog'] = $Base
$entregaBuilder['Integrated Security'] = $true
$entregaConexion = New-Object System.Data.SqlClient.SqlConnection $entregaBuilder.ConnectionString
$entregaTx = $null
try {
    $entregaConexion.Open()
    $entregaComando = $entregaConexion.CreateCommand()
    try {
        $entregaComando.CommandText = "SELECT CASE WHEN OBJECT_ID('dbo.v_IntegridadPN2_Instalada','V') IS NOT NULL AND OBJECT_ID('dbo.ComprobantePendiente','U') IS NOT NULL THEN 1 ELSE 0 END"
        if ([int]$entregaComando.ExecuteScalar() -ne 1) { throw 'Primero instalá PN2 y comprobantes mediante InstalarPN2.ps1.' }
        $entregaComando.CommandText = 'SELECT COUNT(*) FROM dbo.v_IntegridadPN1_Errores'
        if ([int]$entregaComando.ExecuteScalar() -ne 0) { throw 'Hay inconsistencias de integridad. Revisalas antes de actualizar.' }
        $entregaComando.CommandText = "SELECT CONVERT(NVARCHAR(4000),SERVERPROPERTY('InstanceDefaultBackupPath'))"
        $entregaCarpeta = [string]$entregaComando.ExecuteScalar()
        if (!$entregaCarpeta) { throw 'SQL Server no informó una carpeta de backups.' }
        $entregaBackup = $entregaCarpeta.TrimEnd('\','/') + '\' + $Base + '_AntesEntregaFinal_' + [DateTime]::UtcNow.ToString('yyyyMMdd_HHmmss') + '_' + [Guid]::NewGuid().ToString('N') + '.bak'
        $entregaComando.CommandTimeout = 180
        $entregaComando.CommandText = 'BACKUP DATABASE [' + $Base + '] TO DISK=@ruta WITH COPY_ONLY,CHECKSUM'
        [void]$entregaComando.Parameters.Add('@ruta',[System.Data.SqlDbType]::NVarChar,4000)
        $entregaComando.Parameters['@ruta'].Value = $entregaBackup
        [void]$entregaComando.ExecuteNonQuery()
        $entregaComando.CommandText = 'RESTORE VERIFYONLY FROM DISK=@ruta WITH CHECKSUM'
        [void]$entregaComando.ExecuteNonQuery()
        Write-Host "Respaldo verificado: $entregaBackup"
    } finally { $entregaComando.Dispose() }
    $entregaTx = $entregaConexion.BeginTransaction()
    foreach ($entregaScript in @('15_AbastecimientoRiesgo.sql','15a_AbastecimientoUI.sql','16_DiagramasFinalesSSMS.sql')) {
        $entregaRuta = Join-Path $PSScriptRoot ('StoredProcedures/' + $entregaScript)
        $entregaSql = (Get-Content -Raw -Encoding UTF8 -LiteralPath $entregaRuta).Replace('USE [HemovidaGest];','')
        foreach ($entregaLote in [regex]::Split($entregaSql,'(?im)^\s*GO\s*(?:--[^\r\n]*)?$')) {
            if (!$entregaLote.Trim()) { continue }
            $entregaComando = $entregaConexion.CreateCommand()
            try {
                $entregaComando.Transaction = $entregaTx
                $entregaComando.CommandTimeout = 180
                $entregaComando.CommandText = $entregaLote
                [void]$entregaComando.ExecuteNonQuery()
            } finally { $entregaComando.Dispose() }
        }
        Write-Host "Aplicado: $entregaScript"
    }
    $entregaComando = $entregaConexion.CreateCommand()
    try {
        $entregaComando.Transaction = $entregaTx
        $entregaComando.CommandText = 'SELECT COUNT(*) FROM dbo.v_IntegridadPN1_Errores'
        if ([int]$entregaComando.ExecuteScalar() -ne 0) { throw 'La actualización no conserva la integridad; se revierte.' }
        $entregaComando.CommandText = "SELECT CASE WHEN OBJECT_ID('dbo.sp_PN2_Abastecimiento_Listar','P') IS NOT NULL AND EXISTS(SELECT 1 FROM dbo.sysdiagrams WHERE principal_id=DATABASE_PRINCIPAL_ID(N'dbo') AND name=N'G07 - Modelo de datos completo' AND DATALENGTH(definition)>0) THEN 1 ELSE 0 END"
        if ([int]$entregaComando.ExecuteScalar() -ne 1) { throw 'Falta el reporte o el modelo general; se revierte la actualización.' }
    } finally { $entregaComando.Dispose() }
    $entregaTx.Commit(); $entregaTx.Dispose(); $entregaTx = $null
    Write-Host "Entrega final instalada en $Servidor / $Base. Reiniciá la aplicación para cargar las traducciones."
} catch {
    if ($entregaTx) { try { $entregaTx.Rollback() } catch {} }
    throw
} finally {
    if ($entregaTx) { $entregaTx.Dispose() }
    $entregaConexion.Dispose()
}
