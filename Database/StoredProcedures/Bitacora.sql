USE [HemovidaGest];
GO

IF OBJECT_ID('dbo.Bitacora', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Bitacora
    (
        id_bitacora INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Bitacora PRIMARY KEY,
        id_usuario INT NULL,
        identificador_usuario NVARCHAR(255) NULL,
        modulo NVARCHAR(100) NOT NULL,
        accion NVARCHAR(100) NOT NULL,
        nivel NVARCHAR(50) NOT NULL,
        descripcion NVARCHAR(500) NULL,
        equipo NVARCHAR(128) NULL,
        fecha_evento DATETIME NOT NULL CONSTRAINT DF_Bitacora_fecha_evento DEFAULT (GETDATE()),
        CONSTRAINT FK_Bitacora_Usuario FOREIGN KEY (id_usuario)
            REFERENCES dbo.Usuario(id_usuario)
    );
END
GO

IF OBJECT_ID('dbo.sp_Bitacora_Registrar', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.sp_Bitacora_Registrar;
END
GO

CREATE PROCEDURE dbo.sp_Bitacora_Registrar
    @id_usuario INT = NULL,
    @identificador_usuario NVARCHAR(255) = NULL,
    @modulo NVARCHAR(100),
    @accion NVARCHAR(100),
    @nivel NVARCHAR(50),
    @descripcion NVARCHAR(500) = NULL,
    @equipo NVARCHAR(128) = NULL,
    @fecha_evento DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Bitacora
    (
        id_usuario,
        identificador_usuario,
        modulo,
        accion,
        nivel,
        descripcion,
        equipo,
        fecha_evento
    )
    VALUES
    (
        @id_usuario,
        @identificador_usuario,
        @modulo,
        @accion,
        @nivel,
        @descripcion,
        @equipo,
        ISNULL(@fecha_evento, GETDATE())
    );

    SELECT
        id_bitacora,
        id_usuario,
        identificador_usuario,
        modulo,
        accion,
        nivel,
        descripcion,
        equipo,
        fecha_evento
    FROM dbo.Bitacora
    WHERE id_bitacora = SCOPE_IDENTITY();
END
GO
