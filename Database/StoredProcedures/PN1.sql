USE [HemovidaGest];
GO

IF OBJECT_ID('dbo.sp_Donante_Listar', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Donante_Listar;
GO

CREATE PROCEDURE dbo.sp_Donante_Listar
    @buscar VARCHAR(150) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        d.id_donante,
        d.documento,
        d.nombre,
        d.apellido,
        d.fecha_nacimiento,
        d.telefono,
        d.email,
        d.domicilio,
        d.estado_donante,
        d.fecha_alta,
        d.id_usuario_alta
    FROM dbo.Donante d
    WHERE d.estado_donante = 'ACTIVO'
      AND (
            @buscar IS NULL
            OR d.documento LIKE '%' + @buscar + '%'
            OR d.nombre LIKE '%' + @buscar + '%'
            OR d.apellido LIKE '%' + @buscar + '%'
          )
    ORDER BY d.apellido, d.nombre, d.id_donante;
END
GO

IF OBJECT_ID('dbo.sp_Donante_Registrar', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Donante_Registrar;
GO

CREATE PROCEDURE dbo.sp_Donante_Registrar
    @documento VARCHAR(30),
    @nombre VARCHAR(100),
    @apellido VARCHAR(100),
    @fecha_nacimiento DATE = NULL,
    @telefono VARCHAR(50) = NULL,
    @email VARCHAR(150) = NULL,
    @domicilio VARCHAR(255) = NULL,
    @id_usuario INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;
        EXEC dbo.sp_IntegridadPN1_Exigir;

    IF NULLIF(LTRIM(RTRIM(@documento)), '') IS NULL
       OR NULLIF(LTRIM(RTRIM(@nombre)), '') IS NULL
       OR NULLIF(LTRIM(RTRIM(@apellido)), '') IS NULL
       OR @id_usuario IS NULL
       OR @id_usuario <= 0
       OR (@fecha_nacimiento IS NOT NULL AND @fecha_nacimiento > CONVERT(DATE, GETDATE()))
    BEGIN
        SELECT 'DATOS_INVALIDOS' AS codigo_resultado, CAST(NULL AS INT) AS id_donante;
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    IF EXISTS (
        SELECT 1 FROM dbo.Donante
        WHERE documento = LTRIM(RTRIM(@documento))
    )
    BEGIN
        SELECT 'DUPLICADO' AS codigo_resultado, id_donante
        FROM dbo.Donante
        WHERE documento = LTRIM(RTRIM(@documento));
        ROLLBACK TRANSACTION;
        RETURN;
    END;


        INSERT INTO dbo.Donante
        (
            documento, nombre, apellido, fecha_nacimiento, telefono,
            email, domicilio, estado_donante, id_usuario_alta
        )
        VALUES
        (
            LTRIM(RTRIM(@documento)), LTRIM(RTRIM(@nombre)), LTRIM(RTRIM(@apellido)),
            @fecha_nacimiento, NULLIF(LTRIM(RTRIM(@telefono)), ''),
            NULLIF(LTRIM(RTRIM(@email)), ''), NULLIF(LTRIM(RTRIM(@domicilio)), ''),
            'ACTIVO', @id_usuario
        );

        DECLARE @id_donante INT = CONVERT(INT, SCOPE_IDENTITY());
        EXEC dbo.sp_IntegridadPN1_Sellar @id_usuario;
        COMMIT TRANSACTION;

        SELECT 'OK' AS codigo_resultado, id_donante, documento, nombre, apellido,
               fecha_nacimiento, telefono, email, domicilio, estado_donante, fecha_alta, id_usuario_alta
        FROM dbo.Donante
        WHERE id_donante = @id_donante;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

        IF ERROR_NUMBER() IN (2601, 2627)
        BEGIN
            SELECT 'DUPLICADO' AS codigo_resultado, CAST(NULL AS INT) AS id_donante;
            RETURN;
        END;

        THROW;
    END CATCH;
END
GO

IF OBJECT_ID('dbo.sp_Donacion_Registrar', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Donacion_Registrar;
GO

CREATE PROCEDURE dbo.sp_Donacion_Registrar
    @id_donante INT,
    @fecha_donacion DATETIME,
    @cantidad_unidades INT,
    @tipo_componente VARCHAR(100),
    @fecha_vencimiento DATE,
    @observaciones VARCHAR(500) = NULL,
    @id_usuario INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;
        EXEC dbo.sp_IntegridadPN1_Exigir;

    IF @id_donante IS NULL OR @id_donante <= 0
       OR @fecha_donacion IS NULL
       OR @cantidad_unidades IS NULL OR @cantidad_unidades <= 0
       OR NULLIF(LTRIM(RTRIM(@tipo_componente)), '') IS NULL
       OR @fecha_vencimiento IS NULL
       OR @fecha_vencimiento < CONVERT(DATE, @fecha_donacion)
       OR @id_usuario IS NULL OR @id_usuario <= 0
       OR NOT EXISTS (SELECT 1 FROM dbo.Donante WHERE id_donante = @id_donante AND estado_donante = 'ACTIVO')
    BEGIN
        SELECT 'DATOS_INVALIDOS' AS codigo_resultado, CAST(NULL AS INT) AS id_donacion;
        SELECT CAST(NULL AS INT) AS id_unidad;
        ROLLBACK TRANSACTION;
        RETURN;
    END;


        INSERT INTO dbo.Donacion
        (
            id_donante, fecha_donacion, cantidad_unidades, tipo_componente,
            fecha_vencimiento, observaciones, id_usuario_responsable
        )
        VALUES
        (
            @id_donante, @fecha_donacion, @cantidad_unidades, LTRIM(RTRIM(@tipo_componente)),
            @fecha_vencimiento, NULLIF(LTRIM(RTRIM(@observaciones)), ''), @id_usuario
        );

        DECLARE @id_donacion INT = CONVERT(INT, SCOPE_IDENTITY());
        DECLARE @indice INT = 1;
        DECLARE @codigo VARCHAR(50);

        WHILE @indice <= @cantidad_unidades
        BEGIN
            SET @codigo = 'UN-' + REPLACE(CONVERT(VARCHAR(36), NEWID()), '-', '');

            INSERT INTO dbo.Unidad
            (
                codigo_identificacion, id_donacion, tipo_componente, fecha_vencimiento,
                estado_operativo, id_usuario_ultima_modificacion
            )
            VALUES
            (
                @codigo, @id_donacion, LTRIM(RTRIM(@tipo_componente)), @fecha_vencimiento,
                'EN_REVISION', @id_usuario
            );

            DECLARE @id_unidad_nueva INT = CONVERT(INT, SCOPE_IDENTITY());

            INSERT INTO dbo.MovimientoUnidad
            (
                id_unidad, tipo_movimiento, estado_anterior, estado_nuevo,
                observacion, id_usuario
            )
            VALUES
            (
                @id_unidad_nueva, 'REGISTRO', NULL, 'EN_REVISION',
                'Unidad generada por la donación.', @id_usuario
            );

            SET @indice = @indice + 1;
        END;

        EXEC dbo.sp_IntegridadPN1_Sellar @id_usuario;
        COMMIT TRANSACTION;

        SELECT 'OK' AS codigo_resultado, d.id_donacion, d.id_donante, d.fecha_donacion,
               d.cantidad_unidades, d.tipo_componente, d.fecha_vencimiento,
               d.observaciones, d.id_usuario_responsable, d.fecha_alta
        FROM dbo.Donacion d
        WHERE d.id_donacion = @id_donacion;

        SELECT u.id_unidad, u.codigo_identificacion, u.id_donacion, u.tipo_componente,
               u.fecha_vencimiento, u.grupo_sanguineo, u.factor_rh, u.observaciones,
               u.estado_operativo, u.fecha_alta, u.fecha_ultima_modificacion,
               u.id_usuario_ultima_modificacion
        FROM dbo.Unidad u
        WHERE u.id_donacion = @id_donacion
        ORDER BY u.id_unidad;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END
GO

IF OBJECT_ID('dbo.sp_Unidad_Listar', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Unidad_Listar;
GO

CREATE PROCEDURE dbo.sp_Unidad_Listar
    @buscar VARCHAR(150) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.id_unidad,
        u.codigo_identificacion,
        u.id_donacion,
        u.tipo_componente,
        u.fecha_vencimiento,
        u.grupo_sanguineo,
        u.factor_rh,
        u.observaciones,
        u.estado_operativo,
        u.fecha_alta,
        u.fecha_ultima_modificacion,
        u.id_usuario_ultima_modificacion,
        d.id_donante,
        d.documento AS documento_donante,
        d.nombre + ' ' + d.apellido AS donante
    FROM dbo.Unidad u
    INNER JOIN dbo.Donacion dn ON dn.id_donacion = u.id_donacion
    INNER JOIN dbo.Donante d ON d.id_donante = dn.id_donante
    WHERE @buscar IS NULL
       OR u.codigo_identificacion LIKE '%' + @buscar + '%'
       OR u.estado_operativo LIKE '%' + @buscar + '%'
       OR d.documento LIKE '%' + @buscar + '%'
       OR d.nombre LIKE '%' + @buscar + '%'
       OR d.apellido LIKE '%' + @buscar + '%'
    ORDER BY u.fecha_alta DESC, u.id_unidad DESC;
END
GO

IF OBJECT_ID('dbo.sp_Unidad_Clasificar', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Unidad_Clasificar;
GO

CREATE PROCEDURE dbo.sp_Unidad_Clasificar
    @id_unidad INT,
    @tipo_componente VARCHAR(100),
    @grupo_sanguineo VARCHAR(5),
    @factor_rh VARCHAR(5),
    @fecha_vencimiento DATE,
    @observaciones VARCHAR(500) = NULL,
    @id_usuario INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;
        EXEC dbo.sp_IntegridadPN1_Exigir;

    IF @id_unidad IS NULL OR @id_unidad <= 0
       OR NULLIF(LTRIM(RTRIM(@tipo_componente)), '') IS NULL
       OR @grupo_sanguineo NOT IN ('A', 'B', 'AB', 'O')
       OR @factor_rh NOT IN ('+', '-')
       OR @fecha_vencimiento IS NULL
       OR @fecha_vencimiento < CONVERT(DATE, GETDATE())
       OR @id_usuario IS NULL OR @id_usuario <= 0
    BEGIN
        SELECT 'DATOS_INVALIDOS' AS codigo_resultado, CAST(NULL AS INT) AS id_unidad;
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Unidad WHERE id_unidad = @id_unidad)
    BEGIN
        SELECT 'NO_ENCONTRADO' AS codigo_resultado, @id_unidad AS id_unidad;
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    IF EXISTS (SELECT 1 FROM dbo.Unidad WHERE id_unidad = @id_unidad AND estado_operativo <> 'EN_REVISION')
    BEGIN
        SELECT 'CONFLICTO_ESTADO' AS codigo_resultado, @id_unidad AS id_unidad;
        ROLLBACK TRANSACTION;
        RETURN;
    END;


        UPDATE dbo.Unidad
        SET tipo_componente = LTRIM(RTRIM(@tipo_componente)),
            grupo_sanguineo = @grupo_sanguineo,
            factor_rh = @factor_rh,
            fecha_vencimiento = @fecha_vencimiento,
            observaciones = NULLIF(LTRIM(RTRIM(@observaciones)), ''),
            fecha_ultima_modificacion = GETDATE(),
            id_usuario_ultima_modificacion = @id_usuario
        WHERE id_unidad = @id_unidad;

        INSERT INTO dbo.MovimientoUnidad
        (id_unidad, tipo_movimiento, estado_anterior, estado_nuevo, observacion, id_usuario)
        VALUES (@id_unidad, 'CLASIFICACION', 'EN_REVISION', 'EN_REVISION', @observaciones, @id_usuario);

        EXEC dbo.sp_IntegridadPN1_Sellar @id_usuario;
        COMMIT TRANSACTION;

        SELECT 'OK' AS codigo_resultado, u.id_unidad, u.codigo_identificacion, u.id_donacion,
               u.tipo_componente, u.fecha_vencimiento, u.grupo_sanguineo, u.factor_rh,
               u.observaciones, u.estado_operativo, u.fecha_alta, u.fecha_ultima_modificacion,
               u.id_usuario_ultima_modificacion
        FROM dbo.Unidad u
        WHERE u.id_unidad = @id_unidad;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END
GO

IF OBJECT_ID('dbo.sp_Unidad_CambiarEstado', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Unidad_CambiarEstado;
GO

CREATE PROCEDURE dbo.sp_Unidad_CambiarEstado
    @id_unidad INT,
    @estado_nuevo VARCHAR(20),
    @observacion VARCHAR(500) = NULL,
    @id_usuario INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;
        EXEC dbo.sp_IntegridadPN1_Exigir;

    IF @id_unidad IS NULL OR @id_unidad <= 0
       OR @estado_nuevo NOT IN ('LIBERADA', 'BLOQUEADA', 'DESCARTADA')
       OR @id_usuario IS NULL OR @id_usuario <= 0
       OR (@estado_nuevo IN ('BLOQUEADA', 'DESCARTADA') AND NULLIF(LTRIM(RTRIM(@observacion)), '') IS NULL)
    BEGIN
        SELECT 'DATOS_INVALIDOS' AS codigo_resultado, CAST(NULL AS INT) AS id_unidad;
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Unidad WHERE id_unidad = @id_unidad)
    BEGIN
        SELECT 'NO_ENCONTRADO' AS codigo_resultado, @id_unidad AS id_unidad;
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    IF EXISTS (
        SELECT 1 FROM dbo.Unidad
        WHERE id_unidad = @id_unidad
          AND (estado_operativo <> 'EN_REVISION'
               OR tipo_componente IS NULL OR grupo_sanguineo IS NULL OR factor_rh IS NULL
               OR (@estado_nuevo = 'LIBERADA' AND fecha_vencimiento < CONVERT(DATE, GETDATE())))
    )
    BEGIN
        SELECT 'CONFLICTO_ESTADO' AS codigo_resultado, @id_unidad AS id_unidad;
        ROLLBACK TRANSACTION;
        RETURN;
    END;


        UPDATE dbo.Unidad
        SET estado_operativo = @estado_nuevo,
            observaciones = COALESCE(NULLIF(LTRIM(RTRIM(@observacion)), ''), observaciones),
            fecha_ultima_modificacion = GETDATE(),
            id_usuario_ultima_modificacion = @id_usuario
        WHERE id_unidad = @id_unidad;

        INSERT INTO dbo.MovimientoUnidad
        (id_unidad, tipo_movimiento, estado_anterior, estado_nuevo, observacion, id_usuario)
        VALUES (@id_unidad, CASE @estado_nuevo WHEN 'LIBERADA' THEN 'LIBERACION' WHEN 'BLOQUEADA' THEN 'BLOQUEO' ELSE 'DESCARTE' END,
                'EN_REVISION', @estado_nuevo, @observacion, @id_usuario);

        EXEC dbo.sp_IntegridadPN1_Sellar @id_usuario;
        COMMIT TRANSACTION;

        SELECT 'OK' AS codigo_resultado, u.id_unidad, u.codigo_identificacion, u.id_donacion,
               u.tipo_componente, u.fecha_vencimiento, u.grupo_sanguineo, u.factor_rh,
               u.observaciones, u.estado_operativo, u.fecha_alta, u.fecha_ultima_modificacion,
               u.id_usuario_ultima_modificacion
        FROM dbo.Unidad u
        WHERE u.id_unidad = @id_unidad;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END
GO
