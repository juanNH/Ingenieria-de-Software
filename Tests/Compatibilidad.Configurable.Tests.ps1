param([string]$Servidor = '.', [string]$Captura)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$taskDatabase = 'HemovidaGest_Compat_Test_' + [guid]::NewGuid().ToString('N')
$taskConnection = New-Object System.Data.SqlClient.SqlConnection "Server=$Servidor;Database=master;Integrated Security=True;TrustServerCertificate=True"
$taskCreated = $false
function Sql([string]$Texto) {
    $taskCommand = $taskConnection.CreateCommand()
    $taskCommand.CommandText = $Texto; $taskCommand.CommandTimeout = 120
    try { return $taskCommand.ExecuteScalar() } finally { $taskCommand.Dispose() }
}
function Script([string]$Nombre) {
    $taskPath = Join-Path $taskRoot ('Database/' + $Nombre)
    $taskSql = [IO.File]::ReadAllText($taskPath).Replace('HemovidaGest', $taskDatabase)
    foreach ($taskBatch in [regex]::Split($taskSql, '(?im)^\s*GO\s*(?:--[^\r\n]*)?$')) {
        if ($taskBatch.Trim()) {
            try { [void](Sql $taskBatch) } catch { throw "Error en $Nombre : $($_.Exception.Message)" }
        }
    }
}
function Check($Actual, $Esperado, [string]$Descripcion) {
    if ($Actual -ne $Esperado) { throw "$Descripcion. Esperado: $Esperado; recibido: $Actual" }
    Write-Host "OK $Descripcion"
}
function Regla([int]$Id, [int]$Activo) {
    return Sql "EXEC dbo.sp_CompatibilidadTransfusional_Guardar $Id,'COMP_TEST','X','?','Y','~',$Activo,1"
}
try {
    $taskConnection.Open()
    [void](Sql "CREATE DATABASE [$taskDatabase]"); $taskCreated = $true
    $taskConnection.ChangeDatabase($taskDatabase)
    foreach ($taskScript in @('00_tecni_salud_schema.sql','StoredProcedures/01_AuthUsuario.sql',
        'StoredProcedures/02_Bitacora.sql','StoredProcedures/03_Auditoria.sql','StoredProcedures/04_Permisos.sql',
        'StoredProcedures/05_IntegridadPN1.sql','StoredProcedures/06_MaestrosPN1.sql',
        'StoredProcedures/06a_CompatibilidadTransfusional.sql','StoredProcedures/07_PN1.sql',
        'StoredProcedures/08_PN2_Catalogos.sql','StoredProcedures/09_PN2.sql',
        'StoredProcedures/09a_RelacionesSolicitudPN2.sql','StoredProcedures/10_IntegridadPN2.sql',
        'StoredProcedures/11_ComprobantesAutomaticos.sql')) { Script $taskScript }
    Check (Sql 'SELECT COUNT(*) FROM dbo.CompatibilidadTransfusional') 75 '75 combinaciones iniciales'
    Check (Sql "SELECT COUNT(*) FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID('dbo.CompatibilidadTransfusional') AND is_disabled=0 AND is_not_trusted=0") 5 'Cinco relaciones verificadas con los maestros'
    # Las 192 combinaciones de los tres componentes se comparan con la política anterior.
    [void](Sql @'
DECLARE @grupos TABLE(g VARCHAR(5)); INSERT @grupos VALUES('O'),('A'),('B'),('AB');
DECLARE @rh TABLE(r VARCHAR(5)); INSERT @rh VALUES('+'),('-');
IF EXISTS(SELECT 1 FROM @grupos d CROSS JOIN @grupos p CROSS JOIN @rh dr CROSS JOIN @rh pr
 WHERE dbo.fn_PN2_GrupoCandidato('Concentrado de eritrocitos',d.g,dr.r,p.g,pr.r)<>
 CASE WHEN (pr.r='+' OR dr.r='-') AND (d.g='O' OR d.g=p.g OR (p.g='AB' AND d.g IN ('A','B'))) THEN 1 ELSE 0 END)
 THROW 51000,'MATRIZ_ERITROCITOS',1;
IF EXISTS(SELECT 1 FROM @grupos d CROSS JOIN @grupos p CROSS JOIN @rh dr CROSS JOIN @rh pr
 WHERE dbo.fn_PN2_GrupoCandidato('Plasma fresco congelado',d.g,dr.r,p.g,pr.r)<>
 CASE WHEN d.g='AB' OR d.g=p.g OR (p.g='O' AND d.g IN ('A','B')) THEN 1 ELSE 0 END)
 THROW 51000,'MATRIZ_PLASMA',1;
IF EXISTS(SELECT 1 FROM @grupos d CROSS JOIN @grupos p CROSS JOIN @rh dr CROSS JOIN @rh pr
 WHERE dbo.fn_PN2_GrupoCandidato('Concentrado de plaquetas',d.g,dr.r,p.g,pr.r)<>
 CASE WHEN d.g=p.g AND (pr.r='+' OR dr.r='-') THEN 1 ELSE 0 END)
 THROW 51000,'MATRIZ_PLAQUETAS',1;
IF dbo.fn_PN2_GrupoCandidato(NULL,'O','-','A','+')<>0 OR
 dbo.fn_PN2_GrupoCandidato('Concentrado de eritrocitos',NULL,'-','A','+')<>0 OR
 dbo.fn_PN2_GrupoCandidato('Concentrado de eritrocitos','O',NULL,'A','+')<>0 OR
 dbo.fn_PN2_GrupoCandidato('Concentrado de eritrocitos','O','-',NULL,'+')<>0 OR
 dbo.fn_PN2_GrupoCandidato('Concentrado de eritrocitos','O','-','A',NULL)<>0
 THROW 51000,'ENTRADA_INCOMPLETA',1;
'@)
    Write-Host 'OK Matrices anteriores e información incompleta'
    foreach ($taskMaestro in @(@('COMPONENTE','COMP_TEST'),@('GRUPO','X'),@('GRUPO','Y'),@('RH','?'),@('RH','~'))) {
        $taskTipo,$taskCodigo = $taskMaestro
        Check (Sql "EXEC dbo.sp_MaestroPN1_Guardar '$taskTipo',0,'$taskCodigo',N'Prueba configurable',1,1") 'OK' "Maestro nuevo $taskTipo $taskCodigo"
    }
    Check (Sql "SELECT dbo.fn_PN2_GrupoCandidato('COMP_TEST','X','?','Y','~')") 0 'Sin regla no hay compatibilidad'
    Check (Regla 0 1) 'OK' 'Alta de regla con componente, grupos y Rh nuevos'
    $taskRegla = [int](Sql "SELECT id_compatibilidad FROM dbo.CompatibilidadTransfusional WHERE tipo_componente='COMP_TEST'")
    Check (Sql "SELECT dbo.fn_PN2_GrupoCandidato('COMP_TEST','X','?','Y','~')") 1 'Regla nueva aplicada sin cambiar código'
    Check (Sql "SELECT dbo.fn_PN2_GrupoCandidato('COMP_TEST','Y','~','X','?')") 0 'La dirección inversa no se habilita'
    Check (Regla 0 1) 'DUPLICADO' 'Duplicados rechazados'
    Check (Sql "EXEC dbo.sp_CompatibilidadTransfusional_Guardar 0,'INEXISTENTE','X','?','Y','~',1,1") 'DATOS_INVALIDOS' 'Maestros inexistentes rechazados'
    Check (Sql "EXEC dbo.sp_CompatibilidadTransfusional_Guardar $taskRegla,'COMP_TEST','Y','~','X','?',1,1") 'NO_ENCONTRADO' 'Combinación histórica inmutable'
    Check (Regla $taskRegla 0) 'OK' 'Baja lógica de una regla'
    Script 'StoredProcedures/06a_CompatibilidadTransfusional.sql'
    Check (Sql "SELECT dbo.fn_PN2_GrupoCandidato('COMP_TEST','X','?','Y','~')") 0 'Reinstalar conserva las bajas'
    Check (Sql 'SELECT COUNT(*) FROM dbo.CompatibilidadTransfusional') 76 'Reinstalar conserva reglas nuevas sin duplicar'
    Check (Regla $taskRegla 1) 'OK' 'Reactivación'
    $taskDenied = $false
    try { [void](Sql "EXEC dbo.sp_CompatibilidadTransfusional_Guardar 0,'COMP_TEST','X','?','Y','~',1,-1") } catch { $taskDenied = $true }
    Check $taskDenied $true 'Guardar exige permiso administrativo'
    $taskDenied = $false
    try { [void](Sql 'EXEC dbo.sp_CompatibilidadTransfusional_Listar -1') } catch { $taskDenied = $true }
    Check $taskDenied $true 'Consultar reglas administrativas exige permiso'
    Check (Sql "EXEC dbo.sp_Donante_Registrar 'COMP-DOC','Prueba','Configurable',NULL,NULL,NULL,NULL,1") 'OK' 'Donante de prueba'
    $taskDonante = [int](Sql "SELECT id_donante FROM dbo.Donante WHERE documento='COMP-DOC'")
    Check (Sql "DECLARE @f DATETIME=GETDATE(); EXEC dbo.sp_Donacion_Registrar $taskDonante,@f,1,'COMP_TEST','20991231',NULL,1") 'OK' 'Donación del componente nuevo'
    $taskUnidad = [int](Sql 'SELECT MAX(id_unidad) FROM dbo.Unidad')
    Check (Sql "EXEC dbo.sp_Unidad_Clasificar $taskUnidad,'COMP_TEST','X','?','20991231',NULL,1") 'OK' 'Unidad con grupo y Rh nuevos'
    Check (Sql "EXEC dbo.sp_Unidad_CambiarEstado $taskUnidad,'LIBERADA',NULL,1") 'OK' 'Unidad liberada'
    Check (Sql "EXEC dbo.sp_SolicitudTransfusional_Registrar N'Receptor de prueba','COMP_TEST',1,1,N'DOC-R','Y','~',N'Lab',N'Ref','20200101',N'Resultado',N'Profesional'") 'OK' 'Solicitud con valores nuevos'
    $taskSolicitud = [int](Sql 'SELECT MAX(id_solicitud_transfusional) FROM dbo.SolicitudTransfusional')
    Check (Sql "EXEC dbo.sp_PN2_UnidadesCandidatas $taskSolicitud,1") $null 'No hay candidatas sin estudio validado'
    Check (Sql "EXEC dbo.sp_PN2_ValidarEstudio $taskSolicitud,1") 'OK' 'Validación del estudio'
    Check (Sql "EXEC dbo.sp_PN2_UnidadesCandidatas $taskSolicitud,1") $taskUnidad 'Búsqueda utiliza la regla nueva'
    Check (Regla $taskRegla 0) 'OK' 'Desactivación antes de reservar'
    Check (Sql "EXEC dbo.sp_PN2_UnidadesCandidatas $taskSolicitud,1") $null 'La baja elimina candidatas'
    Check (Sql "EXEC dbo.sp_Reserva_Registrar $taskSolicitud,$taskUnidad,NULL,1,N'Confirmación'") 'SIN_DISPONIBILIDAD' 'La baja impide reservar'
    Check (Regla $taskRegla 1) 'OK' 'Restablecer regla para reserva'
    Check (Sql "EXEC dbo.sp_Reserva_Registrar $taskSolicitud,$taskUnidad,NULL,1,N'Confirmación'") 'OK' 'Reserva configurable'
    $taskReserva = [int](Sql 'SELECT MAX(id_reserva) FROM dbo.Reserva')
    Check (Regla $taskRegla 0) 'OK' 'Desactivación después de reservar'
    Check (Sql "EXEC dbo.sp_Entrega_Registrar $taskReserva,NULL,1") 'SIN_DISPONIBILIDAD' 'Entrega vuelve a evaluar la regla vigente'
    Check (Regla $taskRegla 1) 'OK' 'Restablecer regla para entrega'
    Check (Sql "EXEC dbo.sp_Entrega_Registrar $taskReserva,NULL,1") 'OK' 'Entrega configurable'
    Check (Sql "SELECT COUNT(*) FROM dbo.Auditoria WHERE entidad='CompatibilidadTransfusional' AND estado_nuevo_json IS NOT NULL") 7 'Cambios auditados'
    Check (Sql 'SELECT COUNT(*) FROM dbo.v_IntegridadPN1_Errores') 0 'Integridad de negocio conservada'
    if (Test-Path (Join-Path $PSScriptRoot 'bin/Debug/CompatibilidadUI.Tests.exe')) {
        & (Join-Path $PSScriptRoot 'bin/Debug/CompatibilidadUI.Tests.exe') $Servidor $taskDatabase $Captura
        if ($LASTEXITCODE -ne 0) { throw 'Falló la verificación de Application/UI.' }
    } else { throw 'Compilá Tests/CompatibilidadUI.Tests.csproj antes de ejecutar esta prueba.' }
    # Las modificaciones fuera del ABM deben bloquear consultas y operaciones.
    [void](Sql "UPDATE dbo.CompatibilidadTransfusional SET activo=0 WHERE id_compatibilidad=$taskRegla")
    $taskBlocked = $false
    try { [void](Sql "EXEC dbo.sp_PN2_UnidadesCandidatas $taskSolicitud,1") } catch { $taskBlocked = $true }
    Check $taskBlocked $true 'Alteración externa bloquea candidatas'
    $taskBlocked = $false
    try { Script 'StoredProcedures/06a_CompatibilidadTransfusional.sql' } catch { $taskBlocked = $true }
    Check $taskBlocked $true 'La migración no acepta reglas alteradas'
    [void](Sql "UPDATE dbo.CompatibilidadTransfusional SET activo=1 WHERE id_compatibilidad=$taskRegla")
    [void](Sql "UPDATE dbo.Auditoria SET cambios_json=N'[]' WHERE id_auditoria=(SELECT MIN(id_auditoria) FROM dbo.Auditoria WHERE entidad='CompatibilidadTransfusional')")
    $taskBlocked = $false
    try { [void](Sql 'EXEC dbo.sp_CompatibilidadTransfusional_Listar 1') } catch { $taskBlocked = $true }
    Check $taskBlocked $true 'Alteración del historial detectada'
    Write-Host 'Compatibilidad configurable: todas las verificaciones correctas.'
} finally {
    if ($taskCreated -and $taskConnection.State -eq 'Open' -and $taskDatabase -match '^HemovidaGest_Compat_Test_[a-f0-9]{32}$') {
        $taskConnection.ChangeDatabase('master')
        [void](Sql "ALTER DATABASE [$taskDatabase] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$taskDatabase]")
    }
    $taskConnection.Dispose()
}
