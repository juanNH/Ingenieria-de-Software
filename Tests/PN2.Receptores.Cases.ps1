# Se ejecuta desde PN2.Compatibilidad.Tests.ps1 exclusivamente en su clon descartable.
if ($taskClone -notmatch '^CodexPN2Check_[a-f0-9]{12}$' -or $taskConnection.Database -ne $taskClone) {
    throw 'Las pruebas de receptores requieren la base aislada de PN2.'
}
$taskResult = Invoke-TaskSql "EXEC dbo.sp_SolicitudTransfusional_Registrar @paciente_receptor=N'Receptor recurrente',@tipo_componente=N'Plasma',@cantidad_requerida=1,@id_usuario=$taskUser,@documento_receptor=N' REUSE-1 ',@grupo_receptor='A',@rh_receptor='+',@laboratorio_estudio=NULL,@referencia_estudio=NULL,@fecha_estudio=NULL,@resultado_anticuerpos=NULL,@nombre_solicitante=N'Profesional'"
if ($taskResult -ne 'OK') { throw 'No se pudo guardar grupo y Rh sin copiar un estudio.' }
$taskReuseRequest = [int](Invoke-TaskSql "SELECT MAX(id_solicitud_transfusional) FROM dbo.SolicitudTransfusional WHERE documento_receptor=N'REUSE-1'")
if ((Invoke-TaskSql "EXEC dbo.sp_PN2_ValidarEstudio @id_solicitud=$taskReuseRequest,@id_usuario=$taskUser") -ne 'DATOS_INVALIDOS') { throw 'Se valido un pedido que solo tiene grupo y Rh.' }
if ($null -ne (Invoke-TaskSql "EXEC dbo.sp_PN2_UnidadesCandidatas @id_solicitud=$taskReuseRequest,@id_usuario=$taskUser")) { throw 'Se habilitaron candidatos por reutilizar el grupo.' }
$taskResult = Invoke-TaskSql "EXEC dbo.sp_SolicitudTransfusional_Registrar @paciente_receptor=N'Receptor recurrente actualizado',@tipo_componente=N'Plasma',@cantidad_requerida=2,@id_usuario=$taskUser,@documento_receptor=N'REUSE-1',@grupo_receptor=NULL,@rh_receptor=NULL,@laboratorio_estudio=NULL,@referencia_estudio=NULL,@fecha_estudio=NULL,@resultado_anticuerpos=NULL,@nombre_solicitante=N'Profesional'"
if ($taskResult -ne 'OK') { throw 'No se pudo repetir una solicitud para el mismo receptor.' }
[void](Invoke-TaskSql "DECLARE @r TABLE(documento nvarchar(50),nombre nvarchar(150),grupo varchar(5),rh varchar(5));
INSERT @r EXEC dbo.sp_PN2_Receptores_Listar @buscar=N'REUSE-1',@id_usuario=$taskUser;
IF (SELECT COUNT(*) FROM @r)<>1 THROW 51000,'RECIPIENT_DUPLICATED',1;
IF NOT EXISTS(SELECT 1 FROM @r WHERE nombre=N'Receptor recurrente actualizado' AND grupo='A' AND rh='+') THROW 51000,'RECIPIENT_BLOOD_GROUP_LOST',1;
DELETE @r; INSERT @r EXEC dbo.sp_PN2_Receptores_Listar @buscar=N'recurrente actualizado',@id_usuario=$taskUser;
IF (SELECT COUNT(*) FROM @r WHERE documento=N'REUSE-1')<>1 THROW 51000,'RECIPIENT_NAME_SEARCH',1;
DELETE @r; INSERT @r EXEC dbo.sp_PN2_Receptores_Listar @buscar=N'DOC2',@id_usuario=$taskUser;
IF NOT EXISTS(SELECT 1 FROM @r WHERE grupo='O' AND rh='-') THROW 51000,'EXISTING_RECIPIENT_NOT_IMPORTED',1;
DELETE @r; INSERT @r EXEC dbo.sp_PN2_Receptores_Listar @buscar=N'DOC3',@id_usuario=$taskUser;
IF NOT EXISTS(SELECT 1 FROM @r WHERE grupo IS NULL AND rh IS NULL) THROW 51000,'UNKNOWN_GROUP_NOT_PRESERVED',1;")
# Mismo nombre, distinto documento: mantener identidades independientes.
$taskResult = Invoke-TaskSql "EXEC dbo.sp_SolicitudTransfusional_Registrar @paciente_receptor=N'Receptor recurrente actualizado',@tipo_componente=N'Plasma',@cantidad_requerida=1,@id_usuario=$taskUser,@documento_receptor=N'REUSE-2',@grupo_receptor='B',@rh_receptor='-',@laboratorio_estudio=NULL,@referencia_estudio=NULL,@fecha_estudio=NULL,@resultado_anticuerpos=NULL,@nombre_solicitante=N'Profesional'"
if ($taskResult -ne 'OK') { throw 'No se pudo registrar un homonimo.' }
[void](Invoke-TaskSql "DECLARE @r TABLE(documento nvarchar(50),nombre nvarchar(150),grupo varchar(5),rh varchar(5));
INSERT @r EXEC dbo.sp_PN2_Receptores_Listar @buscar=N'recurrente actualizado',@id_usuario=$taskUser;
IF (SELECT COUNT(*) FROM @r)<>2 THROW 51000,'RECIPIENT_NAMES_MERGED',1;")
$taskResult = Invoke-TaskSql "EXEC dbo.sp_SolicitudTransfusional_Registrar @paciente_receptor=N'Estudio parcial',@tipo_componente=N'Plasma',@cantidad_requerida=1,@id_usuario=$taskUser,@documento_receptor=N'REUSE-PARTIAL',@grupo_receptor='A',@rh_receptor='+',@laboratorio_estudio=N'Lab',@referencia_estudio=NULL,@fecha_estudio=NULL,@resultado_anticuerpos=NULL,@nombre_solicitante=N'Profesional'"
if ($taskResult -ne 'DATOS_INVALIDOS') { throw 'Se acepto un informe parcial al reutilizar grupo y Rh.' }

# Restaurar la autorizacion real de lectura para comprobar alcance y denegacion.
[void](Invoke-TaskSql $taskReadAuthorization)
[void](Invoke-TaskSql "INSERT dbo.UsuarioComponentePermiso(id_usuario,id_componente) SELECT $taskUser,id_componente FROM dbo.ComponentePermiso WHERE codigo='PN2_PROFESIONAL_SOLICITANTE';")
$taskOtherUser = [int](Invoke-TaskSql "INSERT dbo.Usuario(nombre_usuario,email,password_hash) VALUES('otro_receptores','otro_receptores@example.invalid','prueba'); SELECT CONVERT(int,SCOPE_IDENTITY());")
[void](Invoke-TaskSql "INSERT dbo.UsuarioComponentePermiso(id_usuario,id_componente) SELECT $taskOtherUser,id_componente FROM dbo.ComponentePermiso WHERE codigo='PN2_PROFESIONAL_SOLICITANTE';")
$taskResult = Invoke-TaskSql "EXEC dbo.sp_SolicitudTransfusional_Registrar @paciente_receptor=N'Receptor privado',@tipo_componente=N'Plasma',@cantidad_requerida=1,@id_usuario=$taskOtherUser,@documento_receptor=N'REUSE-1',@grupo_receptor='O',@rh_receptor='-',@laboratorio_estudio=NULL,@referencia_estudio=NULL,@fecha_estudio=NULL,@resultado_anticuerpos=NULL,@nombre_solicitante=N'Otro profesional'"
if ($taskResult -ne 'OK') { throw 'No se pudo preparar el receptor de otro profesional.' }
[void](Invoke-TaskSql "DECLARE @r TABLE(documento nvarchar(50),nombre nvarchar(150),grupo varchar(5),rh varchar(5));
INSERT @r EXEC dbo.sp_PN2_Receptores_Listar @buscar=N'REUSE-1',@id_usuario=$taskUser;
IF NOT EXISTS(SELECT 1 FROM @r WHERE grupo='A' AND rh='+' AND nombre=N'Receptor recurrente actualizado') THROW 51000,'RECIPIENT_SCOPE_LEAK',1;
DELETE @r; INSERT @r EXEC dbo.sp_PN2_Receptores_Listar @buscar=N'REUSE-1',@id_usuario=$taskOtherUser;
IF NOT EXISTS(SELECT 1 FROM @r WHERE grupo='O' AND rh='-' AND nombre=N'Receptor privado') THROW 51000,'RECIPIENT_OTHER_SCOPE_LEAK',1;
BEGIN TRY
 EXEC dbo.sp_PN2_Receptores_Listar @id_usuario=-1;
 THROW 51000,'RECIPIENT_AUTHORIZATION_BYPASSED',1;
END TRY BEGIN CATCH
 IF ERROR_NUMBER()<>51003 THROW;
END CATCH;")
[void](Invoke-TaskSql "UPDATE dbo.UsuarioComponentePermiso SET id_componente=(SELECT id_componente FROM dbo.ComponentePermiso WHERE codigo='PN2_OPERADOR_BANCO') WHERE id_usuario=$taskUser;
DECLARE @r TABLE(documento nvarchar(50),nombre nvarchar(150),grupo varchar(5),rh varchar(5));
INSERT @r EXEC dbo.sp_PN2_Receptores_Listar @buscar=N'REUSE-1',@id_usuario=$taskUser;
IF NOT EXISTS(SELECT 1 FROM @r WHERE nombre=N'Receptor privado') THROW 51000,'RECIPIENT_OPERATOR_SCOPE',1;
UPDATE dbo.UsuarioComponentePermiso SET id_componente=(SELECT id_componente FROM dbo.ComponentePermiso WHERE codigo='PN2_PROFESIONAL_SOLICITANTE') WHERE id_usuario=$taskUser;")
if ($ReceptoresUiRunner) {
    [void](Invoke-TaskSql "INSERT dbo.Idioma(codigo,nombre) SELECT codigo,nombre FROM (VALUES('es-AR','Espanol'),('en-US','English')) i(codigo,nombre) WHERE NOT EXISTS(SELECT 1 FROM dbo.Idioma d WHERE d.codigo=i.codigo);")
    $taskUiCatalog = [IO.File]::ReadAllText((Join-Path $PSScriptRoot '../Database/StoredProcedures/08_PN2_Catalogos.sql')).Replace('USE [HemovidaGest];','')
    foreach ($taskBatch in [regex]::Split($taskUiCatalog,'(?im)^\s*GO\s*(?:--[^\r\n]*)?$')) {
        if ($taskBatch.Trim()) { [void](Invoke-TaskSql $taskBatch) }
    }
    & $ReceptoresUiRunner $taskBuilder.DataSource $taskClone $taskUser
    if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas de receptores en la UI.' }
}
if ([int](Invoke-TaskSql 'SELECT COUNT(*) FROM dbo.v_IntegridadPN1_Errores') -ne 0) { throw 'Integridad invalida despues de reutilizar receptores.' }
Write-Host 'Correcto: receptores reutilizables por documento, grupo/Rh persistidos, estudios independientes y permisos.'
