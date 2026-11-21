param(
    [Parameter(Mandatory = $true, ParameterSetName = 'Instalar')] [string]$Servidor,
    [Parameter(Mandatory = $true, ParameterSetName = 'Instalar')] [string]$Configuracion,
    [Parameter(ParameterSetName = 'Instalar')]
    [Parameter(Mandatory = $true, ParameterSetName = 'LeerServidor')] [string]$ConfiguracionExistente,
    [Parameter(ParameterSetName = 'Instalar')] [switch]$PrepararLocalDB,
    [Parameter(ParameterSetName = 'Instalar')] [switch]$DatosPrueba,
    [Parameter(Mandatory = $true, ParameterSetName = 'LeerServidor')] [switch]$LeerServidorConfigurado,
    [Parameter(Mandatory = $true, ParameterSetName = 'Detectar')] [switch]$DetectarInstancias,
    [Parameter(Mandatory = $true, ParameterSetName = 'VerificarLocalDB')] [switch]$VerificarLocalDB
)

$ErrorActionPreference = 'Stop'
function Read-ConexionConfigurada([string]$Ruta) {
    [xml]$xml = Get-Content -Raw -LiteralPath $Ruta
    $entrada = $xml.SelectSingleNode('/configuration/connectionStrings/add[@connectionString]')
    if (!$entrada) { throw 'App.local.config no contiene una conexión SQL.' }
    $conexion = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $entrada.GetAttribute('connectionString')
    if ($conexion.InitialCatalog -ne 'HemovidaGest' -or !$conexion.DataSource.Trim() -or $conexion.AttachDBFilename) {
        throw 'App.local.config debe señalar una base HemovidaGest en una instancia de SQL Server.'
    }
    return $conexion
}

if ($LeerServidorConfigurado) {
    try {
        (Read-ConexionConfigurada $ConfiguracionExistente).DataSource
        exit 0
    } catch {
        [Console]::Error.WriteLine($_.Exception.GetBaseException().Message)
        exit 1
    }
}

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
    '11_ComprobantesAutomaticos.sql',
    '12_DiagramasSSMS.sql',
    '13_AjusteCantidadDonacion.sql',
    '14_DiagramasCDUSSMS.sql',
    '15_AbastecimientoRiesgo.sql',
    '15a_AbastecimientoUI.sql',
    '16_DiagramasFinalesSSMS.sql'
)
$scriptsMigracion = @(
    '06_MaestrosPN1.sql', '06a_CompatibilidadTransfusional.sql', '06b_TipificacionDonante.sql',
    '07_PN1.sql', '08_PN2_Catalogos.sql', '09_PN2.sql', '10_IntegridadPN2.sql',
    '11_ComprobantesAutomaticos.sql', '12_DiagramasSSMS.sql', '13_AjusteCantidadDonacion.sql',
    '14_DiagramasCDUSSMS.sql', '15_AbastecimientoRiesgo.sql', '15a_AbastecimientoUI.sql', '16_DiagramasFinalesSSMS.sql'
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

$base = $null
$master = $null
$baseCreada = $false
$transaccion = $null
$version = $null
$etapa = 'Leer la configuración de SQL Server'

try {
    $conservarConfiguracion = $ConfiguracionExistente -and (Test-Path -LiteralPath $ConfiguracionExistente -PathType Leaf)
    if ($conservarConfiguracion) {
        $ejecutable = Join-Path (Split-Path -Parent $ConfiguracionExistente) 'UI.exe'
        if (Get-Process -Name UI -ErrorAction SilentlyContinue | Where-Object Path -EQ $ejecutable) {
            throw 'Cerrá HemovidaGest para continuar con la actualización.'
        }
        $cadena = Read-ConexionConfigurada $ConfiguracionExistente
        $Servidor = $cadena.DataSource
    } else {
        $cadena = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
        $cadena['Data Source'] = $Servidor.Trim()
        $cadena['Initial Catalog'] = 'HemovidaGest'
        $cadena['Integrated Security'] = $true
    }
    $cadenaAplicacion = $cadena.ConnectionString
    $cadena['Pooling'] = $false
    $base = New-Object System.Data.SqlClient.SqlConnection $cadena.ConnectionString
    $cadena['Initial Catalog'] = 'master'
    $master = New-Object System.Data.SqlClient.SqlConnection $cadena.ConnectionString

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
    Invoke-SqlBatch $master $null @'
DECLARE @resultado int;
EXEC @resultado = sys.sp_getapplock @Resource=N'HemovidaGest.Instalador', @LockMode='Exclusive',
    @LockOwner='Session', @LockTimeout=10000;
IF @resultado < 0 THROW 51000, 'Hay otra instalación de HemovidaGest en curso.', 1;
'@
    $comando = $master.CreateCommand()
    try {
        $comando.CommandText = "SELECT CASE WHEN DB_ID(N'HemovidaGest') IS NULL THEN 0 ELSE 1 END;"
        $existe = [int]$comando.ExecuteScalar() -eq 1
        if ($PrepararLocalDB) {
            $comando.CommandText = "SELECT SERVERPROPERTY('ProductMajorVersion');"
            if ([int]$comando.ExecuteScalar() -lt 15) {
                throw "La instancia $Servidor usa una versión anterior a LocalDB 2019."
            }
        }
        if (!$existe -and $conservarConfiguracion) {
            throw 'La base de la instalación anterior no está disponible en la instancia configurada.'
        }
        if (!$existe -and $PrepararLocalDB) {
            # El directorio predeterminado de LocalDB puede compartirse entre instancias del mismo usuario.
            $comando.CommandText = 'SELECT physical_name FROM sys.master_files WHERE database_id = 1 AND file_id = 1;'
            $carpetaInstancia = Split-Path -Parent ([string]$comando.ExecuteScalar())
            $datos = (Join-Path $carpetaInstancia 'HemovidaGest.mdf').Replace("'", "''")
            $log = (Join-Path $carpetaInstancia 'HemovidaGest_log.ldf').Replace("'", "''")
            $crearBase = @"
CREATE DATABASE [HemovidaGest]
ON (NAME = N'HemovidaGest', FILENAME = N'$datos')
LOG ON (NAME = N'HemovidaGest_log', FILENAME = N'$log');
"@
        }
    } finally {
        $comando.Dispose()
    }

    if (!$existe) {
        $etapa = 'Crear HemovidaGest'
        if (!$PrepararLocalDB) { $crearBase = 'CREATE DATABASE [HemovidaGest];' }
        Invoke-SqlBatch $master $null $crearBase
        $baseCreada = $true
    }

    $base.Open()
    if ($existe) {
        $etapa = 'Comprobar la versión de la base existente'
        $comando = $base.CreateCommand()
        try {
            $comando.CommandTimeout = 180
            $comando.CommandText = @'
IF OBJECT_ID(N'dbo.v_IntegridadPN2_Instalada', N'V') IS NULL
    OR OBJECT_ID(N'dbo.ComprobantePendiente', N'U') IS NULL
    OR OBJECT_ID(N'dbo.MaestrosPN1Control', N'U') IS NULL
    THROW 51000, 'La base no corresponde a HemovidaGest Entrega 2 o 3.', 1;
SELECT ISNULL((SELECT TRY_CONVERT(int, value) FROM sys.extended_properties
    WHERE class=0 AND name=N'HemovidaGest.Entrega'), -1)
WHERE EXISTS (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'HemovidaGest.Entrega');
'@
            $version = $comando.ExecuteScalar()
            if ($null -ne $version -and $version -ne 3) { throw "Versión de base incompatible con Entrega 3: $version." }
            $comando.CommandText = @'
SELECT COALESCE((SELECT ISNULL(TRY_CONVERT(int, value), -1) FROM sys.extended_properties
    WHERE class=0 AND name=N'HemovidaGest.AjusteCantidadDonacion'), 0);
'@
            $revisionAjusteCantidad = [int]$comando.ExecuteScalar()
            if ($revisionAjusteCantidad -lt 0 -or $revisionAjusteCantidad -gt 2) {
                throw "Revisión de corrección de donaciones incompatible: $revisionAjusteCantidad."
            }
            if ($null -eq $version) {
                $scripts = $scriptsMigracion
            } elseif ($revisionAjusteCantidad -lt 2) {
                $scripts = @('13_AjusteCantidadDonacion.sql')
            } else {
                $scripts = @()
            }
            if ($scripts.Count -gt 0) {
                $etapa = 'Respaldar la base antes de actualizar'
                $comando.CommandText = @'
SELECT COALESCE(NULLIF(CONVERT(nvarchar(4000), SERVERPROPERTY('InstanceDefaultBackupPath')), N''),
    (SELECT LEFT(physical_name, LEN(physical_name) - CHARINDEX(N'/', REVERSE(REPLACE(physical_name, N'\', N'/'))) + 1)
     FROM sys.database_files WHERE file_id=1));
'@
                $carpeta = [string]$comando.ExecuteScalar()
                if (!$carpeta) { throw 'SQL Server no informó una carpeta de respaldos.' }
                $separador = if ($carpeta.StartsWith('/')) { '/' } else { '\' }
                $nombreRespaldo = 'HemovidaGest_AntesEntrega3_' + [DateTime]::UtcNow.ToString('yyyyMMdd_HHmmss') + '_' + [guid]::NewGuid().ToString('N') + '.bak'
                $respaldo = $carpeta.TrimEnd('\','/') + $separador + $nombreRespaldo
                $comando.Parameters.Add('@ruta', [System.Data.SqlDbType]::NVarChar, 4000).Value = $respaldo
                $comando.CommandText = 'BACKUP DATABASE [HemovidaGest] TO DISK=@ruta WITH COPY_ONLY,CHECKSUM;'
                [void]$comando.ExecuteNonQuery()
                $comando.CommandText = 'RESTORE VERIFYONLY FROM DISK=@ruta WITH CHECKSUM;'
                [void]$comando.ExecuteNonQuery()
                Write-Output "Respaldo verificado: $respaldo"
            }
        } finally {
            $comando.Dispose()
        }
    } elseif ($DatosPrueba) {
        $scripts += 'DatosPrueba.sql'
    }

    $transaccion = $base.BeginTransaction()
    if ($existe -and $null -eq $version) {
        Invoke-SqlBatch $base $transaccion "EXEC sys.sp_set_session_context @key=N'HemovidaGest.ActualizandoEntrega3', @value=1;"
    }
    foreach ($script in $scripts) {
        $etapa = $script
        $ruta = Join-Path $PSScriptRoot $script
        if (!(Test-Path -LiteralPath $ruta)) {
            $carpetaSql = Join-Path (Split-Path $PSScriptRoot -Parent) 'Database'
            if ($script -ne '00_tecni_salud_schema.sql') { $carpetaSql = Join-Path $carpetaSql 'StoredProcedures' }
            $ruta = Join-Path $carpetaSql $script
        }
        $lotes = [regex]::Split((Get-Content -Raw -Encoding UTF8 -LiteralPath $ruta), '(?im)^[ \t]*GO[ \t]*(?:--[^\r\n]*)?\r?$')
        if ($script -eq '00_tecni_salud_schema.sql') {
            # La creación y la selección de la base ya se hicieron fuera de la transacción.
            if ($lotes.Count -lt 3 -or
                $lotes[0] -notmatch "(?is)\bIF\s+DB_ID\(N'HemovidaGest'\)\s+IS\s+NULL\s+BEGIN\s+EXEC\(N'CREATE DATABASE HemovidaGest'\);\s+END\s*$" -or
                $lotes[1] -notmatch '(?i)^\s*USE HemovidaGest;\s*$') {
                throw 'Cambió el inicio del esquema. Revisá la secuencia del instalador.'
            }
            $lotes = $lotes[2..($lotes.Count - 1)]
        }
        foreach ($lote in $lotes) {
            $sql = $lote -replace '(?im)^[ \t]*USE \[HemovidaGest\];[ \t]*\r?$', ''
            if ($sql.Trim()) { Invoke-SqlBatch $base $transaccion $sql }
        }
        Write-Output "Aplicado $script"
    }

    $etapa = 'Verificar la integridad y los diagramas de la base'
    Invoke-SqlBatch $base $transaccion @'
IF OBJECT_ID(N'dbo.v_IntegridadPN2_Instalada', N'V') IS NULL
    THROW 51000, 'Falta el módulo de integridad PN2.', 1;
IF EXISTS (SELECT 1 FROM dbo.v_IntegridadPN1_Errores)
    THROW 51000, 'La base presenta errores de integridad.', 1;
IF NOT EXISTS (SELECT 1 FROM sys.extended_properties
    WHERE class=0 AND name=N'HemovidaGest.AjusteCantidadDonacion' AND TRY_CONVERT(int, value)=2)
    OR OBJECT_ID(N'dbo.sp_Donacion_ConsultarCantidad', N'P') IS NULL
    OR OBJECT_ID(N'dbo.sp_Donacion_AjustarCantidad', N'P') IS NULL
    THROW 51000, 'Falta la revisión de corrección de cantidad de donaciones.', 1;
IF OBJECT_ID(N'dbo.sp_PN2_Abastecimiento_Listar', N'P') IS NULL
    THROW 51000, 'Falta el reporte de abastecimiento.', 1;
IF OBJECT_ID(N'dbo.sysdiagrams', N'U') IS NULL
    THROW 51000, 'No se instalaron los diagramas nativos de SSMS.', 1;
IF dbo.fn_diagramobjects() <> 255
    THROW 51000, 'El soporte nativo de diagramas de SSMS está incompleto.', 1;
IF (SELECT COUNT(DISTINCT LEFT(name, 6)) FROM dbo.sysdiagrams
    WHERE principal_id = DATABASE_PRINCIPAL_ID(N'dbo')
      AND name LIKE N'CDU-0[1-7] - %'
      AND version IS NOT NULL AND DATALENGTH(definition) > 0) <> 7
    THROW 51000, 'Falta al menos un diagrama nativo de CDU-01 a CDU-07.', 1;
'@

    $etapa = 'Guardar App.local.config'
    if ($conservarConfiguracion) {
        Copy-Item -LiteralPath $ConfiguracionExistente -Destination $Configuracion
    } else {
        [xml]$xml = '<configuration><connectionStrings><add name="HemovidaGest" providerName="System.Data.SqlClient" /></connectionStrings></configuration>'
        $xml.configuration.connectionStrings.add.SetAttribute('connectionString', $cadenaAplicacion)
        $xml.Save($Configuracion)
    }
    $etapa = 'Confirmar la instalación de la base'
    if ($null -eq $version) {
        Invoke-SqlBatch $base $transaccion "EXEC sys.sp_addextendedproperty @name=N'HemovidaGest.Entrega', @value=3;"
    }
    $transaccion.Commit()
    $baseCreada = $false
    Write-Output "HemovidaGest Entrega 3 lista en $Servidor."
} catch {
    $mensaje = "$etapa - $($_.Exception.GetBaseException().Message)"
    if ($transaccion) { try { $transaccion.Rollback() } catch {} }
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
    if ($base) { $base.Dispose() }
    if ($master) { $master.Dispose() }
}
