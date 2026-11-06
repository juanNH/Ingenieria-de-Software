-- Declara las referencias que usa la solicitud. Ejecutar después de 09_PN2.sql.
USE [HemovidaGest];
GO
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;

    -- El código de componente tiene el mismo tipo que en el maestro y en Unidad.
    -- Verifica que el cambio de tipo no pierda caracteres en solicitudes existentes.
    IF EXISTS (SELECT 1 FROM sys.columns
        WHERE object_id=OBJECT_ID('dbo.SolicitudTransfusional')
          AND name='tipo_componente' AND system_type_id=TYPE_ID('nvarchar'))
    BEGIN
        IF EXISTS (SELECT 1 FROM dbo.SolicitudTransfusional
            WHERE CONVERT(VARBINARY(200),tipo_componente)
                <> CONVERT(VARBINARY(200),CONVERT(NVARCHAR(100),CONVERT(VARCHAR(100),tipo_componente))))
            THROW 51010,'PN2_COMPONENTE_CONVERSION_PERDERIA_DATOS',1;

        ALTER TABLE dbo.SolicitudTransfusional ALTER COLUMN tipo_componente VARCHAR(100) NOT NULL;
    END;

    IF OBJECT_ID('dbo.FK_SolicitudTransfusional_Componente','F') IS NULL
        ALTER TABLE dbo.SolicitudTransfusional WITH CHECK ADD CONSTRAINT FK_SolicitudTransfusional_Componente
            FOREIGN KEY (tipo_componente) REFERENCES dbo.ComponenteTransfusional(codigo);
    IF OBJECT_ID('dbo.FK_SolicitudTransfusional_Grupo','F') IS NULL
        ALTER TABLE dbo.SolicitudTransfusional WITH CHECK ADD CONSTRAINT FK_SolicitudTransfusional_Grupo
            FOREIGN KEY (grupo_receptor) REFERENCES dbo.GrupoSanguineo(codigo);
    IF OBJECT_ID('dbo.FK_SolicitudTransfusional_Rh','F') IS NULL
        ALTER TABLE dbo.SolicitudTransfusional WITH CHECK ADD CONSTRAINT FK_SolicitudTransfusional_Rh
            FOREIGN KEY (rh_receptor) REFERENCES dbo.FactorRh(codigo);
    IF OBJECT_ID('dbo.FK_SolicitudTransfusional_UsuarioValidacion','F') IS NULL
        ALTER TABLE dbo.SolicitudTransfusional WITH CHECK ADD CONSTRAINT FK_SolicitudTransfusional_UsuarioValidacion
            FOREIGN KEY (id_usuario_validacion) REFERENCES dbo.Usuario(id_usuario);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
