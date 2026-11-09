-- Instalar después de 11_ComprobantesAutomaticos.sql.
USE [HemovidaGest];
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;
EXEC dbo.sp_IntegridadPN1_Exigir;
EXEC dbo.sp_MaestroPN1_Exigir;
IF NOT EXISTS (SELECT 1 FROM dbo.EstadoUnidad WHERE codigo='ANULADA')
    THROW 51002,'ACTUALIZAR_MAESTROS_ANTES_DE_AJUSTE',1;

IF NOT EXISTS (SELECT 1 FROM dbo.Permiso WHERE codigo='DONACION_AJUSTAR_CANTIDAD')
    INSERT dbo.Permiso(codigo,nombre,descripcion,modulo,accion)
    VALUES('DONACION_AJUSTAR_CANTIDAD','Corregir cantidad de donación','Corrige la cantidad declarada en donaciones abiertas.','PN1','DONACION_AJUSTAR_CANTIDAD');
IF NOT EXISTS (SELECT 1 FROM dbo.ComponentePermiso WHERE codigo='DONACION_AJUSTAR_CANTIDAD')
    INSERT dbo.ComponentePermiso(codigo,nombre,descripcion,tipo)
    VALUES('DONACION_AJUSTAR_CANTIDAD','Corregir cantidad de donación','Corrige la cantidad declarada en donaciones abiertas.','PERMISO');
INSERT dbo.ComponentePermisoRelacion(id_padre,id_hijo)
SELECT p.id_componente,h.id_componente FROM dbo.ComponentePermiso p CROSS JOIN dbo.ComponentePermiso h
WHERE p.codigo='PN1_RESPONSABLE_AUTORIZADO' AND h.codigo='DONACION_AJUSTAR_CANTIDAD'
  AND NOT EXISTS (SELECT 1 FROM dbo.ComponentePermisoRelacion r WHERE r.id_padre=p.id_componente AND r.id_hijo=h.id_componente);
INSERT dbo.RolPermiso(id_rol,id_permiso)
SELECT r.id_rol,p.id_permiso FROM dbo.Rol r CROSS JOIN dbo.Permiso p
WHERE r.nombre IN ('Administrador','Responsable autorizado') AND p.codigo='DONACION_AJUSTAR_CANTIDAD'
  AND NOT EXISTS (SELECT 1 FROM dbo.RolPermiso rp WHERE rp.id_rol=r.id_rol AND rp.id_permiso=p.id_permiso);
GO

CREATE OR ALTER VIEW dbo.v_Donacion_CantidadCerrada AS
SELECT id_referencia AS id_donacion FROM dbo.ComprobantePendiente WHERE proceso='PN1'
UNION
SELECT u.id_donacion FROM dbo.Unidad u WHERE u.estado_operativo IN ('LIBERADA','RESERVADA','ENTREGADA','TRANSFUNDIDA','DEVUELTA')
    OR EXISTS (SELECT 1 FROM dbo.MovimientoUnidad m WHERE m.id_unidad=u.id_unidad AND m.tipo_movimiento='LIBERACION');
GO

CREATE OR ALTER VIEW dbo.v_Donacion_UnidadAnulable AS
SELECT u.id_unidad,u.id_donacion,u.codigo_identificacion FROM dbo.Unidad u
WHERE u.estado_operativo='EN_REVISION'
  AND (u.tipo_componente IS NULL OR u.grupo_sanguineo IS NULL OR u.factor_rh IS NULL
       OR EXISTS (SELECT 1 FROM dbo.MovimientoUnidad m WHERE m.id_unidad=u.id_unidad AND m.tipo_movimiento IN ('REGISTRO','TIPIFICACION_DONANTE')))
  AND NOT EXISTS (SELECT 1 FROM dbo.MovimientoUnidad m WHERE m.id_unidad=u.id_unidad AND m.tipo_movimiento IN ('CLASIFICACION','REINGRESO','LIBERACION'));
GO

CREATE OR ALTER VIEW dbo.v_Donacion_HistorialCantidad AS
SELECT a.id_auditoria,a.id_entidad AS id_donacion,a.accion,a.fecha_evento,a.identificador_usuario_actor,
       TRY_CONVERT(INT,JSON_VALUE(a.estado_anterior_json,'$.cantidad_unidades')) AS cantidad_anterior,
       TRY_CONVERT(INT,JSON_VALUE(a.estado_nuevo_json,'$.cantidad_unidades')) AS cantidad_nueva,
       JSON_VALUE(a.cambios_json,'$[1].ValorNuevo_380_jh') AS motivo,detalle.unidades_afectadas
FROM dbo.v_IntegridadPN1_Auditoria a
OUTER APPLY OPENJSON(a.cambios_json,'$[2]') WITH (unidades_afectadas NVARCHAR(MAX) '$.ValorNuevo_380_jh') detalle
WHERE a.entidad='Donacion' AND (a.accion IN ('AJUSTE_CANTIDAD','RESTAURA_CANTIDAD') OR a.id_auditoria=(
    SELECT MIN(inicial.id_auditoria) FROM dbo.Auditoria inicial WHERE inicial.entidad='Donacion' AND inicial.id_entidad=a.id_entidad));
GO

-- Entrada de comandos. El trigger aplica la corrección y usa Auditoria como
-- historial persistente, con los mismos dígitos verificadores que el resto de PN1.
CREATE OR ALTER VIEW dbo.v_Donacion_AjusteCantidad AS
SELECT CAST(NULL AS INT) AS id_donacion,CAST(NULL AS INT) AS version_esperada,
       CAST(NULL AS INT) AS cantidad,CAST(NULL AS NVARCHAR(500)) AS motivo,
       CAST(NULL AS NVARCHAR(MAX)) AS unidades_json,CAST(NULL AS INT) AS id_usuario,
       CAST(NULL AS INT) AS version_a_restaurar
WHERE 1=0;
GO

CREATE OR ALTER TRIGGER dbo.tr_Donacion_AjusteCantidad ON dbo.v_Donacion_AjusteCantidad
INSTEAD OF INSERT AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF (SELECT COUNT(*) FROM inserted)<>1 THROW 51120,'AJUSTE_INVALIDO',1;

    DECLARE @id INT,@version INT,@cantidad INT,@motivo NVARCHAR(500),@json NVARCHAR(MAX),@actor INT,@restaurar INT;
    SELECT @id=id_donacion,@version=version_esperada,@cantidad=cantidad,@motivo=NULLIF(LTRIM(RTRIM(motivo)),''),
           @json=unidades_json,@actor=id_usuario,@restaurar=version_a_restaurar FROM inserted;

    EXEC dbo.sp_IntegridadPN1_Exigir;
    EXEC dbo.sp_MaestroPN1_Exigir;
    IF dbo.fn_PN2_TienePermiso(@actor,'UNIDAD_VER')<>1 OR dbo.fn_PN2_TienePermiso(@actor,'DONACION_AJUSTAR_CANTIDAD')<>1
        THROW 51003,'OPERATION_NOT_AUTHORIZED',1;

    DECLARE @anterior INT,@version_actual INT,@estado_anterior NVARCHAR(MAX),@estado_nuevo NVARCHAR(MAX),@restaurada INT;
    SELECT @anterior=d.cantidad_unidades,@version_actual=v.id_auditoria,@estado_anterior=v.estado_nuevo_json
    FROM dbo.Donacion d JOIN dbo.v_IntegridadPN1_UltimaVersion v ON v.entidad='Donacion' AND v.id_entidad=d.id_donacion
    WHERE d.id_donacion=@id;
    IF @anterior IS NULL THROW 51121,'DONACION_NO_ENCONTRADA',1;
    IF @version IS NULL OR @version_actual<>@version THROW 51122,'DONACION_VERSION_CAMBIO',1;
    IF EXISTS (SELECT 1 FROM dbo.v_Donacion_CantidadCerrada WHERE id_donacion=@id) THROW 51123,'DONACION_CERRADA',1;

    IF @restaurar IS NOT NULL
    BEGIN
        SELECT @restaurada=cantidad_nueva FROM dbo.v_Donacion_HistorialCantidad WHERE id_donacion=@id AND id_auditoria=@restaurar;
        IF @restaurada IS NULL OR @restaurada<>@cantidad THROW 51120,'AJUSTE_INVALIDO',1;
    END;
    IF @cantidad IS NULL OR @cantidad NOT BETWEEN 1 AND 1000 OR @cantidad=@anterior OR @motivo IS NULL
       OR @json IS NULL OR LEFT(LTRIM(@json),1)<>'[' OR ISJSON(@json)<>1
       THROW 51120,'AJUSTE_INVALIDO',1;
    IF @anterior<>(SELECT COUNT(*) FROM dbo.Unidad WHERE id_donacion=@id AND estado_operativo<>'ANULADA')
        THROW 51120,'CANTIDAD_NO_COINCIDE_CON_UNIDADES',1;

    DECLARE @delta INT=@cantidad-@anterior;
    DECLARE @afectadas TABLE (id_unidad INT PRIMARY KEY,codigo VARCHAR(50));
    IF @delta<0
    BEGIN
        INSERT @afectadas(id_unidad,codigo)
        SELECT DISTINCT u.id_unidad,u.codigo_identificacion
        FROM OPENJSON(@json) j JOIN dbo.v_Donacion_UnidadAnulable u ON u.id_unidad=TRY_CONVERT(INT,j.value)
        WHERE j.type=2 AND u.id_donacion=@id;
        IF (SELECT COUNT(*) FROM @afectadas)<>-@delta OR (SELECT COUNT(*) FROM OPENJSON(@json))<>-@delta
            THROW 51124,'UNIDADES_NO_ANULABLES',1;
        UPDATE u SET estado_operativo='ANULADA',fecha_ultima_modificacion=GETDATE(),id_usuario_ultima_modificacion=@actor
        FROM dbo.Unidad u JOIN @afectadas a ON a.id_unidad=u.id_unidad;
    END
    ELSE
    BEGIN
        IF EXISTS (SELECT 1 FROM OPENJSON(@json)) THROW 51120,'AJUSTE_INVALIDO',1;
        ;WITH numeros AS (SELECT 1 AS n UNION ALL SELECT n+1 FROM numeros WHERE n<@delta)
        INSERT dbo.Unidad(codigo_identificacion,id_donacion,tipo_componente,fecha_vencimiento,grupo_sanguineo,factor_rh,estado_operativo,id_usuario_ultima_modificacion)
        OUTPUT inserted.id_unidad,inserted.codigo_identificacion INTO @afectadas
        SELECT 'UN-'+REPLACE(CONVERT(VARCHAR(36),NEWID()),'-',''),d.id_donacion,d.tipo_componente,d.fecha_vencimiento,
               donante.grupo_sanguineo,donante.factor_rh,'EN_REVISION',@actor
        FROM numeros CROSS JOIN dbo.Donacion d JOIN dbo.Donante donante ON donante.id_donante=d.id_donante
        WHERE d.id_donacion=@id OPTION(MAXRECURSION 1000);
    END;

    UPDATE dbo.Donacion SET cantidad_unidades=@cantidad WHERE id_donacion=@id;
    INSERT dbo.MovimientoUnidad(id_unidad,tipo_movimiento,estado_anterior,estado_nuevo,observacion,id_usuario)
    SELECT id_unidad,CASE WHEN @delta<0 THEN 'ANULACION_CANTIDAD' ELSE 'REGISTRO' END,
           CASE WHEN @delta<0 THEN 'EN_REVISION' END,CASE WHEN @delta<0 THEN 'ANULADA' ELSE 'EN_REVISION' END,
           @motivo,@actor FROM @afectadas;

    SELECT @estado_nuevo=datos_json FROM dbo.v_IntegridadPN1_Datos WHERE entidad='Donacion' AND id_entidad=@id;
    DECLARE @codigos NVARCHAR(MAX)=(SELECT STRING_AGG(CONVERT(NVARCHAR(MAX),codigo),N', ') WITHIN GROUP (ORDER BY id_unidad) FROM @afectadas);
    DECLARE @cambios NVARCHAR(MAX)=(SELECT campo AS Campo_380_jh,anterior AS ValorAnterior_380_jh,nuevo AS ValorNuevo_380_jh
        FROM (VALUES (1,N'cantidad_unidades',CONVERT(NVARCHAR(MAX),@anterior),CONVERT(NVARCHAR(MAX),@cantidad)),
                     (2,N'motivo',NULL,@motivo),(3,N'unidades_afectadas',NULL,@codigos),
                     (4,N'version_restaurada',NULL,CONVERT(NVARCHAR(MAX),@restaurar))) cambios(orden,campo,anterior,nuevo)
        WHERE orden<>4 OR @restaurar IS NOT NULL ORDER BY orden FOR JSON PATH, INCLUDE_NULL_VALUES);
    INSERT dbo.Auditoria(entidad,id_entidad,accion,id_usuario_actor,identificador_usuario_actor,estado_anterior_json,estado_nuevo_json,cambios_json)
    SELECT 'Donacion',@id,CASE WHEN @restaurar IS NULL THEN 'AJUSTE_CANTIDAD' ELSE 'RESTAURA_CANTIDAD' END,
           @actor,nombre_usuario,@estado_anterior,@estado_nuevo,@cambios FROM dbo.Usuario WHERE id_usuario=@actor;
    DECLARE @id_auditoria INT=CONVERT(INT,SCOPE_IDENTITY());
    UPDATE a SET dvh_integridad=h.dvh_calculado FROM dbo.Auditoria a JOIN dbo.v_IntegridadPN1_Auditoria h ON h.id_auditoria=a.id_auditoria
    WHERE a.id_auditoria=@id_auditoria;
    INSERT dbo.Bitacora(id_usuario,identificador_usuario,modulo,accion,nivel,descripcion,equipo)
    SELECT @actor,nombre_usuario,'BancoSangre_380_jh','CantidadDonacionAjustada_380_jh','Informacion_380_jh',
           CONCAT(N'Donación #',@id,N' · ',@anterior,N' → ',@cantidad,N' unidades.'),HOST_NAME()
    FROM dbo.Usuario WHERE id_usuario=@actor;
    EXEC dbo.sp_IntegridadPN1_Sellar @actor,'AJUSTE_CANTIDAD';
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Donacion_AjustarCantidad
    @id_donacion INT,@version_esperada INT,@cantidad INT,@motivo NVARCHAR(500),@unidades_json NVARCHAR(MAX),@id_usuario INT,@version_a_restaurar INT=NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.v_Donacion_AjusteCantidad(id_donacion,version_esperada,cantidad,motivo,unidades_json,id_usuario,version_a_restaurar)
    VALUES(@id_donacion,@version_esperada,@cantidad,@motivo,@unidades_json,@id_usuario,@version_a_restaurar);
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Donacion_ConsultarCantidad @id_donacion INT,@id_usuario INT
AS
BEGIN
    SET NOCOUNT ON;
    IF dbo.fn_PN2_TienePermiso(@id_usuario,'UNIDAD_VER')<>1 THROW 51003,'OPERATION_NOT_AUTHORIZED',1;
    -- Una sola instantánea para versión, historial y candidatas de la pantalla.
    BEGIN TRY
        BEGIN TRANSACTION;
        EXEC dbo.sp_IntegridadPN1_Bloquear;
        SELECT d.cantidad_unidades,v.id_auditoria AS version_actual,
               CONVERT(BIT,CASE WHEN c.id_donacion IS NULL AND dbo.fn_PN2_TienePermiso(@id_usuario,'DONACION_AJUSTAR_CANTIDAD')=1
                 AND NOT EXISTS (SELECT 1 FROM dbo.v_IntegridadPN1_Errores) THEN 1 ELSE 0 END) AS editable
        FROM dbo.Donacion d LEFT JOIN dbo.v_IntegridadPN1_UltimaVersion v ON v.entidad='Donacion' AND v.id_entidad=d.id_donacion
        LEFT JOIN dbo.v_Donacion_CantidadCerrada c ON c.id_donacion=d.id_donacion WHERE d.id_donacion=@id_donacion;
        SELECT id_auditoria,accion,fecha_evento,identificador_usuario_actor,cantidad_anterior,cantidad_nueva,motivo,unidades_afectadas
        FROM dbo.v_Donacion_HistorialCantidad WHERE id_donacion=@id_donacion ORDER BY id_auditoria DESC;
        SELECT id_unidad,codigo_identificacion FROM dbo.v_Donacion_UnidadAnulable WHERE id_donacion=@id_donacion ORDER BY id_unidad;
        SELECT id_unidad,codigo_identificacion,id_donacion,tipo_componente,fecha_vencimiento,grupo_sanguineo,
               factor_rh,observaciones,estado_operativo,fecha_alta,fecha_ultima_modificacion,id_usuario_ultima_modificacion
        FROM dbo.Unidad WHERE id_donacion=@id_donacion ORDER BY id_unidad;
        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK;
        THROW;
    END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Donacion_Listar @id_usuario INT
AS
BEGIN
    SET NOCOUNT ON;
    IF dbo.fn_PN2_TienePermiso(@id_usuario,'UNIDAD_VER')<>1 THROW 51003,'OPERATION_NOT_AUTHORIZED',1;
    SELECT d.id_donacion,d.fecha_donacion,d.cantidad_unidades,d.id_donante,p.nombre,p.apellido
    FROM dbo.Donacion d JOIN dbo.Donante p ON p.id_donante=d.id_donante
    ORDER BY d.fecha_alta DESC,d.id_donacion DESC;
END;
GO

COMMIT;
GO
