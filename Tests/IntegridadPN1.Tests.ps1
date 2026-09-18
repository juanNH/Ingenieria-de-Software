param([string]$Servidor = '.', [switch]$ConservarBase, [switch]$DatosExistentes)
$ErrorActionPreference = 'Stop'
$raizProyecto = Split-Path $PSScriptRoot -Parent
$salidaPruebas = Join-Path $PSScriptRoot 'bin/IntegrityTests'
& dotnet build (Join-Path $PSScriptRoot 'IntegridadUI.Tests.csproj') --no-restore -v:q "-p:OutputPath=$salidaPruebas"
if ($LASTEXITCODE -ne 0) { throw 'No compilan las pruebas de UI' }
$basePrueba = 'HemovidaGest_Integridad_Test_' + [Guid]::NewGuid().ToString('N')
$conexionPrueba = New-Object System.Data.SqlClient.SqlConnection "Server=$Servidor;Database=master;Integrated Security=True;TrustServerCertificate=True"
$conexionPrueba.Open()
function Sql([string]$texto) {
    $comando = $conexionPrueba.CreateCommand()
    $comando.CommandTimeout = 120
    $comando.CommandText = $texto
    $datos = New-Object System.Data.DataSet
    $adaptador = New-Object System.Data.SqlClient.SqlDataAdapter $comando
    try { [void]$adaptador.Fill($datos); return ,$datos }
    finally { $adaptador.Dispose(); $comando.Dispose() }
}
function Script([string]$ruta) {
    $contenido = (Get-Content -LiteralPath (Join-Path $raizProyecto $ruta) -Raw).Replace('HemovidaGest', $basePrueba)
    foreach ($lote in [regex]::Split($contenido, '(?im)^\s*GO\s*(?:--[^\r\n]*)?$')) {
        if ($lote.Trim()) { [void](Sql $lote) }
    }
    Write-Host "OK script $ruta"
}
function Assert([bool]$condicion, [string]$mensaje) {
    if (!$condicion) { throw "FAIL: $mensaje" }
    Write-Host "OK $mensaje"
}
function Rechaza([string]$texto, [int]$numero, [string]$mensaje) {
    $rechazado = $false
    try { [void](Sql $texto) } catch {
        $causa = $_.Exception
        while ($causa.InnerException) { $causa = $causa.InnerException }
        if ($causa -isnot [System.Data.SqlClient.SqlException] -or $causa.Number -ne $numero) { throw }
        $rechazado = $true
    }
    Assert $rechazado $mensaje
    Assert ((Sql 'SELECT @@TRANCOUNT AS n').Tables[0].Rows[0].n -eq 0) 'No deja transacciones abiertas'
}
function Verificar { return (Sql 'EXEC dbo.sp_IntegridadPN1_Verificar @id_usuario=1') }
function Restaurar {
    $revision = (Verificar).Tables[0].Rows[0].revision
    [void](Sql "EXEC dbo.sp_IntegridadPN1_Restaurar @id_usuario=1,@revision='$revision'")
    Assert ((Verificar).Tables[0].Rows[0].es_valida) 'Restauracion termina con integridad valida'
}
try {
    Script 'Database/tecni_salud_schema.sql'
    Script 'Database/StoredProcedures/Bitacora.sql'
    if ($DatosExistentes) {
        [void](Sql 'ALTER TABLE dbo.Donante DROP COLUMN dvh; ALTER TABLE dbo.Donacion DROP COLUMN dvh; ALTER TABLE dbo.Unidad DROP COLUMN dvh; ALTER TABLE dbo.MovimientoUnidad DROP COLUMN dvh; ALTER TABLE dbo.Auditoria DROP COLUMN dvh_integridad')
        [void](Sql "INSERT dbo.Donante(documento,nombre,apellido,id_usuario_alta) VALUES('D1','Maria','Prueba',1)")
    }
    Script 'Database/StoredProcedures/IntegridadPN1.sql'
    Script 'Database/StoredProcedures/PN1.sql'
    Script 'Database/StoredProcedures/IntegridadPN1.sql'
    $informe = Sql 'EXEC dbo.sp_IntegridadPN1_Verificar'
    if ($DatosExistentes) {
        Assert (!$informe.Tables[0].Rows[0].es_valida -and $informe.Tables[0].Rows[0].puede_inicializar) 'Los datos existentes requieren aceptar una base inicial'
    } else {
        Assert ($informe.Tables[0].Rows[0].es_valida -and !$informe.Tables[0].Rows[0].puede_inicializar) 'Una base vacia comienza valida sin aceptar una base inicial'
    }
    Assert ((Sql "SELECT COUNT(*) AS n FROM sys.parameters WHERE object_id IN (OBJECT_ID('dbo.sp_IntegridadPN1_Inicializar'),OBJECT_ID('dbo.sp_IntegridadPN1_Restaurar')) AND name='@motivo'").Tables[0].Rows[0].n -eq 0) 'Inicializar y restaurar no solicitan motivo'
    Assert ((Sql "SELECT COUNT(*) AS n FROM dbo.Etiqueta WHERE clave IN ('INTEGRITY_REASON','INTEGRITY_REASON_REQUIRED')").Tables[0].Rows[0].n -eq 0) 'El catalogo no conserva textos de motivo'
    Assert ((Sql "SELECT modulo FROM dbo.Permiso WHERE codigo='INTEGRIDAD_VER'").Tables[0].Rows[0].modulo -eq 'INTEGRIDAD') 'Integridad es un modulo transversal'
    $revision = $informe.Tables[0].Rows[0].revision
    [void](Sql "INSERT dbo.Usuario(nombre_usuario,email,password_hash,nombre,apellido,estado_usuario) VALUES('operador_test','test@example.invalid','test','Prueba','Prueba','ACTIVO')")
    if ($DatosExistentes) {
        Rechaza "EXEC dbo.sp_Donante_Registrar @documento='D0',@nombre='Prueba',@apellido='Prueba',@id_usuario=1" 51001 'Bloquea escrituras con datos sin base inicial'
        Rechaza "EXEC dbo.sp_IntegridadPN1_Inicializar @id_usuario=2,@revision='$revision'" 51003 'Un usuario sin permiso no inicializa'
        [void](Sql "EXEC dbo.sp_IntegridadPN1_Inicializar @id_usuario=1,@revision='$revision'")
        Assert ((Verificar).Tables[0].Rows[0].es_valida) 'Inicializa y verifica la base sin alterar datos'
        Rechaza "EXEC dbo.sp_IntegridadPN1_Inicializar @id_usuario=1,@revision='$revision'" 51004 'No reinicializa una base aceptada'
    }
    $documento = if ($DatosExistentes) { 'D-extra' } else { 'D1' }
    [void](Sql "EXEC dbo.sp_Donante_Registrar @documento='$documento',@nombre='Maria',@apellido='Prueba',@id_usuario=1")
    [void](Sql "EXEC dbo.sp_Donacion_Registrar @id_donante=1,@fecha_donacion='20260101',@cantidad_unidades=3,@tipo_componente='Sangre',@fecha_vencimiento='20991231',@id_usuario=1")
    foreach ($id in 1..3) {
        [void](Sql "EXEC dbo.sp_Unidad_Clasificar @id_unidad=$id,@tipo_componente='Sangre',@grupo_sanguineo='O',@factor_rh='+',@fecha_vencimiento='20991231',@id_usuario=1")
    }
    [void](Sql "EXEC dbo.sp_Unidad_CambiarEstado @id_unidad=1,@estado_nuevo='LIBERADA',@id_usuario=1")
    [void](Sql "EXEC dbo.sp_Unidad_CambiarEstado @id_unidad=2,@estado_nuevo='BLOQUEADA',@observacion='Prueba',@id_usuario=1")
    [void](Sql "EXEC dbo.sp_Unidad_CambiarEstado @id_unidad=3,@estado_nuevo='DESCARTADA',@observacion='Prueba',@id_usuario=1")
    Assert ((Verificar).Tables[0].Rows[0].es_valida) 'Altas, clasificacion y los tres estados mantienen DVH/DVV e historial'
    Assert ((Sql 'SELECT COUNT(*) AS n FROM dbo.v_IntegridadPN1_Datos WHERE dvh IS NULL').Tables[0].Rows[0].n -eq 0) 'Todas las filas PN1 tienen DVH'
    [void](Sql "CREATE USER EjecutorPN1Test WITHOUT LOGIN; GRANT EXECUTE ON dbo.sp_Donante_Registrar TO EjecutorPN1Test")
    try {
        [void](Sql "EXECUTE AS USER='EjecutorPN1Test'")
        Rechaza "EXEC dbo.sp_IntegridadPN1_Sellar @id_usuario=1" 229 'No permite ejecutar directamente el sellado'
    } finally { [void](Sql "IF @@TRANCOUNT>0 ROLLBACK; REVERT") }
    [void](Sql "EXECUTE AS USER='EjecutorPN1Test'")
    try {
        [void](Sql "EXEC dbo.sp_Donante_Registrar @documento='D-executor',@nombre='Test',@apellido='Test',@id_usuario=1")
    } finally { [void](Sql 'REVERT') }
    Assert ((Verificar).Tables[0].Rows[0].es_valida) 'El procedimiento autorizado puede sellar mediante ownership chaining'
    [void](Sql 'BEGIN TRANSACTION; EXEC dbo.sp_IntegridadPN1_Exigir')
    $segundaConexion = New-Object System.Data.SqlClient.SqlConnection "Server=$Servidor;Database=$basePrueba;Integrated Security=True"
    try {
        $segundaConexion.Open()
        $concurrente = $segundaConexion.CreateCommand()
        $concurrente.CommandText = "SET LOCK_TIMEOUT 500; UPDATE dbo.Donante SET nombre='Carrera' WHERE id_donante=1"
        $bloqueado = $false
        try { [void]$concurrente.ExecuteNonQuery() } catch {
            $causa = $_.Exception
            while ($causa.InnerException) { $causa = $causa.InnerException }
            if ($causa.Number -ne 1222) { throw }
            $bloqueado = $true
        }
        Assert $bloqueado 'La verificacion transaccional impide una escritura SQL concurrente'
    } finally { $segundaConexion.Dispose(); [void](Sql 'ROLLBACK TRANSACTION') }
    Assert ((Sql "EXEC dbo.sp_Unidad_Clasificar @id_unidad=1,@tipo_componente='Sangre',@grupo_sanguineo='O',@factor_rh='+',@fecha_vencimiento='20991231',@id_usuario=1").Tables[0].Rows[0].codigo_resultado -eq 'CONFLICTO_ESTADO') 'Respeta estados finales'
    Assert ((Sql 'SELECT @@TRANCOUNT AS n').Tables[0].Rows[0].n -eq 0) 'Validacion de negocio revierte la transaccion'
    foreach ($caso in @(
        @("UPDATE dbo.Donante SET nombre='Alterado' WHERE id_donante=1", 'Donante'),
        @("UPDATE dbo.Donacion SET observaciones='Alterado' WHERE id_donacion=1", 'Donacion'),
        @("UPDATE dbo.Unidad SET grupo_sanguineo='AB' WHERE id_unidad=1", 'Unidad'),
        @("UPDATE dbo.MovimientoUnidad SET observacion='Alterado' WHERE id_movimiento_unidad=1", 'MovimientoUnidad')
    )) {
        [void](Sql $caso[0])
        $errorDatos = Verificar
        Assert (!$errorDatos.Tables[0].Rows[0].es_valida) "Detecta alteracion de $($caso[1])"
        Assert ((Sql "SELECT bloqueo_digitoverificador AS bloqueado FROM dbo.Usuario WHERE id_usuario=2").Tables[0].Rows[0].bloqueado) "Bloquea usuarios no administradores ante alteracion de $($caso[1])"
        $eventos = (Sql "SELECT COUNT(*) AS n FROM dbo.Bitacora WHERE accion='IntegridadDetectada_380_jh'").Tables[0].Rows[0].n
        [void](Verificar)
        Assert ((Sql "SELECT COUNT(*) AS n FROM dbo.Bitacora WHERE accion='IntegridadDetectada_380_jh'").Tables[0].Rows[0].n -eq $eventos) 'No duplica incidentes al verificar nuevamente'
        Rechaza "EXEC dbo.sp_Donante_Registrar @documento='D2',@nombre='Prueba',@apellido='Prueba',@id_usuario=1" 51001 'Bloquea todo PN1 aunque falle otra tabla'
        Assert ((Sql 'EXEC dbo.sp_Unidad_Listar').Tables[0].Rows.Count -eq 3) 'Mantiene disponibles las consultas'
        Restaurar
        Assert (!((Sql "SELECT bloqueo_digitoverificador AS bloqueado FROM dbo.Usuario WHERE id_usuario=2").Tables[0].Rows[0].bloqueado)) "Libera usuarios no administradores al restaurar $($caso[1])"
    }
    [void](Sql "UPDATE dbo.Donante SET telefono='' WHERE id_donante=1")
    Assert (!$((Verificar).Tables[0].Rows[0].es_valida)) 'Distingue NULL de cadena vacia'
    $revision = (Verificar).Tables[0].Rows[0].revision
    Rechaza "EXEC dbo.sp_IntegridadPN1_Restaurar @id_usuario=2,@revision='$revision'" 51003 'Operador sin permiso no restaura'
    [void](Sql "UPDATE dbo.Donante SET telefono='Cambio posterior' WHERE id_donante=1")
    Rechaza "EXEC dbo.sp_IntegridadPN1_Restaurar @id_usuario=1,@revision='$revision'" 51005 'Rechaza una previsualizacion desactualizada'
    Restaurar
    [void](Sql "UPDATE dbo.DigitoVerificadorVertical SET dvv=REPLICATE('0',64) WHERE entidad='Unidad'")
    Restaurar
    [void](Sql "UPDATE dbo.Donante SET nombre='Alterado' WHERE id_donante=1")
    $revision = (Verificar).Tables[0].Rows[0].revision
    [void](Sql 'GRANT EXECUTE ON dbo.sp_IntegridadPN1_Restaurar TO EjecutorPN1Test')
    [void](Sql "EXECUTE AS USER='EjecutorPN1Test'")
    try {
        [void](Sql "EXEC dbo.sp_IntegridadPN1_Restaurar @id_usuario=1,@revision='$revision'")
    } finally { [void](Sql 'REVERT') }
    Assert ((Verificar).Tables[0].Rows[0].es_valida) 'La recuperacion funciona sin sysadmin ni db_owner'
    [void](Sql "UPDATE dbo.Donante SET nombre='Hash recalculado externamente' WHERE id_donante=1")
    [void](Sql "UPDATE d SET dvh=v.dvh_calculado FROM dbo.Donante d JOIN dbo.v_IntegridadPN1_Datos v ON v.entidad='Donante' AND v.id_entidad=d.id_donante; UPDATE d SET dvv=v.dvv_calculado FROM dbo.DigitoVerificadorVertical d JOIN dbo.v_IntegridadPN1_Vertical v ON v.entidad=d.entidad WHERE d.entidad='Donante'")
    $errorVersion = Verificar
    Assert ((@($errorVersion.Tables[1].Rows | Where-Object tipo -eq 'VERSION_DIFERENTE')).Count -gt 0) 'Comparar el historial detecta un recalcado externo de DVH/DVV'
    Restaurar
    [void](Sql "UPDATE dbo.Unidad SET grupo_sanguineo='AB' WHERE id_unidad=1")
    [void](Sql "CREATE TRIGGER dbo.PruebaFalloRestauracion ON dbo.MovimientoUnidad AFTER INSERT AS IF EXISTS(SELECT 1 FROM inserted WHERE tipo_movimiento='RESTAURACION') THROW 51999,'Fallo de prueba',1;")
    $revision = (Verificar).Tables[0].Rows[0].revision
    $auditorias = (Sql 'SELECT COUNT(*) AS n FROM dbo.Auditoria').Tables[0].Rows[0].n
    Rechaza "EXEC dbo.sp_IntegridadPN1_Restaurar @id_usuario=1,@revision='$revision'" 51999 'Fallo intermedio revierte toda la restauracion'
    Assert ((Sql 'SELECT grupo_sanguineo FROM dbo.Unidad WHERE id_unidad=1').Tables[0].Rows[0].grupo_sanguineo -eq 'AB') 'No deja datos parcialmente restaurados'
    Assert ((Sql 'SELECT COUNT(*) AS n FROM dbo.Auditoria').Tables[0].Rows[0].n -eq $auditorias) 'No deja versiones de una restauracion fallida'
    [void](Sql 'DROP TRIGGER dbo.PruebaFalloRestauracion')
    Restaurar
    [void](Sql 'DELETE dbo.MovimientoUnidad; DELETE dbo.Unidad; DELETE dbo.Donacion; DELETE dbo.Donante')
    Assert (!$((Verificar).Tables[0].Rows[0].es_valida)) 'DVV detecta eliminacion completa de tablas'
    Restaurar
    Assert ((Sql 'SELECT COUNT(*) AS n FROM dbo.Unidad WHERE id_unidad IN (1,2,3)').Tables[0].Rows[0].n -eq 3) 'Recupera eliminados con IDs y relaciones originales'
    [void](Sql "INSERT dbo.Donante(documento,nombre,apellido,id_usuario_alta) VALUES('Desconocido','Prueba','Prueba',1)")
    $revision = (Verificar).Tables[0].Rows[0].revision
    Rechaza "EXEC dbo.sp_IntegridadPN1_Restaurar @id_usuario=1,@revision='$revision'" 51006 'No legitima filas insertadas fuera de la app'
    [void](Sql "DELETE dbo.Donante WHERE documento='Desconocido'")
    Assert ((Verificar).Tables[0].Rows[0].es_valida) 'Correccion externa exacta vuelve a verificar sin recalculo'
    & (Join-Path $salidaPruebas 'IntegridadUI.Tests.exe') $Servidor $basePrueba
    if ($LASTEXITCODE -ne 0) { throw 'Fallo la prueba de UI/Application/DAL' }
    Assert ((Sql "SELECT COUNT(*) AS n FROM dbo.Bitacora WHERE accion='RestauracionFallida_380_jh'").Tables[0].Rows[0].n -gt 0) 'Intento no autorizado queda en bitacora'
    [void](Sql "UPDATE dbo.Auditoria SET estado_nuevo_json=N'{}' WHERE id_auditoria=(SELECT MIN(id_auditoria) FROM dbo.Auditoria WHERE entidad='Donante')")
    $revision = (Verificar).Tables[0].Rows[0].revision
    Rechaza "EXEC dbo.sp_IntegridadPN1_Restaurar @id_usuario=1,@revision='$revision'" 51006 'Historial corrupto exige backup'
    Assert ((Sql "SELECT COUNT(*) AS n FROM dbo.Bitacora WHERE accion='IntegridadRestaurada_380_jh'").Tables[0].Rows[0].n -ge 8) 'Recuperaciones registradas en bitacora'
}
finally {
    $conexionPrueba.ChangeDatabase('master')
    if (!$ConservarBase -and $basePrueba -match '^HemovidaGest_Integridad_Test_[a-f0-9]{32}$') {
        [void](Sql "IF DB_ID('$basePrueba') IS NOT NULL BEGIN ALTER DATABASE [$basePrueba] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$basePrueba]; END")
        Write-Host "Base temporal eliminada: $basePrueba"
    } else { Write-Host "Base temporal conservada: $basePrueba" }
    $conexionPrueba.Dispose()
}
