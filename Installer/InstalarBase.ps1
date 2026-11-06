param(
    [Parameter(Mandatory = $true, ParameterSetName = 'Instalar')] [string]$Servidor,
    [Parameter(Mandatory = $true, ParameterSetName = 'Instalar')] [string]$Configuracion,
    [Parameter(Mandatory = $true, ParameterSetName = 'Detectar')] [switch]$DetectarInstancias
)

$ErrorActionPreference = 'Stop'
if ($DetectarInstancias) {
    Get-Service -Name 'MSSQLSERVER', 'MSSQL$*' -ErrorAction SilentlyContinue |
        Where-Object Status -eq Running |
        ForEach-Object {
            if ($_.Name -eq 'MSSQLSERVER') { '.' }
            else { '.\' + $_.Name.Substring('MSSQL$'.Length) }
        } |
        Sort-Object
    return
}

$scripts = @(
    '00_tecni_salud_schema.sql',
    '01_AuthUsuario.sql',
    '02_Bitacora.sql',
    '03_Auditoria.sql',
    '04_Permisos.sql',
    '05_IntegridadPN1.sql',
    '06_MaestrosPN1.sql',
    '07_PN1.sql',
    '08_PN2_Catalogos.sql',
    '09_PN2.sql',
    '09a_RelacionesSolicitudPN2.sql',
    '10_IntegridadPN2.sql',
    '11_ComprobantesAutomaticos.sql'
)

function Invoke-SqlBatch($Conexion, $Transaccion, [string]$Sql) {
    $comando = $Conexion.CreateCommand()
    try {
        $comando.Transaction = $Transaccion
        $comando.CommandTimeout = 180
        $comando.CommandText = $Sql
        [void]$comando.ExecuteNonQuery()
    } finally {
        $comando.Dispose()
    }
}

$cadena = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
$cadena['Data Source'] = $Servidor.Trim()
$cadena['Initial Catalog'] = 'HemovidaGest'
$cadena['Integrated Security'] = $true
$cadenaAplicacion = $cadena.ConnectionString
$cadena['Pooling'] = $false
$base = New-Object System.Data.SqlClient.SqlConnection $cadena.ConnectionString
$cadena['Initial Catalog'] = 'master'
$master = New-Object System.Data.SqlClient.SqlConnection $cadena.ConnectionString
$baseCreada = $false
$transaccion = $null
$etapa = 'Preparar los scripts SQL'

try {
    $lotes = @{}
    foreach ($script in $scripts) {
        $ruta = Join-Path $PSScriptRoot $script
        if (!(Test-Path -LiteralPath $ruta)) {
            $carpetaSql = Join-Path (Split-Path $PSScriptRoot -Parent) 'Database'
            if ($script -ne $scripts[0]) { $carpetaSql = Join-Path $carpetaSql 'StoredProcedures' }
            $ruta = Join-Path $carpetaSql $script
        }
        $sql = Get-Content -Raw -Encoding UTF8 -LiteralPath $ruta
        $lotes[$script] = [regex]::Split($sql, '(?im)^[ \t]*GO[ \t]*(?:--[^\r\n]*)?\r?$')
    }

    # El esquema fuente crea y selecciona la base en sus dos primeros lotes.
    # El instalador hace esos dos pasos por separado antes de abrir la transacción.
    $esquema = $lotes[$scripts[0]]
    if ($esquema.Count -lt 3 -or
        $esquema[0] -notmatch "(?is)\bIF\s+DB_ID\(N'HemovidaGest'\)\s+IS\s+NULL\s+BEGIN\s+EXEC\(N'CREATE DATABASE HemovidaGest'\);\s+END\s*$" -or
        $esquema[1] -notmatch '(?i)^\s*USE HemovidaGest;\s*$') {
        throw 'Cambió el inicio del esquema. Revisá la secuencia del instalador.'
    }
    $lotes[$scripts[0]] = $esquema[2..($esquema.Count - 1)]

    $etapa = 'Conectar con la instancia de SQL Server'
    $master.Open()
    $etapa = 'Crear HemovidaGest'
    Invoke-SqlBatch $master $null @'
IF DB_ID(N'HemovidaGest') IS NOT NULL
    THROW 51000, 'HemovidaGest ya existe en esta instancia. Elegí una instancia de prueba o usá la migración correspondiente.', 1;
CREATE DATABASE [HemovidaGest];
'@
    $baseCreada = $true
    $base.Open()
    $transaccion = $base.BeginTransaction()
    foreach ($script in $scripts) {
        $etapa = $script
        foreach ($lote in $lotes[$script]) {
            $sql = $lote -replace '(?im)^[ \t]*USE \[HemovidaGest\];[ \t]*\r?$', ''
            if ($sql.Trim()) { Invoke-SqlBatch $base $transaccion $sql }
        }
        Write-Output "Aplicado $script"
    }

    $etapa = 'Verificar la integridad de la base'
    Invoke-SqlBatch $base $transaccion @'
IF OBJECT_ID(N'dbo.v_IntegridadPN2_Instalada', N'V') IS NULL
    THROW 51000, 'Falta el módulo de integridad PN2.', 1;
IF EXISTS (SELECT 1 FROM dbo.v_IntegridadPN1_Errores)
    THROW 51000, 'La base nueva presenta errores de integridad.', 1;
'@

    $etapa = 'Guardar App.local.config'
    [xml]$xml = '<configuration><connectionStrings><add name="HemovidaGest" providerName="System.Data.SqlClient" /></connectionStrings></configuration>'
    $xml.configuration.connectionStrings.add.SetAttribute('connectionString', $cadenaAplicacion)
    $xml.Save($Configuracion)
    $etapa = 'Confirmar la instalación de la base'
    $transaccion.Commit()
    Write-Output "HemovidaGest instalada en $Servidor."
} catch {
    $mensaje = "$etapa - $($_.Exception.GetBaseException().Message)"
    if ($baseCreada) {
        $base.Dispose()
        try {
            Invoke-SqlBatch $master $null 'DROP DATABASE [HemovidaGest];'
        } catch {
            $mensaje += "`nNo se pudo retirar la base creada durante este intento. $($_.Exception.GetBaseException().Message)"
        }
    }
    [Console]::Error.WriteLine($mensaje)
    exit 1
} finally {
    if ($transaccion) { $transaccion.Dispose() }
    $base.Dispose()
    $master.Dispose()
}
