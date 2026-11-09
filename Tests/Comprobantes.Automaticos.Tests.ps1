param([string]$Servidor = '.')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$database = 'HemovidaGest_Comprobantes_Test_' + [guid]::NewGuid().ToString('N')
$folder = Join-Path ([IO.Path]::GetTempPath()) ('hemovida-auto-test-' + [guid]::NewGuid().ToString('N'))
$connection = New-Object System.Data.SqlClient.SqlConnection "Server=$Servidor;Database=master;Integrated Security=True;TrustServerCertificate=True"
$connection.Open()
function Sql([string]$commandText) {
    $command = $connection.CreateCommand(); $command.CommandText = $commandText; $command.CommandTimeout = 120
    $set = New-Object System.Data.DataSet; $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $command
    try { [void]$adapter.Fill($set); return ,$set } finally { $adapter.Dispose(); $command.Dispose() }
}
function Script([string]$path) {
    $source = [IO.File]::ReadAllText((Join-Path $root $path)).Replace('HemovidaGest', $database)
    foreach ($batch in [regex]::Split($source, '(?im)^\s*GO\s*(?:--[^\r\n]*)?$')) { if ($batch.Trim()) { [void](Sql $batch) } }
}
function Assert([bool]$value, [string]$message) { if (!$value) { throw $message }; Write-Host "OK $message" }
function CountPending([string]$where) { return [int](Sql "SELECT COUNT(*) n FROM dbo.ComprobantePendiente WHERE $where").Tables[0].Rows[0].n }
function RegisterCompatibleRequest {
    $request = Sql @'
EXEC dbo.sp_SolicitudTransfusional_Registrar
    @paciente_receptor=N'Paciente',@tipo_componente=N'Concentrado de eritrocitos',@cantidad_requerida=1,@id_usuario=1,
    @documento_receptor=N'DEMO-COMP',@grupo_receptor='O',@rh_receptor='+',
    @laboratorio_estudio=N'Laboratorio demo',@referencia_estudio=N'EST-COMP',@fecha_estudio='20260101',
    @resultado_anticuerpos=N'No detectados',@nombre_solicitante=N'Profesional demo';
'@
    $id = [int]$request.Tables[0].Rows[0].id_resultado
    [void](Sql "EXEC dbo.sp_PN2_ValidarEstudio @id_solicitud=$id,@id_usuario=1")
    return $id
}
try {
    New-Item -ItemType Directory -Path $folder | Out-Null
    foreach ($script in @('Database/00_tecni_salud_schema.sql','Database/StoredProcedures/01_AuthUsuario.sql',
        'Database/StoredProcedures/02_Bitacora.sql','Database/StoredProcedures/03_Auditoria.sql',
        'Database/StoredProcedures/04_Permisos.sql','Database/StoredProcedures/05_IntegridadPN1.sql',
        'Database/StoredProcedures/06_MaestrosPN1.sql','Database/StoredProcedures/06a_CompatibilidadTransfusional.sql','Database/StoredProcedures/06b_TipificacionDonante.sql','Database/StoredProcedures/07_PN1.sql',
        'Database/StoredProcedures/08_PN2_Catalogos.sql','Database/StoredProcedures/09_PN2.sql',
        'Database/StoredProcedures/09a_RelacionesSolicitudPN2.sql',
        'Database/StoredProcedures/10_IntegridadPN2.sql','Database/StoredProcedures/11_ComprobantesAutomaticos.sql',
        'Database/StoredProcedures/13_AjusteCantidadDonacion.sql')) { Script $script }
    Script 'Database/StoredProcedures/11_ComprobantesAutomaticos.sql'
    [void](Sql "EXEC dbo.sp_Donante_Registrar @documento='PRUEBA',@nombre='Maria',@apellido='Prueba',@id_usuario=1")
    [void](Sql "EXEC dbo.sp_Donacion_Registrar @id_donante=1,@fecha_donacion='20260101',@cantidad_unidades=3,@tipo_componente='Concentrado de eritrocitos',@fecha_vencimiento='20991231',@id_usuario=1")
    foreach ($id in 1..3) { [void](Sql "EXEC dbo.sp_Unidad_Clasificar @id_unidad=$id,@tipo_componente='Concentrado de eritrocitos',@grupo_sanguineo='O',@factor_rh='+',@fecha_vencimiento='20991231',@id_usuario=1") }
    [void](Sql "EXEC dbo.sp_Unidad_CambiarEstado @id_unidad=1,@estado_nuevo='LIBERADA',@id_usuario=1")
    Assert ((CountPending "proceso='PN1'") -eq 0) 'PN1 espera a todas las unidades'
    $request = RegisterCompatibleRequest
    [void](Sql "EXEC dbo.sp_Reserva_Registrar @id_solicitud_transfusional=$request,@id_unidad=1,@id_usuario=1,@referencia_compatibilidad=N'Cruce demo'")
    [void](Sql 'EXEC dbo.sp_Entrega_Registrar @id_reserva=1,@id_usuario=1')
    Assert ((CountPending "proceso='PN2'") -eq 0) 'PN2 espera el destino'
    [void](Sql "EXEC dbo.sp_Entrega_RegistrarDestino @id_entrega=1,@destino='DEVUELTA',@motivo='prueba',@id_usuario=1")
    Assert ((CountPending "proceso='PN2'") -eq 1) 'Destino devuelto crea un comprobante'
    [void](Sql "EXEC dbo.sp_Unidad_CambiarEstado @id_unidad=2,@estado_nuevo='BLOQUEADA',@observacion='prueba',@id_usuario=1")
    [void](Sql "BEGIN TRANSACTION; EXEC dbo.sp_Unidad_CambiarEstado @id_unidad=3,@estado_nuevo='DESCARTADA',@observacion='prueba',@id_usuario=1; ROLLBACK")
    Assert ((CountPending "proceso='PN1'") -eq 0) 'Un rollback revierte el evento de PN1'
    [void](Sql "EXEC dbo.sp_Unidad_CambiarEstado @id_unidad=3,@estado_nuevo='DESCARTADA',@observacion='prueba',@id_usuario=1")
    Assert ((CountPending "proceso='PN1'") -eq 1) 'PN1 termina aunque una unidad haya avanzado a PN2'
    $snapshot = [xml](Sql "SELECT CONVERT(NVARCHAR(MAX),datos) xml_datos FROM dbo.ComprobantePendiente WHERE proceso='PN1'").Tables[0].Rows[0].xml_datos
    Assert ($snapshot.Comprobante.Unidades.Unidad.Count -eq 3 -and $snapshot.Comprobante.Unidades.Unidad[0].EstadoOperativo_380_jh -eq 'LIBERADA') 'La instantánea conserva la decisión de PN1'
    [void](Sql "EXEC dbo.sp_Donacion_Registrar @id_donante=1,@fecha_donacion='20260101',@cantidad_unidades=2,@tipo_componente='Concentrado de eritrocitos',@fecha_vencimiento='20991231',@id_usuario=1")
    foreach ($id in 4..5) {
        [void](Sql "EXEC dbo.sp_Unidad_Clasificar @id_unidad=$id,@tipo_componente='Concentrado de eritrocitos',@grupo_sanguineo='O',@factor_rh='+',@fecha_vencimiento='20991231',@id_usuario=1")
        [void](Sql "EXEC dbo.sp_Unidad_CambiarEstado @id_unidad=$id,@estado_nuevo='LIBERADA',@id_usuario=1")
        $request = RegisterCompatibleRequest
        $reservation = $id - 2
        [void](Sql "EXEC dbo.sp_Reserva_Registrar @id_solicitud_transfusional=$request,@id_unidad=$id,@id_usuario=1,@referencia_compatibilidad=N'Cruce demo'")
        [void](Sql "EXEC dbo.sp_Entrega_Registrar @id_reserva=$reservation,@id_usuario=1")
        $destination = if ($id -eq 4) { 'TRANSFUNDIDA' } else { 'DESCARTADA' }
        [void](Sql "EXEC dbo.sp_Entrega_RegistrarDestino @id_entrega=$reservation,@destino='$destination',@motivo='prueba',@id_usuario=1")
    }
    Assert ((CountPending '1=1') -eq 5) 'Se registran dos donaciones y los tres destinos'
    [void](Sql "EXEC dbo.sp_Entrega_RegistrarDestino @id_entrega=1,@destino='DEVUELTA',@motivo='repetido',@id_usuario=1")
    Assert ((CountPending '1=1') -eq 5) 'No se duplican los comprobantes al rechazar una repetición'
    Assert ((Sql 'SELECT COUNT(*) n FROM dbo.v_IntegridadPN1_Errores').Tables[0].Rows[0].n -eq 0) 'La integridad de negocio sigue válida'
    [void](Sql "INSERT dbo.Usuario(nombre_usuario,email,password_hash,nombre,apellido,estado_usuario) VALUES('sin_permiso','sinpermiso@example.invalid','test','Sin','Permiso','ACTIVO')")
    $other = [int](Sql "SELECT id_usuario FROM dbo.Usuario WHERE nombre_usuario='sin_permiso'").Tables[0].Rows[0].id_usuario
    [void](Sql "UPDATE dbo.ComprobantePendiente SET id_usuario=$other WHERE proceso='PN1' AND id_referencia=1")
    Assert ((Sql "EXEC dbo.sp_Comprobante_Pendientes @id_usuario=$other").Tables[0].Rows.Count -eq 0) 'Un usuario sin permisos no puede procesar pendientes'
    Assert ((Sql 'EXEC dbo.sp_Comprobante_Pendientes @id_usuario=1').Tables[0].Rows.Count -eq 5) 'Un administrador puede recuperar pendientes de otros usuarios'

    Copy-Item -Path (Join-Path $root 'UI/bin/Release/*.dll') -Destination $folder
    $connectionString = "Data Source=$Servidor;Initial Catalog=$database;Integrated Security=SSPI;TrustServerCertificate=True"
    [IO.File]::WriteAllText((Join-Path $folder 'App.local.config'), "<configuration><connectionStrings><add name='HemovidaGest' connectionString='$connectionString' /></connectionStrings></configuration>")
    foreach ($name in @('Abstractions','Domain','DAL','Repository','PdfSharp','Services','Application')) { Add-Type -Path (Join-Path $folder ($name + '.dll')) }
    $user = New-Object Domain.Usuario_380_jh; $user.Id_380_jh = 1; $user.Username_380_jh = 'admin'
    [Domain.Sesion_380_jh]::ObtenerInstancia_380_jh().IniciarSesion_380_jh($user)
    $bad = Join-Path $folder 'not-a-directory'; [IO.File]::WriteAllText($bad,'x')
    $service = New-Object Application.ComprobanteAutomaticoApplicationService_380_jh($bad)
    $result = $service.ProcesarPendientes_380_jh()
    Assert ($result.Pendientes_380_jh -eq 5 -and (CountPending 'fecha_generacion IS NULL') -eq 5) 'Un error de archivo conserva pendientes y operaciones'
    $output = Join-Path $folder 'salida'
    $service = New-Object Application.ComprobanteAutomaticoApplicationService_380_jh($output)
    $result = $service.ProcesarPendientes_380_jh()
    Assert ($result.Archivos_380_jh.Count -eq 5 -and $result.Pendientes_380_jh -eq 0) 'El reintento genera los cinco PDF'
    Assert ((CountPending 'fecha_generacion IS NULL') -eq 0) 'El reintento marca los comprobantes generados'
    Assert (@(Get-ChildItem -Path $output -Filter '*.pdf' -Recurse).Count -eq 5) 'Los cinco PDF existen en las carpetas por proceso'
    $original = $service.ObtenerOriginal_380_jh('PN1', 1)
    Assert ((Test-Path -LiteralPath $original) -and $original -like '*PN1*') 'Se puede recuperar el original para reimpresión'
    $hash = (Get-FileHash -LiteralPath $original -Algorithm SHA256).Hash
    [void](Sql "UPDATE dbo.ComprobantePendiente SET fecha_generacion=NULL,ruta=NULL WHERE proceso='PN1' AND id_referencia=1")
    $result = $service.ProcesarPendientes_380_jh()
    Assert ($result.Archivos_380_jh.Count -eq 1 -and (Get-FileHash -LiteralPath $original -Algorithm SHA256).Hash -eq $hash) 'Reintentar después de guardar reutiliza el original sin sobrescribirlo'
    $rejected = $false
    try { [void](Sql "EXEC dbo.sp_Comprobante_Original @proceso='PN1',@id_referencia=1,@id_usuario=$other") } catch { $rejected = $true }
    Assert $rejected 'La consulta del original exige permiso de unidades'
    $result = $service.ProcesarPendientes_380_jh()
    Assert ($result.Archivos_380_jh.Count -eq 0) 'Reintentar nuevamente no duplica archivos'

    Script 'Database/StoredProcedures/13_AjusteCantidadDonacion.sql'
    $donacion = Sql "EXEC dbo.sp_Donacion_Registrar @id_donante=1,@fecha_donacion='20260101',@cantidad_unidades=2,@tipo_componente='Concentrado de eritrocitos',@fecha_vencimiento='20991231',@id_usuario=1"
    $idDonacion = [int]$donacion.Tables[0].Rows[0].id_donacion
    $unidadClasificada = [int]$donacion.Tables[1].Rows[0].id_unidad
    $unidadAnulable = [int]$donacion.Tables[1].Rows[1].id_unidad
    $codigoAnulable = [string]$donacion.Tables[1].Rows[1].codigo_identificacion
    $consulta = Sql "EXEC dbo.sp_Donacion_ConsultarCantidad $idDonacion,1"
    $versionInicial = [int]$consulta.Tables[0].Rows[0].version_actual
    Assert ($consulta.Tables[1].Rows.Count -eq 1 -and $consulta.Tables[1].Rows[0].cantidad_nueva -eq 2) 'El historial muestra la cantidad inicial'
    [void](Sql "EXEC dbo.sp_Unidad_Clasificar @id_unidad=$unidadClasificada,@tipo_componente='Concentrado de eritrocitos',@grupo_sanguineo='O',@factor_rh='+',@fecha_vencimiento='20991231',@id_usuario=1")
    function Assert-RechazoAjuste([string]$sqlAjuste, [int]$numero, [string]$mensaje) {
        $rechazado = $false
        try { [void](Sql $sqlAjuste) } catch { $rechazado = $_.Exception.GetBaseException().Number -eq $numero }
        Assert $rechazado $mensaje
    }
    Assert-RechazoAjuste "EXEC dbo.sp_Donacion_AjustarCantidad $idDonacion,$versionInicial,3,N'Sin permiso',N'[]',$other" 51003 'El trigger verifica permisos'
    Assert-RechazoAjuste "EXEC dbo.sp_Donacion_AjustarCantidad $idDonacion,$versionInicial,1,N'Reducción',N'[$unidadClasificada]',1" 51124 'Una unidad clasificada no puede anularse'
    [void](Sql "EXEC dbo.sp_Donacion_AjustarCantidad $idDonacion,$versionInicial,1,N'Reducción',N'[$unidadAnulable]',1")
    $consulta = Sql "EXEC dbo.sp_Donacion_ConsultarCantidad $idDonacion,1"
    $versionReduccion = [int]$consulta.Tables[0].Rows[0].version_actual
    $ajuste = $consulta.Tables[1].Rows[0]
    Assert ($consulta.Tables[0].Rows[0].cantidad_unidades -eq 1 -and $ajuste.cantidad_anterior -eq 2 -and $ajuste.cantidad_nueva -eq 1 -and $ajuste.motivo -eq 'Reducción' -and $ajuste.identificador_usuario_actor -eq 'admin' -and $ajuste.unidades_afectadas -eq $codigoAnulable) 'La reducción conserva código, responsable, motivo y cantidades en el historial'
    Assert-RechazoAjuste "EXEC dbo.sp_Donacion_AjustarCantidad $idDonacion,$versionInicial,2,N'Versión anterior',N'[]',1" 51122 'Se rechaza una versión desactualizada'
    [void](Sql "EXEC dbo.sp_Donacion_AjustarCantidad $idDonacion,$versionReduccion,2,N'Restaurar cantidad inicial',N'[]',1,$versionInicial")
    $consulta = Sql "EXEC dbo.sp_Donacion_ConsultarCantidad $idDonacion,1"
    $versionRestaurada = [int]$consulta.Tables[0].Rows[0].version_actual
    $ajuste = $consulta.Tables[1].Rows[0]
    Assert ($ajuste.accion -eq 'RESTAURA_CANTIDAD' -and $ajuste.cantidad_nueva -eq 2 -and $ajuste.unidades_afectadas -ne $codigoAnulable -and $consulta.Tables[1].Rows.Count -eq 3) 'Restaurar agrega una versión y genera un código nuevo'
    [void](Sql "ALTER TABLE dbo.Auditoria ADD CONSTRAINT CK_PruebaFalloAjuste CHECK (identificador_usuario_actor<>'admin' OR cambios_json NOT LIKE N'%Forzar error%')")
    Assert-RechazoAjuste "EXEC dbo.sp_Donacion_AjustarCantidad $idDonacion,$versionRestaurada,3,N'Forzar error',N'[]',1" 547 'Se fuerza un fallo al escribir la auditoría'
    [void](Sql 'ALTER TABLE dbo.Auditoria DROP CONSTRAINT CK_PruebaFalloAjuste')
    $consulta = Sql "EXEC dbo.sp_Donacion_ConsultarCantidad $idDonacion,1"
    Assert ($consulta.Tables[0].Rows[0].version_actual -eq $versionRestaurada -and $consulta.Tables[0].Rows[0].cantidad_unidades -eq 2 -and $consulta.Tables[2].Rows.Count -eq 1) 'El fallo revierte cantidad, nuevas unidades, movimientos e historial'
    [void](Sql "EXEC dbo.sp_Unidad_CambiarEstado @id_unidad=$unidadClasificada,@estado_nuevo='BLOQUEADA',@observacion='prueba',@id_usuario=1")
    $excedente = [int]$consulta.Tables[2].Rows[0].id_unidad
    [void](Sql "EXEC dbo.sp_Donacion_AjustarCantidad $idDonacion,$versionRestaurada,1,N'Cierre por ajuste',N'[$excedente]',1")
    $consulta = Sql "EXEC dbo.sp_Donacion_ConsultarCantidad $idDonacion,1"
    Assert (!$consulta.Tables[0].Rows[0].editable -and (CountPending "proceso='PN1' AND id_referencia=$idDonacion AND datos.value('count(/Comprobante/Unidades/Unidad)','int')=1") -eq 1) 'El comprobante incluye solo unidades vigentes y deja el historial en consulta'
    $versionCerrada = [int]$consulta.Tables[0].Rows[0].version_actual
    Assert-RechazoAjuste "EXEC dbo.sp_Donacion_AjustarCantidad $idDonacion,$versionCerrada,2,N'Cerrada',N'[]',1" 51123 'Se rechaza el ajuste de una donación con comprobante'
    $versionLiberada = [int](Sql 'EXEC dbo.sp_Donacion_ConsultarCantidad 1,1').Tables[0].Rows[0].version_actual
    Assert-RechazoAjuste "EXEC dbo.sp_Donacion_AjustarCantidad 1,$versionLiberada,4,N'Cerrada',N'[]',1" 51123 'Se rechaza el ajuste de una donación liberada'
    Assert ((Sql 'SELECT COUNT(*) n FROM dbo.v_IntegridadPN1_Errores').Tables[0].Rows[0].n -eq 0) 'El ajuste conserva la integridad de PN1'
    Write-Host "PDF de prueba: $output"
}
finally {
    $connection.ChangeDatabase('master')
    if ($database -match '^HemovidaGest_Comprobantes_Test_[a-f0-9]{32}$') {
        [void](Sql "IF DB_ID('$database') IS NOT NULL BEGIN ALTER DATABASE [$database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$database]; END")
    }
    $connection.Dispose()
}
