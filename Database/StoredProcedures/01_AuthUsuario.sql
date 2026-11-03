USE [HemovidaGest];
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_Usuario_nombre_usuario'
      AND object_id = OBJECT_ID('dbo.Usuario')
)
BEGIN
    CREATE UNIQUE INDEX UX_Usuario_nombre_usuario
        ON dbo.Usuario(nombre_usuario);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_Usuario_email'
      AND object_id = OBJECT_ID('dbo.Usuario')
)
BEGIN
    CREATE UNIQUE INDEX UX_Usuario_email
        ON dbo.Usuario(email);
END
GO

IF COL_LENGTH('dbo.Usuario', 'intentos_login_fallidos') IS NULL
BEGIN
    ALTER TABLE dbo.Usuario
    ADD intentos_login_fallidos INT NOT NULL
        CONSTRAINT DF_Usuario_intentos_login_fallidos DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.Usuario', 'nombre') IS NULL
BEGIN
    ALTER TABLE dbo.Usuario
    ADD nombre NVARCHAR(100) NULL;
END
GO

IF COL_LENGTH('dbo.Usuario', 'apellido') IS NULL
BEGIN
    ALTER TABLE dbo.Usuario
    ADD apellido NVARCHAR(100) NULL;
END
GO

IF COL_LENGTH('dbo.Usuario', 'bloqueo_digitoverificador') IS NULL
BEGIN
    ALTER TABLE dbo.Usuario
    ADD bloqueo_digitoverificador BIT NOT NULL
        CONSTRAINT DF_Usuario_bloqueo_digitoverificador DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.Usuario', 'dvh') IS NULL
BEGIN
    ALTER TABLE dbo.Usuario
    ADD dvh VARCHAR(64) NULL;
END
GO

IF OBJECT_ID('dbo.DigitoVerificadorVertical', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DigitoVerificadorVertical (
        id_digito_verificador_vertical INT IDENTITY(1,1) PRIMARY KEY,
        entidad VARCHAR(100) NOT NULL,
        dvv VARCHAR(64) NOT NULL,
        fecha_calculo DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT UQ_DigitoVerificadorVertical_Entidad UNIQUE (entidad)
    );
END
GO

IF OBJECT_ID('dbo.sp_Usuario_Registrar', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.sp_Usuario_Registrar;
END
GO

CREATE PROCEDURE dbo.sp_Usuario_Registrar
    @nombre_usuario NVARCHAR(100),
    @email NVARCHAR(255),
    @password_hash NVARCHAR(255),
    @nombre NVARCHAR(100) = NULL,
    @apellido NVARCHAR(100) = NULL,
    @id_usuario_nuevo INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @id_idioma_default INT;

    BEGIN TRY
        IF EXISTS (
            SELECT 1
            FROM dbo.Usuario
            WHERE nombre_usuario = @nombre_usuario
        )
        BEGIN
            SELECT
                'USUARIO_EXISTENTE' AS codigo_resultado,
                'Ya existe un usuario con ese nombre de usuario.' AS mensaje,
                CAST(NULL AS INT) AS id_usuario;
            RETURN;
        END;

        IF EXISTS (
            SELECT 1
            FROM dbo.Usuario
            WHERE email = @email
        )
        BEGIN
            SELECT
                'EMAIL_EXISTENTE' AS codigo_resultado,
                'Ya existe un usuario con ese email.' AS mensaje,
                CAST(NULL AS INT) AS id_usuario;
            RETURN;
        END;

        SELECT @id_idioma_default = id_idioma
        FROM dbo.Idioma
        WHERE codigo = 'es-AR'
          AND estado_idioma = 'ACTIVO';

        IF @id_idioma_default IS NULL
        BEGIN
            SELECT
                'IDIOMA_DEFAULT_INEXISTENTE' AS codigo_resultado,
                'No existe un idioma activo con codigo es-AR.' AS mensaje,
                CAST(NULL AS INT) AS id_usuario;
            RETURN;
        END;

        BEGIN TRANSACTION;

        INSERT INTO dbo.Usuario
        (
            id_idioma,
            nombre_usuario,
            email,
            password_hash,
            nombre,
            apellido,
            estado_usuario,
            intentos_login_fallidos,
            fecha_alta
        )
        VALUES
        (
            @id_idioma_default,
            @nombre_usuario,
            @email,
            @password_hash,
            @nombre,
            @apellido,
            'ACTIVO',
            0,
            GETDATE()
        );

        SET @id_usuario_nuevo = SCOPE_IDENTITY();

        COMMIT TRANSACTION;

        SELECT
            'OK' AS codigo_resultado,
            'Usuario registrado con exito.' AS mensaje,
            u.id_usuario,
            u.id_idioma,
            u.nombre_usuario,
            u.email,
            u.nombre,
            u.apellido,
            u.estado_usuario,
            u.intentos_login_fallidos,
            u.bloqueo_digitoverificador,
            u.dvh,
            u.fecha_alta
        FROM dbo.Usuario u
        WHERE u.id_usuario = @id_usuario_nuevo;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END;

        IF ERROR_NUMBER() IN (2601, 2627)
        BEGIN
            IF CHARINDEX('email', ERROR_MESSAGE()) > 0
            BEGIN
                SELECT
                    'EMAIL_EXISTENTE' AS codigo_resultado,
                    'Ya existe un usuario con ese email.' AS mensaje,
                    CAST(NULL AS INT) AS id_usuario;
                RETURN;
            END;

            SELECT
                'USUARIO_EXISTENTE' AS codigo_resultado,
                'Ya existe un usuario con ese nombre de usuario.' AS mensaje,
                CAST(NULL AS INT) AS id_usuario;
            RETURN;
        END;

        THROW;
    END CATCH;
END
GO

IF OBJECT_ID('dbo.sp_Usuario_Login', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.sp_Usuario_Login;
END
GO

CREATE PROCEDURE dbo.sp_Usuario_Login
    @identificador NVARCHAR(255),
    @password_hash NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @id_usuario INT;

    SELECT TOP (1)
        @id_usuario = u.id_usuario
    FROM dbo.Usuario u
    WHERE (u.nombre_usuario = @identificador OR u.email = @identificador)
      AND u.password_hash = @password_hash
      AND u.estado_usuario = 'ACTIVO';

    IF @id_usuario IS NOT NULL
    BEGIN
        UPDATE dbo.Usuario
        SET intentos_login_fallidos = 0
        WHERE id_usuario = @id_usuario;
    END;

    SELECT TOP (1)
        u.id_usuario,
        u.id_idioma,
        u.nombre_usuario,
        u.email,
        u.nombre,
        u.apellido,
        u.estado_usuario,
        u.intentos_login_fallidos,
        u.bloqueo_digitoverificador,
        u.dvh,
        u.fecha_alta
    FROM dbo.Usuario u
    WHERE u.id_usuario = @id_usuario;

    SELECT
        ur.id_usuario,
        ur.id_rol,
        r.nombre AS nombre_rol
    FROM dbo.UsuarioRol ur
    INNER JOIN dbo.Rol r
        ON r.id_rol = ur.id_rol
    INNER JOIN dbo.Usuario u
        ON u.id_usuario = ur.id_usuario
    WHERE u.id_usuario = @id_usuario
      AND ur.estado_usuario_rol = 'ACTIVO'
      AND r.estado_rol = 'ACTIVO';
END
GO
