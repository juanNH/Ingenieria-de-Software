param(
    [Parameter(Mandatory = $true, ParameterSetName = 'Instalar')] [string]$Servidor,
    [Parameter(Mandatory = $true, ParameterSetName = 'Instalar')] [string]$Configuracion,
    [Parameter(ParameterSetName = 'Instalar')] [switch]$PrepararLocalDB,
    [Parameter(ParameterSetName = 'Instalar')] [switch]$DatosPrueba,
    [Parameter(Mandatory = $true, ParameterSetName = 'Detectar')] [switch]$DetectarInstancias,
    [Parameter(Mandatory = $true, ParameterSetName = 'VerificarLocalDB')] [switch]$VerificarLocalDB
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

function Get-LocalDB {
    if (![Environment]::Is64BitOperatingSystem) { return }

    # Consultar la vista nativa aunque el instalador ejecute PowerShell de 32 bits.
    $registro = [Microsoft.Win32.RegistryKey]::OpenBaseKey(
        [Microsoft.Win32.RegistryHive]::LocalMachine, [Microsoft.Win32.RegistryView]::Registry64
    )
    try {
        $instaladas = $registro.OpenSubKey('SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions')
        if (!$instaladas) { return }
        try {
            $versiones = $instaladas.GetSubKeyNames() |
                Where-Object { $_ -match '^\d+\.\d+$' -and ([version]$_).Major -ge 15 } |
                Sort-Object { [version]$_ } -Descending

            foreach ($version in $versiones) {
                $clave = $instaladas.OpenSubKey($version)
                if (!$clave) { continue }
                try {
                    $api = $clave.GetValue('InstanceAPIPath')
                } finally {
                    $clave.Dispose()
                }
                if (!$api -or !(Test-Path -LiteralPath $api -PathType Leaf)) { continue }

                $bin = Split-Path -Parent $api
                $motor = Split-Path -Parent (Split-Path -Parent $bin)
                $utilidad = Join-Path $motor 'Tools\Binn\SqlLocalDB.exe'
                if (Test-Path -LiteralPath $utilidad -PathType Leaf) {
                    return [pscustomobject]@{ Ruta = $utilidad; Version = $version }
                }
            }

            if ($versiones) {
                throw 'LocalDB figura instalado, pero no se encontraron los archivos necesarios para iniciarlo.'
            }
        } finally {
            $instaladas.Dispose()
        }
    } finally {
        $registro.Dispose()
    }
}

function Invoke-LocalDB([string]$Ruta, [string]$Argumentos) {
    $proceso = New-Object System.Diagnostics.Process
    try {
        $proceso.StartInfo.FileName = $Ruta
        $proceso.StartInfo.Arguments = $Argumentos
        $proceso.StartInfo.UseShellExecute = $false
        $proceso.StartInfo.CreateNoWindow = $true
        $proceso.StartInfo.RedirectStandardOutput = $true
        $proceso.StartInfo.RedirectStandardError = $true
        [void]$proceso.Start()
        $salida = $proceso.StandardOutput.ReadToEndAsync()
        $errorNativo = $proceso.StandardError.ReadToEndAsync()
        $proceso.WaitForExit()
        if ($proceso.ExitCode -ne 0) {
            $detalle = ($salida.Result + ' ' + $errorNativo.Result).Trim()
            throw "SqlLocalDB $Argumentos devolvió el código $($proceso.ExitCode). $detalle"
        }
        return $salida.Result
    } finally {
        $proceso.Dispose()
    }
}

if ($VerificarLocalDB) {
    try {
        $localDB = Get-LocalDB
        if (!$localDB) { exit 2 }
        [void](Invoke-LocalDB $localDB.Ruta 'versions')
        exit 0
    } catch {
        [Console]::Error.WriteLine($_.Exception.GetBaseException().Message)
        exit 1
    }
}

$scripts = @(
    '00_tecni_salud_schema.sql',
    '01_AuthUsuario.sql',
    '02_Bitacora.sql',
    '03_Auditoria.sql',
    '04_Permisos.sql',
    '05_IntegridadPN1.sql',
    '06_MaestrosPN1.sql','06a_CompatibilidadTransfusional.sql','06b_TipificacionDonante.sql',
    '07_PN1.sql',
    '08_PN2_Catalogos.sql',
    '09_PN2.sql',
    '09a_RelacionesSolicitudPN2.sql',
    '10_IntegridadPN2.sql',
    '11_ComprobantesAutomaticos.sql'
)
if ($DatosPrueba) {
    $scripts += 'DatosPrueba.sql'
}

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

    if ($PrepararLocalDB) {
        $etapa = 'Preparar la instancia local de HemovidaGest'
        if ($Servidor.Trim() -notmatch '^\(localdb\)\\([a-z0-9_]{1,128})$') {
            throw 'Indicá una instancia LocalDB con un nombre formado por letras, números o guiones bajos.'
        }
        $instancia = $Matches[1]
        $localDB = Get-LocalDB
        if (!$localDB) { throw 'No se encontró SQL Server Express LocalDB 2019 o posterior.' }

        $instancias = (Invoke-LocalDB $localDB.Ruta 'info') -split '\r?\n'
        if (($instancias | ForEach-Object { $_.Trim() }) -notcontains $instancia) {
            [void](Invoke-LocalDB $localDB.Ruta ('create "{0}" {1}' -f $instancia, $localDB.Version))
        }
        [void](Invoke-LocalDB $localDB.Ruta ('start "{0}"' -f $instancia))
    }

    $etapa = 'Conectar con la instancia de SQL Server'
    $master.Open()
    $crearBase = @'
IF DB_ID(N'HemovidaGest') IS NOT NULL
    THROW 51000, 'HemovidaGest ya existe en esta instancia. Elegí una instancia de prueba o usá la migración correspondiente.', 1;
'@
    if ($PrepararLocalDB) {
        $comando = $master.CreateCommand()
        try {
            $comando.CommandText = "SELECT SERVERPROPERTY('ProductMajorVersion');"
            if ([int]$comando.ExecuteScalar() -lt 15) {
                throw "La instancia $Servidor usa una versión anterior a LocalDB 2019."
            }
            # El directorio predeterminado de LocalDB puede compartirse entre instancias del mismo usuario.
            $comando.CommandText = 'SELECT physical_name FROM sys.master_files WHERE database_id = 1 AND file_id = 1;'
            $carpetaInstancia = Split-Path -Parent ([string]$comando.ExecuteScalar())
            $datos = (Join-Path $carpetaInstancia 'HemovidaGest.mdf').Replace("'", "''")
            $log = (Join-Path $carpetaInstancia 'HemovidaGest_log.ldf').Replace("'", "''")
            $crearBase += [Environment]::NewLine + @"
CREATE DATABASE [HemovidaGest]
ON (NAME = N'HemovidaGest', FILENAME = N'$datos')
LOG ON (NAME = N'HemovidaGest_log', FILENAME = N'$log');
"@
        } finally {
            $comando.Dispose()
        }
    } else {
        $crearBase += [Environment]::NewLine + 'CREATE DATABASE [HemovidaGest];'
    }
    $etapa = 'Crear HemovidaGest'
    Invoke-SqlBatch $master $null $crearBase
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
