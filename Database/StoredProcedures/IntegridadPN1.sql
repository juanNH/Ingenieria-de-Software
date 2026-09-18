-- HemovidaGest: migracion incremental de integridad PN1 (SQL Server 2016+).
-- Ejecutar antes de PN1.sql. No acepta los datos existentes automaticamente.
USE [HemovidaGest];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO
IF COL_LENGTH('dbo.Donante', 'dvh') IS NULL ALTER TABLE dbo.Donante ADD dvh VARCHAR(64) NULL;
IF COL_LENGTH('dbo.Donacion', 'dvh') IS NULL ALTER TABLE dbo.Donacion ADD dvh VARCHAR(64) NULL;
IF COL_LENGTH('dbo.Unidad', 'dvh') IS NULL ALTER TABLE dbo.Unidad ADD dvh VARCHAR(64) NULL;
IF COL_LENGTH('dbo.MovimientoUnidad', 'dvh') IS NULL ALTER TABLE dbo.MovimientoUnidad ADD dvh VARCHAR(64) NULL;
IF COL_LENGTH('dbo.Auditoria', 'dvh_integridad') IS NULL ALTER TABLE dbo.Auditoria ADD dvh_integridad VARCHAR(64) NULL;
GO
IF OBJECT_ID('dbo.IntegridadIncidente', 'U') IS NULL
CREATE TABLE dbo.IntegridadIncidente
(
    id_incidente INT IDENTITY PRIMARY KEY,
    entidad VARCHAR(100) NOT NULL,
    id_entidad INT NOT NULL,
    tipo VARCHAR(40) NOT NULL,
    fecha_deteccion DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_resolucion DATETIME NULL
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.IntegridadIncidente') AND name='UX_IntegridadIncidente_Abierto')
CREATE UNIQUE INDEX UX_IntegridadIncidente_Abierto ON dbo.IntegridadIncidente(entidad,id_entidad,tipo) WHERE fecha_resolucion IS NULL;
GO
-- Una reparacion fisica desde una version verificada tambien deja un movimiento.
ALTER TABLE dbo.MovimientoUnidad DROP CONSTRAINT CK_MovimientoUnidad_Tipo;
ALTER TABLE dbo.MovimientoUnidad ADD CONSTRAINT CK_MovimientoUnidad_Tipo
CHECK (tipo_movimiento IN ('REGISTRO','CLASIFICACION','LIBERACION','BLOQUEO','DESCARTE','RESTAURACION'));
GO
CREATE OR ALTER VIEW dbo.v_IntegridadPN1_Datos AS
SELECT CAST('Donante' AS VARCHAR(100)) entidad, d.id_donante id_entidad, j.datos_json, d.dvh,
    CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CONVERT(NVARCHAR(MAX), j.datos_json)), 2) dvh_calculado
FROM dbo.Donante d
CROSS APPLY (SELECT (SELECT d.id_donante, d.documento, d.nombre, d.apellido, d.fecha_nacimiento, d.telefono, d.email, d.domicilio, d.estado_donante, d.fecha_alta, d.id_usuario_alta FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER) datos_json) j
UNION ALL
SELECT CAST('Donacion' AS VARCHAR(100)) entidad, d.id_donacion id_entidad, j.datos_json, d.dvh,
    CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CONVERT(NVARCHAR(MAX), j.datos_json)), 2) dvh_calculado
FROM dbo.Donacion d
CROSS APPLY (SELECT (SELECT d.id_donacion, d.id_donante, d.fecha_donacion, d.cantidad_unidades, d.tipo_componente, d.fecha_vencimiento, d.observaciones, d.id_usuario_responsable, d.fecha_alta FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER) datos_json) j
UNION ALL
SELECT CAST('Unidad' AS VARCHAR(100)) entidad, d.id_unidad id_entidad, j.datos_json, d.dvh,
    CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CONVERT(NVARCHAR(MAX), j.datos_json)), 2) dvh_calculado
FROM dbo.Unidad d
CROSS APPLY (SELECT (SELECT d.id_unidad, d.codigo_identificacion, d.id_donacion, d.tipo_componente, d.fecha_vencimiento, d.grupo_sanguineo, d.factor_rh, d.observaciones, d.estado_operativo, d.fecha_alta, d.fecha_ultima_modificacion, d.id_usuario_ultima_modificacion FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER) datos_json) j
UNION ALL
SELECT CAST('MovimientoUnidad' AS VARCHAR(100)) entidad, d.id_movimiento_unidad id_entidad, j.datos_json, d.dvh,
    CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CONVERT(NVARCHAR(MAX), j.datos_json)), 2) dvh_calculado
FROM dbo.MovimientoUnidad d
CROSS APPLY (SELECT (SELECT d.id_movimiento_unidad, d.id_unidad, d.tipo_movimiento, d.estado_anterior, d.estado_nuevo, d.observacion, d.fecha_movimiento, d.id_usuario FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER) datos_json) j;
GO
CREATE OR ALTER VIEW dbo.v_IntegridadPN1_Auditoria AS
SELECT a.*, CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CONVERT(NVARCHAR(MAX), j.datos_json)), 2) dvh_calculado
FROM dbo.Auditoria a
CROSS APPLY (SELECT (SELECT a.id_auditoria,a.entidad,a.id_entidad,a.accion,a.id_usuario_actor,
 a.identificador_usuario_actor,a.fecha_evento,a.estado_anterior_json,a.estado_nuevo_json,a.cambios_json
 FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER) datos_json) j
WHERE a.entidad IN ('Donante','Donacion','Unidad','MovimientoUnidad');
GO
CREATE OR ALTER VIEW dbo.v_IntegridadPN1_UltimaVersion AS
SELECT entidad,id_entidad,estado_nuevo_json, id_auditoria
FROM (SELECT entidad,id_entidad,estado_nuevo_json,id_auditoria,
 ROW_NUMBER() OVER(PARTITION BY entidad,id_entidad ORDER BY id_auditoria DESC) rn
 FROM dbo.v_IntegridadPN1_Auditoria) a WHERE rn=1;
GO
CREATE OR ALTER VIEW dbo.v_IntegridadPN1_Vertical AS
SELECT e.entidad, CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CONVERT(NVARCHAR(MAX), COALESCE(j.contenido,N'[]'))), 2) dvv_calculado
FROM (VALUES ('Donante'),('Donacion'),('Unidad'),('MovimientoUnidad'),('AuditoriaPN1')) e(entidad)
CROSS APPLY (SELECT CASE WHEN e.entidad='AuditoriaPN1' THEN
 (SELECT a.id_auditoria, a.dvh_integridad FROM dbo.v_IntegridadPN1_Auditoria a ORDER BY a.id_auditoria FOR JSON PATH, INCLUDE_NULL_VALUES)
 ELSE (SELECT d.id_entidad, d.dvh FROM dbo.v_IntegridadPN1_Datos d WHERE d.entidad=e.entidad ORDER BY d.id_entidad FOR JSON PATH, INCLUDE_NULL_VALUES)
 END contenido) j;
GO
CREATE OR ALTER VIEW dbo.v_IntegridadPN1_Errores AS
SELECT CAST('PN1' AS VARCHAR(100)) entidad, 0 id_entidad, CAST('SIN_INICIALIZAR' AS VARCHAR(40)) tipo
WHERE (SELECT COUNT(*) FROM dbo.DigitoVerificadorVertical WHERE entidad IN ('Donante','Donacion','Unidad','MovimientoUnidad','AuditoriaPN1'))<>5
  AND (
        EXISTS (SELECT 1 FROM dbo.v_IntegridadPN1_Datos)
        OR EXISTS (SELECT 1 FROM dbo.v_IntegridadPN1_Auditoria)
        OR EXISTS (SELECT 1 FROM dbo.DigitoVerificadorVertical WHERE entidad IN ('Donante','Donacion','Unidad','MovimientoUnidad','AuditoriaPN1'))
      )
UNION ALL
SELECT entidad,id_entidad,'DVH' FROM dbo.v_IntegridadPN1_Datos WHERE dvh IS NULL OR dvh<>dvh_calculado
UNION ALL
SELECT v.entidad,0,'DVV' FROM dbo.v_IntegridadPN1_Vertical v
LEFT JOIN dbo.DigitoVerificadorVertical d ON d.entidad=v.entidad
WHERE (d.dvv IS NULL OR d.dvv<>v.dvv_calculado)
  AND (
        EXISTS (SELECT 1 FROM dbo.v_IntegridadPN1_Datos)
        OR EXISTS (SELECT 1 FROM dbo.v_IntegridadPN1_Auditoria)
        OR EXISTS (SELECT 1 FROM dbo.DigitoVerificadorVertical WHERE entidad IN ('Donante','Donacion','Unidad','MovimientoUnidad','AuditoriaPN1'))
      )
UNION ALL
SELECT 'AuditoriaPN1',id_auditoria,'HISTORIAL_INVALIDO' FROM dbo.v_IntegridadPN1_Auditoria
WHERE dvh_integridad IS NULL OR dvh_integridad<>dvh_calculado
UNION ALL
SELECT v.entidad,v.id_entidad,'FALTANTE' FROM dbo.v_IntegridadPN1_UltimaVersion v
LEFT JOIN dbo.v_IntegridadPN1_Datos d ON d.entidad=v.entidad AND d.id_entidad=v.id_entidad
WHERE d.id_entidad IS NULL
UNION ALL
SELECT d.entidad,d.id_entidad,'SIN_VERSION' FROM dbo.v_IntegridadPN1_Datos d
LEFT JOIN dbo.v_IntegridadPN1_UltimaVersion v ON d.entidad=v.entidad AND d.id_entidad=v.id_entidad
WHERE v.id_entidad IS NULL
UNION ALL
SELECT d.entidad,d.id_entidad,'VERSION_DIFERENTE' FROM dbo.v_IntegridadPN1_Datos d
JOIN dbo.v_IntegridadPN1_UltimaVersion v ON d.entidad=v.entidad AND d.id_entidad=v.id_entidad
WHERE d.dvh_calculado<>CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CONVERT(NVARCHAR(MAX), v.estado_nuevo_json)), 2);
GO
CREATE OR ALTER VIEW dbo.v_IntegridadPN1_Revision AS
SELECT CONVERT(VARCHAR(64),HASHBYTES('SHA2_256',CONVERT(NVARCHAR(MAX),
 COALESCE((SELECT entidad,id_entidad,tipo FROM dbo.v_IntegridadPN1_Errores ORDER BY entidad,id_entidad,tipo FOR JSON PATH),N'[]') +
 COALESCE((SELECT entidad,id_entidad,datos_json,dvh FROM dbo.v_IntegridadPN1_Datos ORDER BY entidad,id_entidad FOR JSON PATH,INCLUDE_NULL_VALUES),N'[]') +
 COALESCE((SELECT id_auditoria,dvh_integridad,dvh_calculado FROM dbo.v_IntegridadPN1_Auditoria ORDER BY id_auditoria FOR JSON PATH,INCLUDE_NULL_VALUES),N'[]') +
 COALESCE((SELECT entidad,dvv FROM dbo.DigitoVerificadorVertical WHERE entidad IN ('Donante','Donacion','Unidad','MovimientoUnidad','AuditoriaPN1') ORDER BY entidad FOR JSON PATH),N'[]')
)),2) revision;
GO
-- Se serializan verificacion/escritura/restauracion. Los locks de tabla evitan
-- que una modificacion SQL concurrente entre la lectura y el sellado se legitime.
CREATE OR ALTER PROCEDURE dbo.sp_IntegridadPN1_Bloquear AS
BEGIN
 SET NOCOUNT ON;
 IF @@TRANCOUNT=0 THROW 51002,'INTEGRITY_TRANSACTION_REQUIRED',1;
 DECLARE @lock INT, @n BIGINT;
 EXEC @lock=sys.sp_getapplock @Resource='HemovidaGest.PN1.Integridad',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000;
 IF @lock<0 THROW 51002,'INTEGRITY_LOCK_TIMEOUT',1;
 SELECT @n=COUNT_BIG(*) FROM dbo.Donante WITH(TABLOCKX,HOLDLOCK);
 SELECT @n=COUNT_BIG(*) FROM dbo.Donacion WITH(TABLOCKX,HOLDLOCK);
 SELECT @n=COUNT_BIG(*) FROM dbo.Unidad WITH(TABLOCKX,HOLDLOCK);
 SELECT @n=COUNT_BIG(*) FROM dbo.MovimientoUnidad WITH(TABLOCKX,HOLDLOCK);
 SELECT @n=COUNT_BIG(*) FROM dbo.Auditoria WITH(TABLOCKX,HOLDLOCK);
 SELECT @n=COUNT_BIG(*) FROM dbo.DigitoVerificadorVertical WITH(UPDLOCK,HOLDLOCK) WHERE entidad IN ('Donante','Donacion','Unidad','MovimientoUnidad','AuditoriaPN1');
END;
GO
CREATE OR ALTER PROCEDURE dbo.sp_IntegridadPN1_Exigir AS
BEGIN
 SET NOCOUNT ON;
 EXEC dbo.sp_IntegridadPN1_Bloquear;
 IF EXISTS(SELECT 1 FROM dbo.v_IntegridadPN1_Errores) THROW 51001,'INTEGRITY_BLOCKED',1;
END;
GO
-- Interno: ejecutar exclusivamente despues de Exigir o de una restauracion validada.
CREATE OR ALTER PROCEDURE dbo.sp_IntegridadPN1_Sellar
 @id_usuario INT, @accion NVARCHAR(50) = 'UPDATE'
AS
BEGIN
 SET NOCOUNT ON;
 IF @@TRANCOUNT=0 THROW 51002,'INTEGRITY_TRANSACTION_REQUIRED',1;
 DECLARE @nuevos TABLE(id INT);
 INSERT dbo.Auditoria(entidad,id_entidad,accion,id_usuario_actor,identificador_usuario_actor,
 estado_anterior_json,estado_nuevo_json,cambios_json)
 OUTPUT inserted.id_auditoria INTO @nuevos
 SELECT d.entidad,d.id_entidad,CASE WHEN @accion='BASELINE' THEN @accion WHEN v.id_auditoria IS NULL THEN 'CREATE' ELSE @accion END,
 @id_usuario,(SELECT nombre_usuario FROM dbo.Usuario WHERE id_usuario=@id_usuario),
 v.estado_nuevo_json,d.datos_json,
 (SELECT 'Registro' Campo_380_jh,v.estado_nuevo_json ValorAnterior_380_jh,d.datos_json ValorNuevo_380_jh FOR JSON PATH, INCLUDE_NULL_VALUES)
 FROM dbo.v_IntegridadPN1_Datos d
 LEFT JOIN dbo.v_IntegridadPN1_UltimaVersion v ON v.entidad=d.entidad AND v.id_entidad=d.id_entidad
 WHERE v.id_auditoria IS NULL OR d.dvh_calculado<>CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CONVERT(NVARCHAR(MAX), v.estado_nuevo_json)), 2);
 UPDATE a SET dvh_integridad=h.dvh_calculado FROM dbo.Auditoria a
 JOIN dbo.v_IntegridadPN1_Auditoria h ON h.id_auditoria=a.id_auditoria
 JOIN @nuevos n ON n.id=a.id_auditoria;
 UPDATE d SET dvh=v.dvh_calculado FROM dbo.Donante d JOIN dbo.v_IntegridadPN1_Datos v ON v.entidad='Donante' AND v.id_entidad=d.id_donante;
 UPDATE d SET dvh=v.dvh_calculado FROM dbo.Donacion d JOIN dbo.v_IntegridadPN1_Datos v ON v.entidad='Donacion' AND v.id_entidad=d.id_donacion;
 UPDATE d SET dvh=v.dvh_calculado FROM dbo.Unidad d JOIN dbo.v_IntegridadPN1_Datos v ON v.entidad='Unidad' AND v.id_entidad=d.id_unidad;
 UPDATE d SET dvh=v.dvh_calculado FROM dbo.MovimientoUnidad d JOIN dbo.v_IntegridadPN1_Datos v ON v.entidad='MovimientoUnidad' AND v.id_entidad=d.id_movimiento_unidad;
 MERGE dbo.DigitoVerificadorVertical AS d USING dbo.v_IntegridadPN1_Vertical AS v ON d.entidad=v.entidad
 WHEN MATCHED THEN UPDATE SET dvv=v.dvv_calculado,fecha_calculo=GETDATE()
 WHEN NOT MATCHED THEN INSERT(entidad,dvv) VALUES(v.entidad,v.dvv_calculado);
END;
GO
CREATE OR ALTER PROCEDURE dbo.sp_IntegridadPN1_Verificar @id_usuario INT = NULL AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 BEGIN TRY
 BEGIN TRANSACTION;
 EXEC dbo.sp_IntegridadPN1_Bloquear;
 SELECT * INTO #errores FROM dbo.v_IntegridadPN1_Errores;
 DECLARE @nuevos TABLE(entidad VARCHAR(100),id_entidad INT,tipo VARCHAR(40));
 INSERT dbo.IntegridadIncidente(entidad,id_entidad,tipo)
 OUTPUT inserted.entidad,inserted.id_entidad,inserted.tipo INTO @nuevos
 SELECT e.entidad,e.id_entidad,e.tipo FROM #errores e
 WHERE NOT EXISTS(SELECT 1 FROM dbo.IntegridadIncidente i WHERE i.entidad=e.entidad AND i.id_entidad=e.id_entidad AND i.tipo=e.tipo AND i.fecha_resolucion IS NULL);
 INSERT dbo.Bitacora(id_usuario,identificador_usuario,modulo,accion,nivel,descripcion,equipo)
 SELECT @id_usuario,(SELECT nombre_usuario FROM dbo.Usuario WHERE id_usuario=@id_usuario),
 'Integridad_380_jh','IntegridadDetectada_380_jh','Error_380_jh',
 CONCAT(entidad,' #',id_entidad,': ',tipo),HOST_NAME() FROM @nuevos;
 UPDATE i SET fecha_resolucion=GETDATE() FROM dbo.IntegridadIncidente i
 WHERE i.fecha_resolucion IS NULL AND NOT EXISTS(SELECT 1 FROM #errores e WHERE e.entidad=i.entidad AND e.id_entidad=i.id_entidad AND e.tipo=i.tipo);
 DECLARE @inicializable BIT=0;
 IF NOT EXISTS(SELECT 1 FROM dbo.DigitoVerificadorVertical WHERE entidad IN ('Donante','Donacion','Unidad','MovimientoUnidad','AuditoriaPN1'))
 AND NOT EXISTS(SELECT 1 FROM dbo.v_IntegridadPN1_Auditoria)
 AND NOT EXISTS(SELECT 1 FROM dbo.Bitacora WHERE accion='IntegridadInicializada_380_jh')
 AND NOT EXISTS(SELECT 1 FROM dbo.v_IntegridadPN1_Datos WHERE dvh IS NOT NULL)
 AND EXISTS(SELECT 1 FROM dbo.v_IntegridadPN1_Datos)
 SET @inicializable=1;

 -- El campo existente se utiliza como bloqueo operativo transversal: ante una
 -- inconsistencia de negocio solo quedan habilitados los usuarios con rol
 -- Administrador. El campo no forma parte del DVH de Usuario.
 IF EXISTS(SELECT 1 FROM #errores)
 BEGIN
     UPDATE u SET bloqueo_digitoverificador=1
     FROM dbo.Usuario u
     WHERE u.estado_usuario='ACTIVO'
       AND NOT EXISTS(
           SELECT 1
           FROM dbo.UsuarioRol ur
           JOIN dbo.Rol r ON r.id_rol=ur.id_rol
           WHERE ur.id_usuario=u.id_usuario
             AND ur.estado_usuario_rol='ACTIVO'
             AND r.estado_rol='ACTIVO'
             AND r.nombre='Administrador'
       );
 END
 ELSE
 BEGIN
     UPDATE u SET bloqueo_digitoverificador=0
     FROM dbo.Usuario u
     WHERE u.estado_usuario='ACTIVO'
       AND u.bloqueo_digitoverificador=1
       AND NOT EXISTS(
           SELECT 1
           FROM dbo.UsuarioRol ur
           JOIN dbo.Rol r ON r.id_rol=ur.id_rol
           WHERE ur.id_usuario=u.id_usuario
             AND ur.estado_usuario_rol='ACTIVO'
             AND r.estado_rol='ACTIVO'
             AND r.nombre='Administrador'
       );
 END;
 DECLARE @revision VARCHAR(64);
 SELECT @revision=revision FROM dbo.v_IntegridadPN1_Revision;
 SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM #errores) THEN 0 ELSE 1 END AS BIT) es_valida,
 @inicializable puede_inicializar,@revision revision;
 SELECT e.entidad,e.id_entidad,e.tipo,i.fecha_deteccion,
 d.datos_json estado_actual,v.estado_nuevo_json estado_confiable
 FROM #errores e JOIN dbo.IntegridadIncidente i ON i.entidad=e.entidad AND i.id_entidad=e.id_entidad AND i.tipo=e.tipo AND i.fecha_resolucion IS NULL
 LEFT JOIN dbo.v_IntegridadPN1_Datos d ON d.entidad=e.entidad AND d.id_entidad=e.id_entidad
 LEFT JOIN dbo.v_IntegridadPN1_UltimaVersion v ON v.entidad=e.entidad AND v.id_entidad=e.id_entidad
 ORDER BY e.entidad,e.id_entidad,e.tipo;
 COMMIT;
END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_IntegridadPN1_Autorizar @id_usuario INT, @permiso VARCHAR(100) AS
BEGIN
 SET NOCOUNT ON;
 DECLARE @permitido BIT=0;
 ;WITH asignaciones AS(
 SELECT id_componente FROM dbo.UsuarioComponentePermiso WHERE id_usuario=@id_usuario AND estado_usuario_componente='ACTIVO'
 UNION
 SELECT c.id_componente FROM dbo.UsuarioRol ur JOIN dbo.Rol r ON r.id_rol=ur.id_rol
 JOIN dbo.ComponentePermiso c ON c.nombre=r.nombre AND c.tipo='FAMILIA'
 WHERE ur.id_usuario=@id_usuario AND ur.estado_usuario_rol='ACTIVO' AND r.estado_rol='ACTIVO'
 UNION
 SELECT c.id_componente FROM dbo.UsuarioRol ur JOIN dbo.Rol r ON r.id_rol=ur.id_rol
 JOIN dbo.RolPermiso rp ON rp.id_rol=r.id_rol JOIN dbo.Permiso p ON p.id_permiso=rp.id_permiso
 JOIN dbo.ComponentePermiso c ON c.codigo=p.codigo
 WHERE ur.id_usuario=@id_usuario AND ur.estado_usuario_rol='ACTIVO' AND r.estado_rol='ACTIVO'
 ), arbol AS(
 SELECT c.id_componente,c.codigo FROM asignaciones a JOIN dbo.ComponentePermiso c ON c.id_componente=a.id_componente
 JOIN dbo.Usuario u ON u.id_usuario=@id_usuario
 WHERE u.estado_usuario='ACTIVO' AND u.bloqueo_digitoverificador=0 AND c.estado_componente='ACTIVO'
 UNION ALL
 SELECT c.id_componente,c.codigo FROM arbol a JOIN dbo.ComponentePermisoRelacion r ON r.id_padre=a.id_componente
 JOIN dbo.ComponentePermiso c ON c.id_componente=r.id_hijo WHERE c.estado_componente='ACTIVO')
 SELECT @permitido=1 FROM arbol WHERE codigo=@permiso OPTION(MAXRECURSION 100);
 IF @permitido=0 THROW 51003,'OPERATION_NOT_AUTHORIZED',1;
END;
GO
CREATE OR ALTER PROCEDURE dbo.sp_IntegridadPN1_Inicializar @id_usuario INT,@revision VARCHAR(64) AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 BEGIN TRY BEGIN TRANSACTION;
 EXEC dbo.sp_IntegridadPN1_Bloquear;
 EXEC dbo.sp_IntegridadPN1_Autorizar @id_usuario,'INTEGRIDAD_RESTAURAR';
 EXEC dbo.sp_IntegridadPN1_Autorizar @id_usuario,'INTEGRIDAD_VER';
 IF EXISTS(SELECT 1 FROM dbo.DigitoVerificadorVertical WHERE entidad IN ('Donante','Donacion','Unidad','MovimientoUnidad','AuditoriaPN1'))
 OR EXISTS(SELECT 1 FROM dbo.v_IntegridadPN1_Auditoria)
 OR EXISTS(SELECT 1 FROM dbo.Bitacora WHERE accion='IntegridadInicializada_380_jh')
 OR EXISTS(SELECT 1 FROM dbo.v_IntegridadPN1_Datos WHERE dvh IS NOT NULL)
 THROW 51004,'INTEGRITY_ALREADY_INITIALIZED',1;
 IF @revision IS NULL OR @revision<>(SELECT revision FROM dbo.v_IntegridadPN1_Revision) THROW 51005,'INTEGRITY_STALE_PREVIEW',1;
 EXEC dbo.sp_IntegridadPN1_Sellar @id_usuario,'BASELINE';
 INSERT dbo.Bitacora(id_usuario,identificador_usuario,modulo,accion,nivel,descripcion,equipo)
 SELECT @id_usuario,nombre_usuario,'Integridad_380_jh','IntegridadInicializada_380_jh','Informacion_380_jh',N'Base inicial de integridad aceptada.',HOST_NAME()
 FROM dbo.Usuario WHERE id_usuario=@id_usuario;
 IF EXISTS(SELECT 1 FROM dbo.v_IntegridadPN1_Errores) THROW 51001,'INTEGRITY_BLOCKED',1;
 COMMIT;
 SELECT 'OK' codigo_resultado;
 END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH;
END;
GO

-- Restauracion conjunta: ultima version verificada, conserva IDs y dependencias.
-- Nunca permite sobrescribir con JSON enviado por el cliente ni aceptar datos alterados.
CREATE OR ALTER PROCEDURE dbo.sp_IntegridadPN1_Restaurar
 @id_usuario INT,@revision VARCHAR(64)
WITH EXECUTE AS OWNER
AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 BEGIN TRY BEGIN TRANSACTION;
 EXEC dbo.sp_IntegridadPN1_Bloquear;
 EXEC dbo.sp_IntegridadPN1_Autorizar @id_usuario,'INTEGRIDAD_RESTAURAR';
 EXEC dbo.sp_IntegridadPN1_Autorizar @id_usuario,'INTEGRIDAD_VER';
 SELECT * INTO #errores FROM dbo.v_IntegridadPN1_Errores;
 DECLARE @revision_actual VARCHAR(64);
 SELECT @revision_actual=revision FROM dbo.v_IntegridadPN1_Revision;
 IF @revision IS NULL OR @revision<>@revision_actual THROW 51005,'INTEGRITY_STALE_PREVIEW',1;
 IF NOT EXISTS(SELECT 1 FROM #errores) THROW 51004,'INTEGRITY_NOTHING_TO_RESTORE',1;
 IF EXISTS(SELECT 1 FROM #errores WHERE tipo IN ('SIN_INICIALIZAR','SIN_VERSION','HISTORIAL_INVALIDO') OR entidad='AuditoriaPN1')
 THROW 51006,'INTEGRITY_BACKUP_REQUIRED',1;

 SELECT v.entidad,v.id_entidad,v.estado_nuevo_json confiable,d.datos_json actual
 INTO #restaurar FROM dbo.v_IntegridadPN1_UltimaVersion v
 LEFT JOIN dbo.v_IntegridadPN1_Datos d ON d.entidad=v.entidad AND d.id_entidad=v.id_entidad
 WHERE d.id_entidad IS NULL OR d.dvh IS NULL OR d.dvh<>d.dvh_calculado OR d.dvh_calculado<>CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CONVERT(NVARCHAR(MAX), v.estado_nuevo_json)), 2);

 -- Reparar filas en orden de FK. MovimientoUnidad conserva el contenido original;
 -- se agrega luego un movimiento de restauracion, sin borrar el rastro del incidente.

 UPDATE d SET documento=j.documento,nombre=j.nombre,apellido=j.apellido,fecha_nacimiento=j.fecha_nacimiento,telefono=j.telefono,email=j.email,domicilio=j.domicilio,estado_donante=j.estado_donante,fecha_alta=j.fecha_alta,id_usuario_alta=j.id_usuario_alta
 FROM dbo.Donante d JOIN #restaurar r ON r.entidad='Donante' AND r.id_entidad=d.id_donante
 CROSS APPLY OPENJSON(r.confiable) WITH(id_donante int '$.id_donante',documento varchar(30) '$.documento',nombre varchar(100) '$.nombre',apellido varchar(100) '$.apellido',fecha_nacimiento date '$.fecha_nacimiento',telefono varchar(50) '$.telefono',email varchar(150) '$.email',domicilio varchar(255) '$.domicilio',estado_donante varchar(20) '$.estado_donante',fecha_alta datetime '$.fecha_alta',id_usuario_alta int '$.id_usuario_alta') j;
 SET IDENTITY_INSERT dbo.Donante ON;
 INSERT dbo.Donante(id_donante,documento,nombre,apellido,fecha_nacimiento,telefono,email,domicilio,estado_donante,fecha_alta,id_usuario_alta)
 SELECT j.id_donante,j.documento,j.nombre,j.apellido,j.fecha_nacimiento,j.telefono,j.email,j.domicilio,j.estado_donante,j.fecha_alta,j.id_usuario_alta FROM #restaurar r
 CROSS APPLY OPENJSON(r.confiable) WITH(id_donante int '$.id_donante',documento varchar(30) '$.documento',nombre varchar(100) '$.nombre',apellido varchar(100) '$.apellido',fecha_nacimiento date '$.fecha_nacimiento',telefono varchar(50) '$.telefono',email varchar(150) '$.email',domicilio varchar(255) '$.domicilio',estado_donante varchar(20) '$.estado_donante',fecha_alta datetime '$.fecha_alta',id_usuario_alta int '$.id_usuario_alta') j
 WHERE r.entidad='Donante' AND NOT EXISTS(SELECT 1 FROM dbo.Donante d WHERE d.id_donante=r.id_entidad);
 SET IDENTITY_INSERT dbo.Donante OFF;

 UPDATE d SET id_donante=j.id_donante,fecha_donacion=j.fecha_donacion,cantidad_unidades=j.cantidad_unidades,tipo_componente=j.tipo_componente,fecha_vencimiento=j.fecha_vencimiento,observaciones=j.observaciones,id_usuario_responsable=j.id_usuario_responsable,fecha_alta=j.fecha_alta
 FROM dbo.Donacion d JOIN #restaurar r ON r.entidad='Donacion' AND r.id_entidad=d.id_donacion
 CROSS APPLY OPENJSON(r.confiable) WITH(id_donacion int '$.id_donacion',id_donante int '$.id_donante',fecha_donacion datetime '$.fecha_donacion',cantidad_unidades int '$.cantidad_unidades',tipo_componente varchar(100) '$.tipo_componente',fecha_vencimiento date '$.fecha_vencimiento',observaciones varchar(500) '$.observaciones',id_usuario_responsable int '$.id_usuario_responsable',fecha_alta datetime '$.fecha_alta') j;
 SET IDENTITY_INSERT dbo.Donacion ON;
 INSERT dbo.Donacion(id_donacion,id_donante,fecha_donacion,cantidad_unidades,tipo_componente,fecha_vencimiento,observaciones,id_usuario_responsable,fecha_alta)
 SELECT j.id_donacion,j.id_donante,j.fecha_donacion,j.cantidad_unidades,j.tipo_componente,j.fecha_vencimiento,j.observaciones,j.id_usuario_responsable,j.fecha_alta FROM #restaurar r
 CROSS APPLY OPENJSON(r.confiable) WITH(id_donacion int '$.id_donacion',id_donante int '$.id_donante',fecha_donacion datetime '$.fecha_donacion',cantidad_unidades int '$.cantidad_unidades',tipo_componente varchar(100) '$.tipo_componente',fecha_vencimiento date '$.fecha_vencimiento',observaciones varchar(500) '$.observaciones',id_usuario_responsable int '$.id_usuario_responsable',fecha_alta datetime '$.fecha_alta') j
 WHERE r.entidad='Donacion' AND NOT EXISTS(SELECT 1 FROM dbo.Donacion d WHERE d.id_donacion=r.id_entidad);
 SET IDENTITY_INSERT dbo.Donacion OFF;

 UPDATE d SET codigo_identificacion=j.codigo_identificacion,id_donacion=j.id_donacion,tipo_componente=j.tipo_componente,fecha_vencimiento=j.fecha_vencimiento,grupo_sanguineo=j.grupo_sanguineo,factor_rh=j.factor_rh,observaciones=j.observaciones,estado_operativo=j.estado_operativo,fecha_alta=j.fecha_alta,fecha_ultima_modificacion=j.fecha_ultima_modificacion,id_usuario_ultima_modificacion=j.id_usuario_ultima_modificacion
 FROM dbo.Unidad d JOIN #restaurar r ON r.entidad='Unidad' AND r.id_entidad=d.id_unidad
 CROSS APPLY OPENJSON(r.confiable) WITH(id_unidad int '$.id_unidad',codigo_identificacion varchar(50) '$.codigo_identificacion',id_donacion int '$.id_donacion',tipo_componente varchar(100) '$.tipo_componente',fecha_vencimiento date '$.fecha_vencimiento',grupo_sanguineo varchar(5) '$.grupo_sanguineo',factor_rh varchar(5) '$.factor_rh',observaciones varchar(500) '$.observaciones',estado_operativo varchar(20) '$.estado_operativo',fecha_alta datetime '$.fecha_alta',fecha_ultima_modificacion datetime '$.fecha_ultima_modificacion',id_usuario_ultima_modificacion int '$.id_usuario_ultima_modificacion') j;
 SET IDENTITY_INSERT dbo.Unidad ON;
 INSERT dbo.Unidad(id_unidad,codigo_identificacion,id_donacion,tipo_componente,fecha_vencimiento,grupo_sanguineo,factor_rh,observaciones,estado_operativo,fecha_alta,fecha_ultima_modificacion,id_usuario_ultima_modificacion)
 SELECT j.id_unidad,j.codigo_identificacion,j.id_donacion,j.tipo_componente,j.fecha_vencimiento,j.grupo_sanguineo,j.factor_rh,j.observaciones,j.estado_operativo,j.fecha_alta,j.fecha_ultima_modificacion,j.id_usuario_ultima_modificacion FROM #restaurar r
 CROSS APPLY OPENJSON(r.confiable) WITH(id_unidad int '$.id_unidad',codigo_identificacion varchar(50) '$.codigo_identificacion',id_donacion int '$.id_donacion',tipo_componente varchar(100) '$.tipo_componente',fecha_vencimiento date '$.fecha_vencimiento',grupo_sanguineo varchar(5) '$.grupo_sanguineo',factor_rh varchar(5) '$.factor_rh',observaciones varchar(500) '$.observaciones',estado_operativo varchar(20) '$.estado_operativo',fecha_alta datetime '$.fecha_alta',fecha_ultima_modificacion datetime '$.fecha_ultima_modificacion',id_usuario_ultima_modificacion int '$.id_usuario_ultima_modificacion') j
 WHERE r.entidad='Unidad' AND NOT EXISTS(SELECT 1 FROM dbo.Unidad d WHERE d.id_unidad=r.id_entidad);
 SET IDENTITY_INSERT dbo.Unidad OFF;

 UPDATE d SET id_unidad=j.id_unidad,tipo_movimiento=j.tipo_movimiento,estado_anterior=j.estado_anterior,estado_nuevo=j.estado_nuevo,observacion=j.observacion,fecha_movimiento=j.fecha_movimiento,id_usuario=j.id_usuario
 FROM dbo.MovimientoUnidad d JOIN #restaurar r ON r.entidad='MovimientoUnidad' AND r.id_entidad=d.id_movimiento_unidad
 CROSS APPLY OPENJSON(r.confiable) WITH(id_movimiento_unidad int '$.id_movimiento_unidad',id_unidad int '$.id_unidad',tipo_movimiento varchar(30) '$.tipo_movimiento',estado_anterior varchar(20) '$.estado_anterior',estado_nuevo varchar(20) '$.estado_nuevo',observacion varchar(500) '$.observacion',fecha_movimiento datetime '$.fecha_movimiento',id_usuario int '$.id_usuario') j;
 SET IDENTITY_INSERT dbo.MovimientoUnidad ON;
 INSERT dbo.MovimientoUnidad(id_movimiento_unidad,id_unidad,tipo_movimiento,estado_anterior,estado_nuevo,observacion,fecha_movimiento,id_usuario)
 SELECT j.id_movimiento_unidad,j.id_unidad,j.tipo_movimiento,j.estado_anterior,j.estado_nuevo,j.observacion,j.fecha_movimiento,j.id_usuario FROM #restaurar r
 CROSS APPLY OPENJSON(r.confiable) WITH(id_movimiento_unidad int '$.id_movimiento_unidad',id_unidad int '$.id_unidad',tipo_movimiento varchar(30) '$.tipo_movimiento',estado_anterior varchar(20) '$.estado_anterior',estado_nuevo varchar(20) '$.estado_nuevo',observacion varchar(500) '$.observacion',fecha_movimiento datetime '$.fecha_movimiento',id_usuario int '$.id_usuario') j
 WHERE r.entidad='MovimientoUnidad' AND NOT EXISTS(SELECT 1 FROM dbo.MovimientoUnidad d WHERE d.id_movimiento_unidad=r.id_entidad);
 SET IDENTITY_INSERT dbo.MovimientoUnidad OFF;

 DECLARE @audit TABLE(id INT);
 INSERT dbo.Auditoria(entidad,id_entidad,accion,id_usuario_actor,identificador_usuario_actor,
 estado_anterior_json,estado_nuevo_json,cambios_json)
 OUTPUT inserted.id_auditoria INTO @audit
 SELECT r.entidad,r.id_entidad,'RESTORE',@id_usuario,
 (SELECT nombre_usuario FROM dbo.Usuario WHERE id_usuario=@id_usuario),r.actual,r.confiable,
 (SELECT 'Registro' Campo_380_jh,r.actual ValorAnterior_380_jh,r.confiable ValorNuevo_380_jh FOR JSON PATH,INCLUDE_NULL_VALUES)
 FROM #restaurar r;
 UPDATE a SET dvh_integridad=h.dvh_calculado FROM dbo.Auditoria a
 JOIN dbo.v_IntegridadPN1_Auditoria h ON h.id_auditoria=a.id_auditoria JOIN @audit n ON n.id=a.id_auditoria;

 INSERT dbo.MovimientoUnidad(id_unidad,tipo_movimiento,estado_anterior,estado_nuevo,observacion,id_usuario)
 SELECT u.id_unidad,'RESTAURACION',u.estado_operativo,u.estado_operativo,N'Restauración automática de integridad.',@id_usuario
 FROM dbo.Unidad u WHERE EXISTS(SELECT 1 FROM #restaurar r
 WHERE (r.entidad='Unidad' AND r.id_entidad=u.id_unidad)
 OR (r.entidad='MovimientoUnidad' AND TRY_CONVERT(INT,JSON_VALUE(r.confiable,'$.id_unidad'))=u.id_unidad));
 EXEC dbo.sp_IntegridadPN1_Sellar @id_usuario,'RESTORE';
 IF EXISTS(SELECT 1 FROM dbo.v_IntegridadPN1_Errores) THROW 51001,'INTEGRITY_BLOCKED',1;
 UPDATE dbo.IntegridadIncidente SET fecha_resolucion=GETDATE() WHERE fecha_resolucion IS NULL;
 INSERT dbo.Bitacora(id_usuario,identificador_usuario,modulo,accion,nivel,descripcion,equipo)
 SELECT @id_usuario,nombre_usuario,'Integridad_380_jh','IntegridadRestaurada_380_jh','Informacion_380_jh',N'Integridad restaurada desde historial verificado.',HOST_NAME()
 FROM dbo.Usuario WHERE id_usuario=@id_usuario;
 COMMIT;
 SELECT 'OK' codigo_resultado;
 END TRY
 BEGIN CATCH
 IF @@TRANCOUNT>0 ROLLBACK;
 SET IDENTITY_INSERT dbo.Donante OFF;
 SET IDENTITY_INSERT dbo.Donacion OFF;
 SET IDENTITY_INSERT dbo.Unidad OFF;
 SET IDENTITY_INSERT dbo.MovimientoUnidad OFF;
 THROW;
 END CATCH;
END;
GO
-- Los helpers de sellado no son una API administrativa de recalculo.
-- Ownership chaining permite su uso desde los procedimientos PN1.
DENY EXECUTE ON dbo.sp_IntegridadPN1_Sellar TO public;
GO

-- Permisos nuevos: solo el administrador los recibe de forma predeterminada.
DECLARE @permisos_integridad TABLE(codigo VARCHAR(100), nombre VARCHAR(100), accion VARCHAR(100));
INSERT @permisos_integridad VALUES
('INTEGRIDAD_VER','Consultar integridad','VER'),
('INTEGRIDAD_RESTAURAR','Restaurar integridad','RESTAURAR');
INSERT dbo.Permiso(codigo,nombre,descripcion,modulo,accion)
SELECT p.codigo,p.nombre,p.nombre,'INTEGRIDAD',p.accion FROM @permisos_integridad p
WHERE NOT EXISTS(SELECT 1 FROM dbo.Permiso d WHERE d.codigo=p.codigo);
UPDATE d SET nombre=p.nombre,descripcion=p.nombre,modulo='INTEGRIDAD',accion=p.accion
FROM dbo.Permiso d JOIN @permisos_integridad p ON p.codigo=d.codigo;
INSERT dbo.ComponentePermiso(codigo,nombre,descripcion,tipo)
SELECT p.codigo,p.nombre,p.nombre,'PERMISO' FROM @permisos_integridad p
WHERE NOT EXISTS(SELECT 1 FROM dbo.ComponentePermiso d WHERE d.codigo=p.codigo);
UPDATE d SET nombre=p.nombre,descripcion=p.nombre,tipo='PERMISO',estado_componente='ACTIVO'
FROM dbo.ComponentePermiso d JOIN @permisos_integridad p ON p.codigo=d.codigo;
INSERT dbo.ComponentePermisoRelacion(id_padre,id_hijo)
SELECT a.id_componente,c.id_componente FROM dbo.ComponentePermiso a
CROSS JOIN dbo.ComponentePermiso c WHERE a.codigo='ADMINISTRADOR'
AND c.codigo IN ('INTEGRIDAD_VER','INTEGRIDAD_RESTAURAR')
AND NOT EXISTS(SELECT 1 FROM dbo.ComponentePermisoRelacion r WHERE r.id_padre=a.id_componente AND r.id_hijo=c.id_componente);
INSERT dbo.RolPermiso(id_rol,id_permiso)
SELECT r.id_rol,p.id_permiso FROM dbo.Rol r CROSS JOIN dbo.Permiso p
WHERE r.nombre='Administrador' AND p.codigo IN ('INTEGRIDAD_VER','INTEGRIDAD_RESTAURAR')
AND NOT EXISTS(SELECT 1 FROM dbo.RolPermiso rp WHERE rp.id_rol=r.id_rol AND rp.id_permiso=p.id_permiso);
GO
-- Catalogo incremental de integridad: no elimina idiomas ni traducciones personalizadas.
DECLARE @i18n_integridad TABLE(clave VARCHAR(150), es NVARCHAR(500), en NVARCHAR(500));
INSERT @i18n_integridad VALUES
('MENU_INTEGRITY',N'Integridad',N'Integrity'),
('BITACORA_MODULE_INTEGRITY',N'Integridad',N'Integrity'),
('INTEGRITY_OK',N'Integridad: última verificación correcta.',N'Integrity: last verification passed.'),
('INTEGRITY_BLOCKED',N'Modo solo consulta: integridad comprometida. Los datos afectados no son confiables; contactá al administrador.',N'Read-only mode: integrity compromised. Affected data is untrusted; contact the administrator.'),
('INTEGRITY_UNAVAILABLE',N'Operaciones protegidas bloqueadas: no se pudo verificar la integridad. Revisá la conexión y las migraciones SQL.',N'Protected operations are blocked: integrity could not be verified. Check the connection and SQL migrations.'),
('INTEGRITY_PENDING',N'Pendiente: revisar los datos y aceptar la base inicial.',N'Pending: review the data and accept the initial baseline.'),
('INTEGRITY_DESCRIPTION',N'Se verifican filas (DVH), tablas (DVV) e historiales de los procesos protegidos. Restaurar recupera las últimas versiones verificadas; no deshace operaciones legítimas.',N'Checks rows (DVH), tables (DVV), and histories for protected processes. Restore recovers the latest verified versions; it does not undo legitimate operations.'),
('INTEGRITY_VERIFY',N'Verificar / actualizar',N'Verify / refresh'),
('INTEGRITY_INITIALIZE',N'Aceptar base inicial',N'Accept initial baseline'),
('INTEGRITY_RESTORE',N'Restaurar desde historial',N'Restore from history'),
('INTEGRITY_ENTITY',N'Entidad',N'Entity'),
('INTEGRITY_ID',N'ID',N'ID'),
('INTEGRITY_TYPE',N'Incidencia',N'Issue'),
('INTEGRITY_DATE',N'Detectada',N'Detected'),
('INTEGRITY_CURRENT',N'Registro actual (JSON; vacío si falta)',N'Current record (JSON; empty if missing)'),
('INTEGRITY_TRUSTED',N'Última versión del historial (usar solo si es íntegro)',N'Latest history version (use only when intact)'),
('INTEGRITY_CONFIRM_BASELINE',N'¿Aceptás los datos actuales como base inicial? Confirmá solo después de revisarlos y respaldarlos. Esta acción no corrige datos previos y se realiza una sola vez.',N'Accept current data as the initial baseline? Confirm only after reviewing and backing it up. This does not correct existing data and can only be done once.'),
('INTEGRITY_CONFIRM_RESTORE',N'¿Restaurar todas las inconsistencias recuperables desde las últimas versiones verificadas? Se conservarán los identificadores y se auditará la operación. No se aceptarán datos alterados como válidos.',N'Restore all recoverable inconsistencies from the latest verified versions? IDs will be preserved and the operation audited. Altered data will not be accepted as valid.'),
('INTEGRITY_RECOVERED',N'Operación completada e integridad verificada.',N'Operation completed and integrity verified.'),
('INTEGRITY_RECOVERY_FAILED',N'No se confirmó la recuperación. Verificá nuevamente el estado y revisá la bitácora antes de reintentar.',N'Recovery was not confirmed. Verify the state again and review the log before retrying.'),
('INTEGRITY_STALE_PREVIEW',N'Los datos cambiaron desde la revisión. Actualizá el informe antes de confirmar.',N'Data changed since the preview. Refresh the report before confirming.'),
('INTEGRITY_BACKUP_REQUIRED',N'La fuente no permite una recuperación segura. Necesitás un backup confiable; no recalcules para ocultar el error.',N'The source cannot support safe recovery. A trusted backup is required; do not recalculate to hide the error.'),
('INTEGRITY_TYPE_SIN_INICIALIZAR',N'Base inicial ausente o incompleta',N'Missing or incomplete baseline'),
('INTEGRITY_TYPE_DVH',N'Dígito de fila incorrecto',N'Row checksum mismatch'),
('INTEGRITY_TYPE_DVV',N'Dígito de tabla incorrecto',N'Table checksum mismatch'),
('INTEGRITY_TYPE_HISTORIAL_INVALIDO',N'Historial alterado',N'Altered history'),
('INTEGRITY_TYPE_FALTANTE',N'Registro eliminado',N'Deleted record'),
('INTEGRITY_TYPE_SIN_VERSION',N'Registro sin versión confiable',N'Record without a trusted version'),
('INTEGRITY_TYPE_VERSION_DIFERENTE',N'Datos distintos del historial',N'Data differs from history'),
('BITACORA_ACTION_INTEGRITY_DETECTED',N'Error de integridad detectado',N'Integrity error detected'),
('BITACORA_ACTION_INTEGRITY_INITIALIZED',N'Base inicial de integridad aceptada',N'Initial integrity baseline accepted'),
('BITACORA_ACTION_INTEGRITY_RESTORED',N'Integridad restaurada',N'Integrity restored'),
('BITACORA_ACTION_RESTORE_FAILED',N'Recuperación fallida',N'Recovery failed');
INSERT dbo.Etiqueta(clave,descripcion)
SELECT c.clave,c.clave FROM @i18n_integridad c
WHERE NOT EXISTS(SELECT 1 FROM dbo.Etiqueta e WHERE e.clave=c.clave);
INSERT dbo.Traduccion(id_etiqueta,id_idioma,texto)
SELECT e.id_etiqueta,i.id_idioma,CASE WHEN i.codigo='en-US' THEN c.en ELSE c.es END
FROM @i18n_integridad c JOIN dbo.Etiqueta e ON e.clave=c.clave
CROSS JOIN dbo.Idioma i WHERE i.codigo IN ('es-AR','en-US')
AND NOT EXISTS(SELECT 1 FROM dbo.Traduccion t WHERE t.id_etiqueta=e.id_etiqueta AND t.id_idioma=i.id_idioma);
UPDATE t SET texto=CASE WHEN i.codigo='en-US' THEN c.en ELSE c.es END
FROM dbo.Traduccion t JOIN dbo.Etiqueta e ON e.id_etiqueta=t.id_etiqueta
JOIN @i18n_integridad c ON c.clave=e.clave JOIN dbo.Idioma i ON i.id_idioma=t.id_idioma
WHERE i.codigo IN ('es-AR','en-US');
-- Limpieza de las etiquetas retiradas junto con el campo de motivo.
DELETE t FROM dbo.Traduccion t JOIN dbo.Etiqueta e ON e.id_etiqueta=t.id_etiqueta
WHERE e.clave IN ('INTEGRITY_REASON','INTEGRITY_REASON_REQUIRED');
DELETE FROM dbo.Etiqueta WHERE clave IN ('INTEGRITY_REASON','INTEGRITY_REASON_REQUIRED');
GO
