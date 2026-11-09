-- Migracion incremental PN1/PN2. Ejecutar antes de 07_PN1.sql con stop-on-error.
-- Conserva los JSON y hashes historicos; los campos desconocidos se omiten del JSON.
USE [HemovidaGest];
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;
EXEC dbo.sp_IntegridadPN1_Exigir;
EXEC dbo.sp_MaestroPN1_Exigir;
IF COL_LENGTH('dbo.Donante', 'grupo_sanguineo') IS NULL
    ALTER TABLE dbo.Donante ADD grupo_sanguineo VARCHAR(5) NULL;
IF COL_LENGTH('dbo.Donante', 'factor_rh') IS NULL
    ALTER TABLE dbo.Donante ADD factor_rh VARCHAR(5) NULL;
GO
DECLARE @def NVARCHAR(MAX) = OBJECT_DEFINITION(OBJECT_ID('dbo.v_IntegridadPN1_Datos'));
IF @def IS NULL OR CHARINDEX(N'''$.grupo_sanguineo'', d.grupo_sanguineo)', @def) = 0
BEGIN
    DECLARE @anterior NVARCHAR(MAX) = N'(SELECT d.id_donante, d.documento, d.nombre, d.apellido, d.fecha_nacimiento, d.telefono, d.email, d.domicilio, d.estado_donante, d.fecha_alta, d.id_usuario_alta FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)';
    IF @def IS NULL OR CHARINDEX(@anterior, @def) = 0
        THROW 51013, 'DONOR_INTEGRITY_DEFINITION_UNSUPPORTED', 1;
    SET @def = REPLACE(@def, @anterior, N'JSON_MODIFY(JSON_MODIFY(' + @anterior + N', ''$.grupo_sanguineo'', d.grupo_sanguineo), ''$.factor_rh'', d.factor_rh)');
    SET @def = STUFF(@def, 1, CHARINDEX('VIEW', UPPER(@def)) + 3, 'ALTER VIEW');
    EXEC sys.sp_executesql @def;
END;

SET @def = OBJECT_DEFINITION(OBJECT_ID('dbo.sp_IntegridadPN1_Restaurar'));
IF @def IS NULL OR @def NOT LIKE '%SET grupo_sanguineo=j.grupo_sanguineo,factor_rh=j.factor_rh,documento=j.documento%'
BEGIN
    IF @def IS NULL OR CHARINDEX('SET documento=j.documento', @def) = 0
        THROW 51013, 'DONOR_RESTORE_DEFINITION_UNSUPPORTED', 1;
    SET @def = REPLACE(@def, 'SET documento=j.documento', 'SET grupo_sanguineo=j.grupo_sanguineo,factor_rh=j.factor_rh,documento=j.documento');
    SET @def = REPLACE(@def, 'INSERT dbo.Donante(id_donante,documento', 'INSERT dbo.Donante(grupo_sanguineo,factor_rh,id_donante,documento');
    SET @def = REPLACE(@def, 'SELECT j.id_donante,j.documento', 'SELECT j.grupo_sanguineo,j.factor_rh,j.id_donante,j.documento');
    SET @def = REPLACE(@def, 'WITH(id_donante int ''$.id_donante''', 'WITH(grupo_sanguineo varchar(5) ''$.grupo_sanguineo'',factor_rh varchar(5) ''$.factor_rh'',id_donante int ''$.id_donante''');
    SET @def = STUFF(@def, 1, CHARINDEX('PROCEDURE', UPPER(@def)) + 8, 'ALTER PROCEDURE');
    EXEC sys.sp_executesql @def;
END;
GO
IF OBJECT_ID('dbo.CK_Donante_Tipificacion', 'C') IS NULL
    ALTER TABLE dbo.Donante ADD CONSTRAINT CK_Donante_Tipificacion
    CHECK ((grupo_sanguineo IS NULL AND factor_rh IS NULL) OR (grupo_sanguineo IS NOT NULL AND factor_rh IS NOT NULL));
IF OBJECT_ID('dbo.FK_Donante_Grupo', 'F') IS NULL
    ALTER TABLE dbo.Donante ADD CONSTRAINT FK_Donante_Grupo FOREIGN KEY (grupo_sanguineo) REFERENCES dbo.GrupoSanguineo(codigo);
IF OBJECT_ID('dbo.FK_Donante_Rh', 'F') IS NULL
    ALTER TABLE dbo.Donante ADD CONSTRAINT FK_Donante_Rh FOREIGN KEY (factor_rh) REFERENCES dbo.FactorRh(codigo);

-- Conservar las variantes PN1 y PN2 del CHECK al agregar el nuevo movimiento.
DECLARE @tipos_movimiento NVARCHAR(MAX) = OBJECT_DEFINITION(OBJECT_ID('dbo.CK_MovimientoUnidad_Tipo', 'C'));
IF @tipos_movimiento IS NULL THROW 51013, 'DONOR_MOVEMENT_CONSTRAINT_REQUIRED', 1;
IF CHARINDEX('TIPIFICACION_DONANTE', @tipos_movimiento) = 0
BEGIN
    ALTER TABLE dbo.MovimientoUnidad DROP CONSTRAINT CK_MovimientoUnidad_Tipo;
    DECLARE @agregar_tipo NVARCHAR(MAX) = N'ALTER TABLE dbo.MovimientoUnidad WITH CHECK ADD CONSTRAINT CK_MovimientoUnidad_Tipo CHECK ('
        + @tipos_movimiento + N' OR tipo_movimiento = ''TIPIFICACION_DONANTE'');';
    EXEC sys.sp_executesql @agregar_tipo;
END;

-- Las fichas previas solo se consideran confirmadas cuando existe una liberacion.
-- Si faltaba la ficha, inferirla de unidades ya liberadas con valores coincidentes.
;WITH antecedentes AS (
    SELECT donacion.id_donante, MIN(unidad.grupo_sanguineo) grupo, MIN(unidad.factor_rh) rh,
           CASE WHEN MIN(unidad.grupo_sanguineo) = MAX(unidad.grupo_sanguineo)
                  AND MIN(unidad.factor_rh) = MAX(unidad.factor_rh)
                  AND COUNT(*) = COUNT(unidad.grupo_sanguineo) AND COUNT(*) = COUNT(unidad.factor_rh)
                THEN 1 ELSE 0 END AS concordantes
    FROM dbo.Unidad unidad
    INNER JOIN dbo.Donacion donacion ON donacion.id_donacion = unidad.id_donacion
    WHERE unidad.estado_operativo IN ('LIBERADA','RESERVADA','ENTREGADA','TRANSFUNDIDA','DEVUELTA')
       OR EXISTS (SELECT 1 FROM dbo.MovimientoUnidad m WHERE m.id_unidad = unidad.id_unidad AND m.tipo_movimiento = 'LIBERACION')
    GROUP BY donacion.id_donante
)
UPDATE donante SET grupo_sanguineo = antecedentes.grupo, factor_rh = antecedentes.rh
FROM dbo.Donante donante
LEFT JOIN antecedentes ON antecedentes.id_donante = donante.id_donante
WHERE (antecedentes.id_donante IS NULL AND donante.grupo_sanguineo IS NOT NULL)
   OR (antecedentes.concordantes = 1 AND donante.grupo_sanguineo IS NULL);

DECLARE @fichas_actualizadas INT = @@ROWCOUNT;
DECLARE @pendientes_actualizadas TABLE (id_unidad INT PRIMARY KEY);
UPDATE u SET grupo_sanguineo = d.grupo_sanguineo, factor_rh = d.factor_rh,
             fecha_ultima_modificacion = GETDATE()
OUTPUT inserted.id_unidad INTO @pendientes_actualizadas
FROM dbo.Unidad u
JOIN dbo.Donacion dn ON dn.id_donacion = u.id_donacion
JOIN dbo.Donante d ON d.id_donante = dn.id_donante
WHERE u.estado_operativo = 'EN_REVISION' AND d.grupo_sanguineo IS NOT NULL
  AND u.grupo_sanguineo IS NULL AND u.factor_rh IS NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.MovimientoUnidad m WHERE m.id_unidad = u.id_unidad AND m.tipo_movimiento IN ('LIBERACION','REINGRESO'));

IF @fichas_actualizadas > 0 OR EXISTS (SELECT 1 FROM @pendientes_actualizadas)
BEGIN
    DECLARE @id_usuario INT = (SELECT TOP (1) id_usuario FROM dbo.Usuario WHERE nombre_usuario = 'admin');
    IF @id_usuario IS NULL THROW 51013, 'DONOR_MIGRATION_ADMIN_REQUIRED', 1;
    UPDATE u SET id_usuario_ultima_modificacion = @id_usuario
    FROM dbo.Unidad u JOIN @pendientes_actualizadas p ON p.id_unidad = u.id_unidad;
    INSERT dbo.MovimientoUnidad (id_unidad, tipo_movimiento, estado_anterior, estado_nuevo, observacion, id_usuario)
    SELECT id_unidad, 'TIPIFICACION_DONANTE', 'EN_REVISION', 'EN_REVISION',
           N'Grupo y Rh tomados de la ficha confirmada del donante. Confirmar la muestra.', @id_usuario
    FROM @pendientes_actualizadas;
    EXEC dbo.sp_IntegridadPN1_Sellar @id_usuario, 'MIGRACION_TIPIFICACION';
END;
IF EXISTS (SELECT 1 FROM dbo.v_IntegridadPN1_Errores)
    THROW 51013, 'DONOR_MIGRATION_INTEGRITY_FAILED', 1;
COMMIT;
GO

DECLARE @textos TABLE (clave VARCHAR(100), es NVARCHAR(500), en NVARCHAR(500));
INSERT @textos VALUES ('UNIT_DONOR_BLOOD_CONFLICT',
    N'El grupo sanguíneo o factor Rh no coincide con los antecedentes del donante. Revisá la tipificación antes de continuar.',
    N'The blood group or Rh factor does not match the donor history. Review the typing before continuing.');
INSERT dbo.Etiqueta(clave, descripcion)
SELECT clave, clave FROM @textos t WHERE NOT EXISTS (SELECT 1 FROM dbo.Etiqueta e WHERE e.clave = t.clave);
INSERT dbo.Traduccion(id_idioma, id_etiqueta, texto)
SELECT i.id_idioma, e.id_etiqueta, CASE WHEN i.codigo = 'en-US' THEN t.en ELSE t.es END
FROM @textos t INNER JOIN dbo.Etiqueta e ON e.clave = t.clave
CROSS JOIN dbo.Idioma i
WHERE i.codigo IN ('es-AR','en-US') AND NOT EXISTS (
    SELECT 1 FROM dbo.Traduccion tr WHERE tr.id_idioma = i.id_idioma AND tr.id_etiqueta = e.id_etiqueta);
GO
