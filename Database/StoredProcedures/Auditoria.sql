USE [HemovidaGest];
GO

IF OBJECT_ID('dbo.Auditoria', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Auditoria
    (
        id_auditoria INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Auditoria PRIMARY KEY,
        entidad NVARCHAR(100) NOT NULL,
        id_entidad INT NOT NULL,
        accion NVARCHAR(50) NOT NULL,
        id_usuario_actor INT NULL,
        identificador_usuario_actor NVARCHAR(255) NULL,
        fecha_evento DATETIME NOT NULL CONSTRAINT DF_Auditoria_fecha_evento DEFAULT (GETDATE()),
        estado_anterior_json NVARCHAR(MAX) NULL,
        estado_nuevo_json NVARCHAR(MAX) NULL,
        cambios_json NVARCHAR(MAX) NOT NULL,
        CONSTRAINT FK_Auditoria_UsuarioActor FOREIGN KEY (id_usuario_actor)
            REFERENCES dbo.Usuario(id_usuario)
    );
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Auditoria_Entidad_IdEntidad_Fecha'
      AND object_id = OBJECT_ID('dbo.Auditoria')
)
BEGIN
    CREATE INDEX IX_Auditoria_Entidad_IdEntidad_Fecha
        ON dbo.Auditoria(entidad, id_entidad, fecha_evento DESC);
END
GO
