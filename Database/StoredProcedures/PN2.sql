-- HemovidaGest: solicitud, reserva, entrega y trazabilidad (PN2).
-- Instalacion incremental SQL Server 2016 SP1+, compatibilidad 130 o superior.
-- Ejecutar despues de IntegridadPN1.sql y PN1.sql, con la aplicacion detenida.
-- Completar luego con IntegridadPN2.sql antes de habilitar operaciones PN2.
USE [HemovidaGest];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.sp_IntegridadPN1_Exigir','P') IS NULL
    THROW 51010,'PN2_REQUIRES_PN1_INTEGRITY',1;
GO
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;
    EXEC dbo.sp_IntegridadPN1_Exigir;
    COMMIT;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK;
    THROW;
END CATCH;
GO

-- Las columnas nuevas son anulables para no modificar datos ni hashes PN1.
IF COL_LENGTH('dbo.Unidad','ubicacion') IS NULL
    ALTER TABLE dbo.Unidad ADD ubicacion NVARCHAR(150) NULL;
GO

IF OBJECT_ID('dbo.SolicitudTransfusional','U') IS NULL
CREATE TABLE dbo.SolicitudTransfusional
(
    id_solicitud_transfusional INT IDENTITY(1,1) CONSTRAINT PK_SolicitudTransfusional PRIMARY KEY,
    paciente_receptor NVARCHAR(150) NOT NULL,
    tipo_componente NVARCHAR(100) NOT NULL,
    cantidad_requerida INT NOT NULL,
    id_usuario_solicitante INT NOT NULL,
    observaciones NVARCHAR(500) NULL,
    fecha_solicitud DATETIME NOT NULL CONSTRAINT DF_SolicitudTransfusional_Fecha DEFAULT GETDATE(),
    estado_solicitud VARCHAR(20) NOT NULL CONSTRAINT DF_SolicitudTransfusional_Estado DEFAULT 'PENDIENTE',
    dvh VARCHAR(64) NULL,
    CONSTRAINT CK_SolicitudTransfusional_Cantidad CHECK (cantidad_requerida > 0),
    CONSTRAINT CK_SolicitudTransfusional_Estado CHECK (estado_solicitud IN ('PENDIENTE','PARCIAL','RESERVADA','ENTREGADA')),
    CONSTRAINT FK_SolicitudTransfusional_Usuario FOREIGN KEY (id_usuario_solicitante) REFERENCES dbo.Usuario(id_usuario)
);
GO

-- Los registros anteriores conservan valores NULL y necesitan estudios nuevos.
IF COL_LENGTH('dbo.SolicitudTransfusional','documento_receptor') IS NULL
    ALTER TABLE dbo.SolicitudTransfusional ADD documento_receptor NVARCHAR(50) NULL, grupo_receptor VARCHAR(5) NULL,
        rh_receptor VARCHAR(5) NULL, laboratorio_estudio NVARCHAR(150) NULL, referencia_estudio NVARCHAR(150) NULL,
        fecha_estudio DATE NULL, resultado_anticuerpos NVARCHAR(500) NULL, id_usuario_validacion INT NULL,
        fecha_validacion DATETIME NULL;
IF COL_LENGTH('dbo.SolicitudTransfusional','nombre_solicitante') IS NULL
    ALTER TABLE dbo.SolicitudTransfusional ADD nombre_solicitante NVARCHAR(150) NULL;
GO

-- Filtro preliminar, deliberadamente limitado a componentes identificados.
-- La aptitud final debe confirmarla el personal con las pruebas correspondientes.
CREATE OR ALTER FUNCTION dbo.fn_PN2_GrupoCandidato
(@componente NVARCHAR(100), @grupo VARCHAR(5), @rh VARCHAR(5), @receptor VARCHAR(5), @rh_receptor VARCHAR(5))
RETURNS BIT
AS
BEGIN
    IF @grupo NOT IN ('O','A','B','AB') OR @receptor NOT IN ('O','A','B','AB')
       OR @rh NOT IN ('+','-') OR @rh_receptor NOT IN ('+','-') RETURN 0;
    DECLARE @tipo NVARCHAR(100)=UPPER(LTRIM(RTRIM(@componente))) COLLATE Latin1_General_CI_AI;
    IF @tipo COLLATE Latin1_General_CI_AI IN (N'GLOBULOS ROJOS', N'CONCENTRADO DE GLOBULOS ROJOS', N'ERITROCITOS', N'CONCENTRADO DE ERITROCITOS')
    BEGIN
        IF @rh_receptor='-' AND @rh<>'-' RETURN 0;
        IF @grupo='O' OR @grupo=@receptor OR (@receptor='AB' AND @grupo IN ('A','B')) RETURN 1;
    END;
    IF @tipo COLLATE Latin1_General_CI_AI IN (N'PLASMA', N'PLASMA FRESCO CONGELADO')
    BEGIN
        IF @grupo='AB' OR @grupo=@receptor OR (@receptor='O' AND @grupo IN ('A','B')) RETURN 1;
    END;
    -- Plaquetas: lista conservadora de grupo identico; la validacion es individual.
    IF @tipo COLLATE Latin1_General_CI_AI IN (N'PLAQUETAS', N'CONCENTRADO DE PLAQUETAS')
       AND @grupo=@receptor AND (@rh_receptor='+' OR @rh='-') RETURN 1;
    RETURN 0;
END;
GO

IF OBJECT_ID('dbo.Reserva','U') IS NULL
CREATE TABLE dbo.Reserva
(
    id_reserva INT IDENTITY(1,1) CONSTRAINT PK_Reserva PRIMARY KEY,
    id_solicitud_transfusional INT NOT NULL,
    id_unidad INT NOT NULL,
    estado_reserva VARCHAR(20) NOT NULL CONSTRAINT DF_Reserva_Estado DEFAULT 'ACTIVA',
    observacion NVARCHAR(500) NULL,
    id_usuario_responsable INT NOT NULL,
    fecha_reserva DATETIME NOT NULL CONSTRAINT DF_Reserva_Fecha DEFAULT GETDATE(),
    fecha_cierre DATETIME NULL,
    motivo_cancelacion NVARCHAR(500) NULL,
    dvh VARCHAR(64) NULL,
    CONSTRAINT CK_Reserva_Estado CHECK (estado_reserva IN ('ACTIVA','ENTREGADA','CANCELADA')),
    CONSTRAINT FK_Reserva_Solicitud FOREIGN KEY (id_solicitud_transfusional) REFERENCES dbo.SolicitudTransfusional(id_solicitud_transfusional),
    CONSTRAINT FK_Reserva_Unidad FOREIGN KEY (id_unidad) REFERENCES dbo.Unidad(id_unidad),
    CONSTRAINT FK_Reserva_Usuario FOREIGN KEY (id_usuario_responsable) REFERENCES dbo.Usuario(id_usuario)
);
GO

IF OBJECT_ID('dbo.Entrega','U') IS NULL
CREATE TABLE dbo.Entrega
(
    id_entrega INT IDENTITY(1,1) CONSTRAINT PK_Entrega PRIMARY KEY,
    id_reserva INT NOT NULL CONSTRAINT UQ_Entrega_Reserva UNIQUE,
    observacion NVARCHAR(500) NULL,
    id_usuario_responsable INT NOT NULL,
    fecha_entrega DATETIME NOT NULL CONSTRAINT DF_Entrega_Fecha DEFAULT GETDATE(),
    fecha_destino DATETIME NULL,
    destino VARCHAR(20) NULL,
    motivo_destino NVARCHAR(500) NULL,
    id_usuario_destino INT NULL,
    dvh VARCHAR(64) NULL,
    CONSTRAINT CK_Entrega_Destino CHECK (destino IS NULL OR destino IN ('TRANSFUNDIDA','DEVUELTA','DESCARTADA')),
    CONSTRAINT CK_Entrega_DestinoCompleto CHECK (
        (destino IS NULL AND fecha_destino IS NULL AND id_usuario_destino IS NULL)
        OR (destino IS NOT NULL AND fecha_destino IS NOT NULL AND id_usuario_destino IS NOT NULL)),
    CONSTRAINT FK_Entrega_Reserva FOREIGN KEY (id_reserva) REFERENCES dbo.Reserva(id_reserva),
    CONSTRAINT FK_Entrega_Usuario FOREIGN KEY (id_usuario_responsable) REFERENCES dbo.Usuario(id_usuario),
    CONSTRAINT FK_Entrega_UsuarioDestino FOREIGN KEY (id_usuario_destino) REFERENCES dbo.Usuario(id_usuario)
);
GO

IF COL_LENGTH('dbo.MovimientoUnidad','id_solicitud_transfusional') IS NULL
    ALTER TABLE dbo.MovimientoUnidad ADD id_solicitud_transfusional INT NULL;
IF COL_LENGTH('dbo.MovimientoUnidad','id_reserva') IS NULL
    ALTER TABLE dbo.MovimientoUnidad ADD id_reserva INT NULL;
IF COL_LENGTH('dbo.MovimientoUnidad','id_entrega') IS NULL
    ALTER TABLE dbo.MovimientoUnidad ADD id_entrega INT NULL;
GO
IF OBJECT_ID('dbo.FK_MovimientoUnidad_Solicitud','F') IS NULL
    ALTER TABLE dbo.MovimientoUnidad ADD CONSTRAINT FK_MovimientoUnidad_Solicitud FOREIGN KEY(id_solicitud_transfusional) REFERENCES dbo.SolicitudTransfusional(id_solicitud_transfusional);
IF OBJECT_ID('dbo.FK_MovimientoUnidad_Reserva','F') IS NULL
    ALTER TABLE dbo.MovimientoUnidad ADD CONSTRAINT FK_MovimientoUnidad_Reserva FOREIGN KEY(id_reserva) REFERENCES dbo.Reserva(id_reserva);
IF OBJECT_ID('dbo.FK_MovimientoUnidad_Entrega','F') IS NULL
    ALTER TABLE dbo.MovimientoUnidad ADD CONSTRAINT FK_MovimientoUnidad_Entrega FOREIGN KEY(id_entrega) REFERENCES dbo.Entrega(id_entrega);
GO

-- Reutiliza el mismo arbol de permisos de la aplicacion. Las consultas siguen
-- disponibles ante un incidente de integridad; Exigir bloquea las escrituras.
CREATE OR ALTER FUNCTION dbo.fn_PN2_TienePermiso(@id_usuario INT,@permiso VARCHAR(100))
RETURNS BIT
AS
BEGIN
    DECLARE @permitido BIT=0;
    ;WITH asignaciones AS (
        SELECT id_componente FROM dbo.UsuarioComponentePermiso
        WHERE id_usuario=@id_usuario AND estado_usuario_componente='ACTIVO'
        UNION
        SELECT c.id_componente FROM dbo.UsuarioRol ur JOIN dbo.Rol r ON r.id_rol=ur.id_rol
        JOIN dbo.ComponentePermiso c ON c.nombre=r.nombre AND c.tipo='FAMILIA'
        WHERE ur.id_usuario=@id_usuario AND ur.estado_usuario_rol='ACTIVO' AND r.estado_rol='ACTIVO'
        UNION
        SELECT c.id_componente FROM dbo.UsuarioRol ur JOIN dbo.Rol r ON r.id_rol=ur.id_rol
        JOIN dbo.RolPermiso rp ON rp.id_rol=r.id_rol JOIN dbo.Permiso p ON p.id_permiso=rp.id_permiso
        JOIN dbo.ComponentePermiso c ON c.codigo=p.codigo
        WHERE ur.id_usuario=@id_usuario AND ur.estado_usuario_rol='ACTIVO' AND r.estado_rol='ACTIVO'
    ), arbol AS (
        SELECT c.id_componente,c.codigo FROM asignaciones a JOIN dbo.ComponentePermiso c ON c.id_componente=a.id_componente
        JOIN dbo.Usuario u ON u.id_usuario=@id_usuario
        WHERE u.estado_usuario='ACTIVO' AND c.estado_componente='ACTIVO'
        UNION ALL
        SELECT c.id_componente,c.codigo FROM arbol a JOIN dbo.ComponentePermisoRelacion r ON r.id_padre=a.id_componente
        JOIN dbo.ComponentePermiso c ON c.id_componente=r.id_hijo WHERE c.estado_componente='ACTIVO'
    )
    SELECT @permitido=1 FROM arbol WHERE codigo=@permiso OPTION(MAXRECURSION 100);
    RETURN @permitido;
END;
GO
CREATE OR ALTER PROCEDURE dbo.sp_PN2_Autorizar @id_usuario INT,@permiso VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    IF dbo.fn_PN2_TienePermiso(@id_usuario,@permiso)=0
        THROW 51003,'OPERATION_NOT_AUTHORIZED',1;
END;
GO
CREATE OR ALTER PROCEDURE dbo.sp_PN2_Exigir
AS
BEGIN
    SET NOCOUNT ON;
    IF OBJECT_ID('dbo.v_IntegridadPN2_Instalada','V') IS NULL
        THROW 51002,'PN2_REQUIRES_INTEGRITY_MIGRATION',1;
    IF OBJECT_DEFINITION(OBJECT_ID('dbo.v_IntegridadPN1_Datos')) NOT LIKE '%SolicitudTransfusional%'
       OR OBJECT_DEFINITION(OBJECT_ID('dbo.v_IntegridadPN1_Datos')) NOT LIKE '%id_entrega%'
       OR OBJECT_DEFINITION(OBJECT_ID('dbo.v_IntegridadPN1_Datos')) NOT LIKE '%grupo_receptor%'
        THROW 51002,'PN2_INTEGRITY_DEFINITIONS_DOWNGRADED',1;
    EXEC dbo.sp_IntegridadPN1_Exigir;
END;
GO

CREATE OR ALTER VIEW dbo.v_PN2_SolicitudCantidades AS
SELECT s.id_solicitud_transfusional,
    COALESCE(r.cantidad_reservada,0) cantidad_reservada,
    COALESCE(e.cantidad_entregada,0) cantidad_entregada,
    CASE WHEN s.cantidad_requerida-COALESCE(r.cantidad_reservada,0)-COALESCE(e.cantidad_entregada,0)>0
         THEN s.cantidad_requerida-COALESCE(r.cantidad_reservada,0)-COALESCE(e.cantidad_entregada,0) ELSE 0 END saldo_pendiente
FROM dbo.SolicitudTransfusional s
OUTER APPLY (SELECT COUNT(*) cantidad_reservada FROM dbo.Reserva r WHERE r.id_solicitud_transfusional=s.id_solicitud_transfusional AND r.estado_reserva='ACTIVA') r
OUTER APPLY (SELECT COUNT(*) cantidad_entregada FROM dbo.Reserva r JOIN dbo.Entrega e ON e.id_reserva=r.id_reserva
             WHERE r.id_solicitud_transfusional=s.id_solicitud_transfusional AND (e.destino IS NULL OR e.destino='TRANSFUNDIDA')) e;
GO
CREATE OR ALTER PROCEDURE dbo.sp_PN2_Solicitud_ActualizarEstado @id_solicitud_transfusional INT
AS
BEGIN
    SET NOCOUNT ON;
    IF @@TRANCOUNT=0 THROW 51002,'INTEGRITY_TRANSACTION_REQUIRED',1;
    UPDATE s SET estado_solicitud=CASE
        WHEN c.cantidad_entregada>=s.cantidad_requerida THEN 'ENTREGADA'
        WHEN c.cantidad_reservada+c.cantidad_entregada>=s.cantidad_requerida THEN 'RESERVADA'
        WHEN c.cantidad_reservada+c.cantidad_entregada>0 THEN 'PARCIAL' ELSE 'PENDIENTE' END
    FROM dbo.SolicitudTransfusional s JOIN dbo.v_PN2_SolicitudCantidades c ON c.id_solicitud_transfusional=s.id_solicitud_transfusional
    WHERE s.id_solicitud_transfusional=@id_solicitud_transfusional;
END;
GO
DENY EXECUTE ON dbo.sp_PN2_Solicitud_ActualizarEstado TO public;
GO

CREATE OR ALTER PROCEDURE dbo.sp_PN2_Inventario_Listar
    @buscar NVARCHAR(150)=NULL,@estado VARCHAR(20)=NULL,@id_usuario INT
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.sp_PN2_Autorizar @id_usuario,'INVENTARIO_VER';
    SELECT u.id_unidad,u.codigo_identificacion,u.id_donacion,u.tipo_componente,u.fecha_vencimiento,
           u.grupo_sanguineo,u.factor_rh,u.observaciones,u.estado_operativo,u.ubicacion,
           u.fecha_alta,u.fecha_ultima_modificacion,u.id_usuario_ultima_modificacion
    FROM dbo.Unidad u
    WHERE (@estado IS NULL OR u.estado_operativo=@estado)
      AND (@buscar IS NULL OR u.codigo_identificacion LIKE '%'+@buscar+'%' OR u.tipo_componente LIKE '%'+@buscar+'%'
           OR u.ubicacion LIKE '%'+@buscar+'%' OR u.grupo_sanguineo+u.factor_rh LIKE '%'+@buscar+'%')
    ORDER BY u.fecha_vencimiento,u.id_unidad;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_SolicitudTransfusional_Listar
    @buscar NVARCHAR(150)=NULL,@id_usuario INT
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.sp_PN2_Autorizar @id_usuario,'SOLICITUD_VER';
    DECLARE @todas BIT=CASE WHEN dbo.fn_PN2_TienePermiso(@id_usuario,'INVENTARIO_VER')=1 OR dbo.fn_PN2_TienePermiso(@id_usuario,'RESERVA_VER')=1 THEN 1 ELSE 0 END;
    SELECT s.id_solicitud_transfusional,s.paciente_receptor,s.fecha_solicitud,s.tipo_componente,
        s.cantidad_requerida,s.id_usuario_solicitante,COALESCE(s.nombre_solicitante,u.nombre+' '+u.apellido) profesional_solicitante,
        s.estado_solicitud,c.cantidad_reservada,c.cantidad_entregada,c.saldo_pendiente,
        s.documento_receptor,s.grupo_receptor,s.rh_receptor,s.laboratorio_estudio,s.referencia_estudio,
        s.fecha_estudio,s.resultado_anticuerpos,s.id_usuario_validacion,s.fecha_validacion
    FROM dbo.SolicitudTransfusional s JOIN dbo.Usuario u ON u.id_usuario=s.id_usuario_solicitante
    JOIN dbo.v_PN2_SolicitudCantidades c ON c.id_solicitud_transfusional=s.id_solicitud_transfusional
    WHERE (@todas=1 OR s.id_usuario_solicitante=@id_usuario)
      AND (@buscar IS NULL OR s.paciente_receptor LIKE '%'+@buscar+'%' OR s.tipo_componente LIKE '%'+@buscar+'%'
           OR CONVERT(VARCHAR(20),s.id_solicitud_transfusional)=@buscar)
    ORDER BY s.fecha_solicitud DESC,s.id_solicitud_transfusional DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Reserva_Listar @id_solicitud_transfusional INT=NULL,@id_usuario INT
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.sp_PN2_Autorizar @id_usuario,'RESERVA_VER';
    SELECT r.id_reserva,r.id_solicitud_transfusional,r.id_unidad,u.codigo_identificacion,
        s.paciente_receptor,s.tipo_componente,r.estado_reserva,r.fecha_reserva,r.id_usuario_responsable,
        a.nombre+' '+a.apellido responsable,r.observacion,r.fecha_cierre,r.motivo_cancelacion
    FROM dbo.Reserva r JOIN dbo.Unidad u ON u.id_unidad=r.id_unidad
    JOIN dbo.SolicitudTransfusional s ON s.id_solicitud_transfusional=r.id_solicitud_transfusional
    JOIN dbo.Usuario a ON a.id_usuario=r.id_usuario_responsable
    WHERE @id_solicitud_transfusional IS NULL OR r.id_solicitud_transfusional=@id_solicitud_transfusional
    ORDER BY r.fecha_reserva DESC,r.id_reserva DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Entrega_Listar @id_solicitud_transfusional INT=NULL,@id_usuario INT
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.sp_PN2_Autorizar @id_usuario,'RESERVA_VER';
    SELECT e.id_entrega,e.id_reserva,r.id_solicitud_transfusional,r.id_unidad,u.codigo_identificacion,
        s.paciente_receptor,s.tipo_componente,e.fecha_entrega,e.id_usuario_responsable,
        a.nombre+' '+a.apellido responsable,e.observacion,e.destino,e.fecha_destino,e.motivo_destino,e.id_usuario_destino
    FROM dbo.Entrega e JOIN dbo.Reserva r ON r.id_reserva=e.id_reserva JOIN dbo.Unidad u ON u.id_unidad=r.id_unidad
    JOIN dbo.SolicitudTransfusional s ON s.id_solicitud_transfusional=r.id_solicitud_transfusional
    JOIN dbo.Usuario a ON a.id_usuario=e.id_usuario_responsable
    WHERE @id_solicitud_transfusional IS NULL OR r.id_solicitud_transfusional=@id_solicitud_transfusional
    ORDER BY e.fecha_entrega DESC,e.id_entrega DESC;
END;
GO

IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Reserva') AND name='UX_Reserva_UnidadActiva')
    CREATE UNIQUE INDEX UX_Reserva_UnidadActiva ON dbo.Reserva(id_unidad) WHERE estado_reserva='ACTIVA';
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Reserva') AND name='IX_Reserva_Solicitud')
    CREATE INDEX IX_Reserva_Solicitud ON dbo.Reserva(id_solicitud_transfusional,estado_reserva);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.SolicitudTransfusional') AND name='IX_SolicitudTransfusional_Usuario_Fecha')
    CREATE INDEX IX_SolicitudTransfusional_Usuario_Fecha ON dbo.SolicitudTransfusional(id_usuario_solicitante,fecha_solicitud DESC);
GO

ALTER TABLE dbo.Unidad DROP CONSTRAINT CK_Unidad_Estado;
ALTER TABLE dbo.Unidad ADD CONSTRAINT CK_Unidad_Estado CHECK (
    estado_operativo IN ('EN_REVISION','LIBERADA','BLOQUEADA','DESCARTADA','RESERVADA','ENTREGADA','TRANSFUNDIDA','DEVUELTA'));
ALTER TABLE dbo.MovimientoUnidad DROP CONSTRAINT CK_MovimientoUnidad_Tipo;
ALTER TABLE dbo.MovimientoUnidad ADD CONSTRAINT CK_MovimientoUnidad_Tipo CHECK (
    tipo_movimiento IN ('REGISTRO','CLASIFICACION','LIBERACION','BLOQUEO','DESCARTE','RESTAURACION','UBICACION','RESERVA','ENTREGA','TRANSFUSION','DEVOLUCION','CANCELACION','REINGRESO'));
ALTER TABLE dbo.MovimientoUnidad DROP CONSTRAINT CK_MovimientoUnidad_EstadoNuevo;
ALTER TABLE dbo.MovimientoUnidad ADD CONSTRAINT CK_MovimientoUnidad_EstadoNuevo CHECK (
    estado_nuevo IN ('EN_REVISION','LIBERADA','BLOQUEADA','DESCARTADA','RESERVADA','ENTREGADA','TRANSFUNDIDA','DEVUELTA'));
GO

CREATE OR ALTER PROCEDURE dbo.sp_SolicitudTransfusional_Registrar
    @paciente_receptor NVARCHAR(150),@tipo_componente NVARCHAR(100),@cantidad_requerida INT,@id_usuario INT,
    @documento_receptor NVARCHAR(50),@grupo_receptor VARCHAR(5),@rh_receptor VARCHAR(5),
    @laboratorio_estudio NVARCHAR(150),@referencia_estudio NVARCHAR(150),@fecha_estudio DATE,
    @resultado_anticuerpos NVARCHAR(500),@nombre_solicitante NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        EXEC dbo.sp_IntegridadPN1_Autorizar @id_usuario,'SOLICITUD_CREAR';
        EXEC dbo.sp_PN2_Exigir;
        IF NULLIF(LTRIM(RTRIM(@paciente_receptor)),'') IS NULL
           OR NULLIF(LTRIM(RTRIM(@tipo_componente)),'') IS NULL OR @cantidad_requerida IS NULL OR @cantidad_requerida<=0
           OR NULLIF(LTRIM(RTRIM(@documento_receptor)),'') IS NULL
           OR NULLIF(LTRIM(RTRIM(@nombre_solicitante)),'') IS NULL
           OR NOT ((@grupo_receptor IS NULL AND @rh_receptor IS NULL AND NULLIF(LTRIM(RTRIM(@laboratorio_estudio)),'') IS NULL
                    AND NULLIF(LTRIM(RTRIM(@referencia_estudio)),'') IS NULL AND @fecha_estudio IS NULL
                    AND NULLIF(LTRIM(RTRIM(@resultado_anticuerpos)),'') IS NULL)
                 OR (@grupo_receptor IS NOT NULL AND @grupo_receptor IN ('O','A','B','AB') AND @rh_receptor IS NOT NULL AND @rh_receptor IN ('+','-')
                    AND NULLIF(LTRIM(RTRIM(@laboratorio_estudio)),'') IS NOT NULL AND NULLIF(LTRIM(RTRIM(@referencia_estudio)),'') IS NOT NULL
                    AND @fecha_estudio IS NOT NULL AND @fecha_estudio<=CONVERT(DATE,GETDATE())
                    AND NULLIF(LTRIM(RTRIM(@resultado_anticuerpos)),'') IS NOT NULL))
        BEGIN
            ROLLBACK; SELECT 'DATOS_INVALIDOS' codigo_resultado,CAST(NULL AS INT) id_resultado; RETURN;
        END;
        INSERT dbo.SolicitudTransfusional(paciente_receptor,tipo_componente,cantidad_requerida,id_usuario_solicitante,
            documento_receptor,grupo_receptor,rh_receptor,laboratorio_estudio,referencia_estudio,fecha_estudio,resultado_anticuerpos,nombre_solicitante)
        VALUES(LTRIM(RTRIM(@paciente_receptor)),LTRIM(RTRIM(@tipo_componente)),@cantidad_requerida,@id_usuario,
            LTRIM(RTRIM(@documento_receptor)),@grupo_receptor,@rh_receptor,LTRIM(RTRIM(@laboratorio_estudio)),
            LTRIM(RTRIM(@referencia_estudio)),@fecha_estudio,LTRIM(RTRIM(@resultado_anticuerpos)),LTRIM(RTRIM(@nombre_solicitante)));
        DECLARE @id INT=CONVERT(INT,SCOPE_IDENTITY());
        EXEC dbo.sp_IntegridadPN1_Sellar @id_usuario;
        COMMIT; SELECT 'OK' codigo_resultado,@id id_resultado;
    END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_PN2_ActualizarEstudio
    @id_solicitud INT,@id_usuario INT,@documento_receptor NVARCHAR(50),@grupo_receptor VARCHAR(5),@rh_receptor VARCHAR(5),
    @laboratorio_estudio NVARCHAR(150),@referencia_estudio NVARCHAR(150),@fecha_estudio DATE,
    @resultado_anticuerpos NVARCHAR(500),@nombre_solicitante NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        EXEC dbo.sp_IntegridadPN1_Autorizar @id_usuario,'SOLICITUD_CREAR';
        EXEC dbo.sp_PN2_Exigir;
        IF NULLIF(LTRIM(RTRIM(@documento_receptor)),'') IS NULL
           OR @grupo_receptor IS NULL OR @grupo_receptor NOT IN ('O','A','B','AB')
           OR @rh_receptor IS NULL OR @rh_receptor NOT IN ('+','-')
           OR NULLIF(LTRIM(RTRIM(@laboratorio_estudio)),'') IS NULL OR NULLIF(LTRIM(RTRIM(@referencia_estudio)),'') IS NULL
           OR @fecha_estudio IS NULL OR @fecha_estudio>CONVERT(DATE,GETDATE())
           OR NULLIF(LTRIM(RTRIM(@resultado_anticuerpos)),'') IS NULL OR NULLIF(LTRIM(RTRIM(@nombre_solicitante)),'') IS NULL
        BEGIN ROLLBACK; SELECT 'DATOS_INVALIDOS' codigo_resultado,CAST(NULL AS INT) id_resultado; RETURN; END;
        IF NOT EXISTS(SELECT 1 FROM dbo.SolicitudTransfusional s WHERE s.id_solicitud_transfusional=@id_solicitud
            AND s.id_usuario_validacion IS NULL AND (s.id_usuario_solicitante=@id_usuario OR dbo.fn_PN2_TienePermiso(@id_usuario,'INVENTARIO_VER')=1)
            AND NOT EXISTS(SELECT 1 FROM dbo.Reserva r WHERE r.id_solicitud_transfusional=s.id_solicitud_transfusional AND r.estado_reserva IN ('ACTIVA','ENTREGADA')))
        BEGIN ROLLBACK; SELECT 'CONFLICTO_ESTADO' codigo_resultado,CAST(NULL AS INT) id_resultado; RETURN; END;
        UPDATE dbo.SolicitudTransfusional SET documento_receptor=LTRIM(RTRIM(@documento_receptor)),grupo_receptor=@grupo_receptor,
            rh_receptor=@rh_receptor,laboratorio_estudio=LTRIM(RTRIM(@laboratorio_estudio)),referencia_estudio=LTRIM(RTRIM(@referencia_estudio)),
            fecha_estudio=@fecha_estudio,resultado_anticuerpos=LTRIM(RTRIM(@resultado_anticuerpos)),nombre_solicitante=LTRIM(RTRIM(@nombre_solicitante))
        WHERE id_solicitud_transfusional=@id_solicitud;
        EXEC dbo.sp_IntegridadPN1_Sellar @id_usuario;
        COMMIT; SELECT 'OK' codigo_resultado,@id_solicitud id_resultado;
    END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_PN2_ValidarEstudio @id_solicitud INT,@id_usuario INT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        EXEC dbo.sp_IntegridadPN1_Autorizar @id_usuario,'RESERVA_CREAR';
        EXEC dbo.sp_PN2_Exigir;
        IF NOT EXISTS(SELECT 1 FROM dbo.SolicitudTransfusional WHERE id_solicitud_transfusional=@id_solicitud
            AND documento_receptor IS NOT NULL AND grupo_receptor IN ('O','A','B','AB') AND rh_receptor IN ('+','-')
            AND laboratorio_estudio IS NOT NULL AND referencia_estudio IS NOT NULL AND fecha_estudio IS NOT NULL
            AND resultado_anticuerpos IS NOT NULL AND id_usuario_validacion IS NULL)
        BEGIN ROLLBACK; SELECT 'DATOS_INVALIDOS' codigo_resultado,CAST(NULL AS INT) id_resultado; RETURN; END;
        UPDATE dbo.SolicitudTransfusional SET id_usuario_validacion=@id_usuario,fecha_validacion=GETDATE()
        WHERE id_solicitud_transfusional=@id_solicitud;
        EXEC dbo.sp_IntegridadPN1_Sellar @id_usuario;
        COMMIT; SELECT 'OK' codigo_resultado,@id_solicitud id_resultado;
    END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_PN2_UnidadesCandidatas @id_solicitud INT,@id_usuario INT
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.sp_PN2_Autorizar @id_usuario,'INVENTARIO_VER';
    EXEC dbo.sp_PN2_Autorizar @id_usuario,'RESERVA_VER';
    SELECT u.id_unidad,u.codigo_identificacion,u.id_donacion,u.tipo_componente,u.fecha_vencimiento,
           u.grupo_sanguineo,u.factor_rh,u.observaciones,u.estado_operativo,u.ubicacion,
           u.fecha_alta,u.fecha_ultima_modificacion,u.id_usuario_ultima_modificacion
    FROM dbo.Unidad u JOIN dbo.SolicitudTransfusional s ON s.id_solicitud_transfusional=@id_solicitud
    WHERE s.id_usuario_validacion IS NOT NULL AND u.estado_operativo='LIBERADA'
      AND u.fecha_vencimiento>=CONVERT(DATE,GETDATE()) AND u.tipo_componente=s.tipo_componente
      AND dbo.fn_PN2_GrupoCandidato(u.tipo_componente,u.grupo_sanguineo,u.factor_rh,s.grupo_receptor,s.rh_receptor)=1
      AND NOT EXISTS(SELECT 1 FROM dbo.Reserva r WHERE r.id_unidad=u.id_unidad AND r.estado_reserva='ACTIVA')
    ORDER BY CASE WHEN u.grupo_sanguineo=s.grupo_receptor AND u.factor_rh=s.rh_receptor THEN 0 ELSE 1 END,
             u.fecha_vencimiento,u.id_unidad;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_PN2_Ubicacion_Actualizar
    @id_unidad INT,@ubicacion NVARCHAR(150),@observacion NVARCHAR(500),@id_usuario INT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        EXEC dbo.sp_IntegridadPN1_Autorizar @id_usuario,'INVENTARIO_ACTUALIZAR';
        EXEC dbo.sp_PN2_Exigir;
        IF @id_unidad IS NULL OR @id_unidad<=0 OR NULLIF(LTRIM(RTRIM(@ubicacion)),'') IS NULL
           OR NULLIF(LTRIM(RTRIM(@observacion)),'') IS NULL
        BEGIN
            ROLLBACK; SELECT 'DATOS_INVALIDOS' codigo_resultado,CAST(NULL AS INT) id_resultado; RETURN;
        END;
        DECLARE @estado VARCHAR(20),@ubicacion_anterior NVARCHAR(150);
        SELECT @estado=estado_operativo,@ubicacion_anterior=ubicacion FROM dbo.Unidad WHERE id_unidad=@id_unidad;
        IF @estado IS NULL
        BEGIN ROLLBACK; SELECT 'NO_ENCONTRADO' codigo_resultado,@id_unidad id_resultado; RETURN; END;
        IF @estado IN ('ENTREGADA','TRANSFUNDIDA','DESCARTADA')
        BEGIN ROLLBACK; SELECT 'CONFLICTO_ESTADO' codigo_resultado,@id_unidad id_resultado; RETURN; END;
        UPDATE dbo.Unidad SET ubicacion=LTRIM(RTRIM(@ubicacion)),fecha_ultima_modificacion=GETDATE(),id_usuario_ultima_modificacion=@id_usuario WHERE id_unidad=@id_unidad;
        INSERT dbo.MovimientoUnidad(id_unidad,tipo_movimiento,estado_anterior,estado_nuevo,observacion,id_usuario)
        VALUES(@id_unidad,'UBICACION',@estado,@estado,LEFT(CONCAT(COALESCE(@ubicacion_anterior,N'(sin ubicación)'),N' → ',LTRIM(RTRIM(@ubicacion)),N'. ',@observacion),500),@id_usuario);
        EXEC dbo.sp_IntegridadPN1_Sellar @id_usuario;
        COMMIT; SELECT 'OK' codigo_resultado,@id_unidad id_resultado;
    END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Reserva_Registrar
    @id_solicitud_transfusional INT,@id_unidad INT,@observacion NVARCHAR(500)=NULL,@id_usuario INT,
    @referencia_compatibilidad NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        EXEC dbo.sp_IntegridadPN1_Autorizar @id_usuario,'RESERVA_CREAR';
        EXEC dbo.sp_PN2_Exigir;
        IF @id_solicitud_transfusional IS NULL OR @id_solicitud_transfusional<=0 OR @id_unidad IS NULL OR @id_unidad<=0
           OR NULLIF(LTRIM(RTRIM(@referencia_compatibilidad)),'') IS NULL
        BEGIN ROLLBACK; SELECT 'DATOS_INVALIDOS' codigo_resultado,CAST(NULL AS INT) id_resultado; RETURN; END;
        IF NOT EXISTS(SELECT 1 FROM dbo.SolicitudTransfusional WHERE id_solicitud_transfusional=@id_solicitud_transfusional)
           OR NOT EXISTS(SELECT 1 FROM dbo.Unidad WHERE id_unidad=@id_unidad)
        BEGIN ROLLBACK; SELECT 'NO_ENCONTRADO' codigo_resultado,CAST(NULL AS INT) id_resultado; RETURN; END;
        IF EXISTS(SELECT 1 FROM dbo.v_PN2_SolicitudCantidades WHERE id_solicitud_transfusional=@id_solicitud_transfusional AND saldo_pendiente=0)
        BEGIN ROLLBACK; SELECT 'CANTIDAD_COMPLETA' codigo_resultado,CAST(NULL AS INT) id_resultado; RETURN; END;
        IF NOT EXISTS(
            SELECT 1 FROM dbo.Unidad u JOIN dbo.SolicitudTransfusional s ON s.id_solicitud_transfusional=@id_solicitud_transfusional
            WHERE u.id_unidad=@id_unidad AND u.estado_operativo='LIBERADA' AND u.fecha_vencimiento>=CONVERT(DATE,GETDATE())
              AND s.id_usuario_validacion IS NOT NULL
              AND dbo.fn_PN2_GrupoCandidato(u.tipo_componente,u.grupo_sanguineo,u.factor_rh,s.grupo_receptor,s.rh_receptor)=1
              AND u.tipo_componente=s.tipo_componente AND NULLIF(LTRIM(RTRIM(u.grupo_sanguineo)),'') IS NOT NULL
              AND NULLIF(LTRIM(RTRIM(u.factor_rh)),'') IS NOT NULL
              AND NOT EXISTS(SELECT 1 FROM dbo.Reserva r WHERE r.id_unidad=u.id_unidad AND r.estado_reserva='ACTIVA')
        )
        BEGIN ROLLBACK; SELECT 'SIN_DISPONIBILIDAD' codigo_resultado,CAST(NULL AS INT) id_resultado; RETURN; END;
        INSERT dbo.Reserva(id_solicitud_transfusional,id_unidad,observacion,id_usuario_responsable)
        VALUES(@id_solicitud_transfusional,@id_unidad,CONCAT(N'Compatibilidad confirmada: ',LTRIM(RTRIM(@referencia_compatibilidad))),@id_usuario);
        DECLARE @id INT=CONVERT(INT,SCOPE_IDENTITY());
        UPDATE dbo.Unidad SET estado_operativo='RESERVADA',fecha_ultima_modificacion=GETDATE(),id_usuario_ultima_modificacion=@id_usuario WHERE id_unidad=@id_unidad;
        INSERT dbo.MovimientoUnidad(id_unidad,tipo_movimiento,estado_anterior,estado_nuevo,observacion,id_usuario,id_solicitud_transfusional,id_reserva)
        VALUES(@id_unidad,'RESERVA','LIBERADA','RESERVADA',@observacion,@id_usuario,@id_solicitud_transfusional,@id);
        EXEC dbo.sp_PN2_Solicitud_ActualizarEstado @id_solicitud_transfusional;
        EXEC dbo.sp_IntegridadPN1_Sellar @id_usuario;
        COMMIT; SELECT 'OK' codigo_resultado,@id id_resultado;
    END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Reserva_Cancelar @id_reserva INT,@motivo NVARCHAR(500),@id_usuario INT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        EXEC dbo.sp_IntegridadPN1_Autorizar @id_usuario,'RESERVA_CANCELAR';
        EXEC dbo.sp_PN2_Exigir;
        IF @id_reserva IS NULL OR @id_reserva<=0 OR NULLIF(LTRIM(RTRIM(@motivo)),'') IS NULL
        BEGIN ROLLBACK; SELECT 'DATOS_INVALIDOS' codigo_resultado,CAST(NULL AS INT) id_resultado; RETURN; END;
        DECLARE @id_unidad INT,@id_solicitud INT,@estado VARCHAR(20);
        SELECT @id_unidad=r.id_unidad,@id_solicitud=r.id_solicitud_transfusional,@estado=r.estado_reserva FROM dbo.Reserva r WHERE r.id_reserva=@id_reserva;
        IF @id_unidad IS NULL
        BEGIN ROLLBACK; SELECT 'NO_ENCONTRADO' codigo_resultado,@id_reserva id_resultado; RETURN; END;
        IF @estado<>'ACTIVA' OR NOT EXISTS(SELECT 1 FROM dbo.Unidad WHERE id_unidad=@id_unidad AND estado_operativo='RESERVADA')
           OR EXISTS(SELECT 1 FROM dbo.Entrega WHERE id_reserva=@id_reserva)
        BEGIN ROLLBACK; SELECT 'CONFLICTO_ESTADO' codigo_resultado,@id_reserva id_resultado; RETURN; END;
        UPDATE dbo.Reserva SET estado_reserva='CANCELADA',fecha_cierre=GETDATE(),motivo_cancelacion=LTRIM(RTRIM(@motivo)) WHERE id_reserva=@id_reserva;
        UPDATE dbo.Unidad SET estado_operativo='LIBERADA',fecha_ultima_modificacion=GETDATE(),id_usuario_ultima_modificacion=@id_usuario WHERE id_unidad=@id_unidad;
        INSERT dbo.MovimientoUnidad(id_unidad,tipo_movimiento,estado_anterior,estado_nuevo,observacion,id_usuario,id_solicitud_transfusional,id_reserva)
        VALUES(@id_unidad,'CANCELACION','RESERVADA','LIBERADA',@motivo,@id_usuario,@id_solicitud,@id_reserva);
        EXEC dbo.sp_PN2_Solicitud_ActualizarEstado @id_solicitud;
        EXEC dbo.sp_IntegridadPN1_Sellar @id_usuario;
        COMMIT; SELECT 'OK' codigo_resultado,@id_reserva id_resultado;
    END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Entrega_Registrar @id_reserva INT,@observacion NVARCHAR(500)=NULL,@id_usuario INT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        EXEC dbo.sp_IntegridadPN1_Autorizar @id_usuario,'ENTREGA_REGISTRAR';
        EXEC dbo.sp_PN2_Exigir;
        IF @id_reserva IS NULL OR @id_reserva<=0
        BEGIN ROLLBACK; SELECT 'DATOS_INVALIDOS' codigo_resultado,CAST(NULL AS INT) id_resultado; RETURN; END;
        DECLARE @id_unidad INT,@id_solicitud INT,@estado VARCHAR(20);
        SELECT @id_unidad=r.id_unidad,@id_solicitud=r.id_solicitud_transfusional,@estado=r.estado_reserva FROM dbo.Reserva r WHERE r.id_reserva=@id_reserva;
        IF @id_unidad IS NULL
        BEGIN ROLLBACK; SELECT 'NO_ENCONTRADO' codigo_resultado,CAST(NULL AS INT) id_resultado; RETURN; END;
        IF @estado<>'ACTIVA' OR EXISTS(SELECT 1 FROM dbo.Entrega WHERE id_reserva=@id_reserva)
        BEGIN ROLLBACK; SELECT 'CONFLICTO_ESTADO' codigo_resultado,CAST(NULL AS INT) id_resultado; RETURN; END;
        IF NOT EXISTS(
            SELECT 1 FROM dbo.Unidad u JOIN dbo.SolicitudTransfusional s ON s.id_solicitud_transfusional=@id_solicitud
            WHERE u.id_unidad=@id_unidad AND u.estado_operativo='RESERVADA' AND u.fecha_vencimiento>=CONVERT(DATE,GETDATE())
              AND s.id_usuario_validacion IS NOT NULL
              AND EXISTS(SELECT 1 FROM dbo.Reserva rr WHERE rr.id_reserva=@id_reserva AND rr.observacion LIKE N'Compatibilidad confirmada: _%')
              AND dbo.fn_PN2_GrupoCandidato(u.tipo_componente,u.grupo_sanguineo,u.factor_rh,s.grupo_receptor,s.rh_receptor)=1
              AND u.tipo_componente=s.tipo_componente AND u.grupo_sanguineo IS NOT NULL AND u.factor_rh IS NOT NULL)
        BEGIN ROLLBACK; SELECT 'SIN_DISPONIBILIDAD' codigo_resultado,CAST(NULL AS INT) id_resultado; RETURN; END;
        INSERT dbo.Entrega(id_reserva,observacion,id_usuario_responsable) VALUES(@id_reserva,NULLIF(LTRIM(RTRIM(@observacion)),''),@id_usuario);
        DECLARE @id INT=CONVERT(INT,SCOPE_IDENTITY());
        UPDATE dbo.Reserva SET estado_reserva='ENTREGADA',fecha_cierre=GETDATE() WHERE id_reserva=@id_reserva;
        UPDATE dbo.Unidad SET estado_operativo='ENTREGADA',fecha_ultima_modificacion=GETDATE(),id_usuario_ultima_modificacion=@id_usuario WHERE id_unidad=@id_unidad;
        INSERT dbo.MovimientoUnidad(id_unidad,tipo_movimiento,estado_anterior,estado_nuevo,observacion,id_usuario,id_solicitud_transfusional,id_reserva,id_entrega)
        VALUES(@id_unidad,'ENTREGA','RESERVADA','ENTREGADA',@observacion,@id_usuario,@id_solicitud,@id_reserva,@id);
        EXEC dbo.sp_PN2_Solicitud_ActualizarEstado @id_solicitud;
        EXEC dbo.sp_IntegridadPN1_Sellar @id_usuario;
        COMMIT; SELECT 'OK' codigo_resultado,@id id_resultado;
    END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Entrega_RegistrarDestino
    @id_entrega INT,@destino VARCHAR(20),@motivo NVARCHAR(500)=NULL,@id_usuario INT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        EXEC dbo.sp_IntegridadPN1_Autorizar @id_usuario,'DESTINO_REGISTRAR';
        EXEC dbo.sp_PN2_Exigir;
        IF @id_entrega IS NULL OR @id_entrega<=0 OR @destino IS NULL OR @destino NOT IN ('TRANSFUNDIDA','DEVUELTA','DESCARTADA')
           OR (@destino IN ('DEVUELTA','DESCARTADA') AND NULLIF(LTRIM(RTRIM(@motivo)),'') IS NULL)
        BEGIN ROLLBACK; SELECT 'DATOS_INVALIDOS' codigo_resultado,CAST(NULL AS INT) id_resultado; RETURN; END;
        DECLARE @id_unidad INT,@id_solicitud INT,@id_reserva INT,@destino_actual VARCHAR(20);
        SELECT @id_unidad=r.id_unidad,@id_solicitud=r.id_solicitud_transfusional,@id_reserva=r.id_reserva,@destino_actual=e.destino
        FROM dbo.Entrega e JOIN dbo.Reserva r ON r.id_reserva=e.id_reserva WHERE e.id_entrega=@id_entrega;
        IF @id_unidad IS NULL
        BEGIN ROLLBACK; SELECT 'NO_ENCONTRADO' codigo_resultado,@id_entrega id_resultado; RETURN; END;
        IF @destino_actual IS NOT NULL OR NOT EXISTS(SELECT 1 FROM dbo.Unidad WHERE id_unidad=@id_unidad AND estado_operativo='ENTREGADA')
        BEGIN ROLLBACK; SELECT 'CONFLICTO_ESTADO' codigo_resultado,@id_entrega id_resultado; RETURN; END;
        UPDATE dbo.Entrega SET destino=@destino,fecha_destino=GETDATE(),motivo_destino=NULLIF(LTRIM(RTRIM(@motivo)),''),id_usuario_destino=@id_usuario WHERE id_entrega=@id_entrega;
        UPDATE dbo.Unidad SET estado_operativo=@destino,fecha_ultima_modificacion=GETDATE(),id_usuario_ultima_modificacion=@id_usuario WHERE id_unidad=@id_unidad;
        INSERT dbo.MovimientoUnidad(id_unidad,tipo_movimiento,estado_anterior,estado_nuevo,observacion,id_usuario,id_solicitud_transfusional,id_reserva,id_entrega)
        VALUES(@id_unidad,CASE @destino WHEN 'TRANSFUNDIDA' THEN 'TRANSFUSION' WHEN 'DEVUELTA' THEN 'DEVOLUCION' ELSE 'DESCARTE' END,
            'ENTREGADA',@destino,@motivo,@id_usuario,@id_solicitud,@id_reserva,@id_entrega);
        EXEC dbo.sp_PN2_Solicitud_ActualizarEstado @id_solicitud;
        EXEC dbo.sp_IntegridadPN1_Sellar @id_usuario;
        COMMIT; SELECT 'OK' codigo_resultado,@id_entrega id_resultado;
    END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_PN2_Unidad_Reingresar @id_unidad INT,@observacion NVARCHAR(500),@id_usuario INT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        EXEC dbo.sp_IntegridadPN1_Autorizar @id_usuario,'UNIDAD_REINGRESAR';
        EXEC dbo.sp_PN2_Exigir;
        IF @id_unidad IS NULL OR @id_unidad<=0 OR NULLIF(LTRIM(RTRIM(@observacion)),'') IS NULL
        BEGIN ROLLBACK; SELECT 'DATOS_INVALIDOS' codigo_resultado,CAST(NULL AS INT) id_resultado; RETURN; END;
        IF NOT EXISTS(SELECT 1 FROM dbo.Unidad WHERE id_unidad=@id_unidad)
        BEGIN ROLLBACK; SELECT 'NO_ENCONTRADO' codigo_resultado,@id_unidad id_resultado; RETURN; END;
        DECLARE @id_entrega INT,@id_reserva INT,@id_solicitud INT;
        SELECT TOP(1) @id_entrega=e.id_entrega,@id_reserva=r.id_reserva,@id_solicitud=r.id_solicitud_transfusional
        FROM dbo.Entrega e JOIN dbo.Reserva r ON r.id_reserva=e.id_reserva
        WHERE r.id_unidad=@id_unidad ORDER BY e.id_entrega DESC;
        IF NOT EXISTS(SELECT 1 FROM dbo.Unidad WHERE id_unidad=@id_unidad AND estado_operativo='DEVUELTA')
           OR NOT EXISTS(SELECT 1 FROM dbo.Entrega WHERE id_entrega=@id_entrega AND destino='DEVUELTA')
           OR EXISTS(SELECT 1 FROM dbo.Reserva WHERE id_unidad=@id_unidad AND estado_reserva='ACTIVA')
        BEGIN ROLLBACK; SELECT 'CONFLICTO_ESTADO' codigo_resultado,@id_unidad id_resultado; RETURN; END;
        UPDATE dbo.Unidad SET estado_operativo='EN_REVISION',fecha_ultima_modificacion=GETDATE(),id_usuario_ultima_modificacion=@id_usuario WHERE id_unidad=@id_unidad;
        INSERT dbo.MovimientoUnidad(id_unidad,tipo_movimiento,estado_anterior,estado_nuevo,observacion,id_usuario,id_solicitud_transfusional,id_reserva,id_entrega)
        VALUES(@id_unidad,'REINGRESO','DEVUELTA','EN_REVISION',@observacion,@id_usuario,@id_solicitud,@id_reserva,@id_entrega);
        EXEC dbo.sp_IntegridadPN1_Sellar @id_usuario;
        COMMIT; SELECT 'OK' codigo_resultado,@id_unidad id_resultado;
    END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_PN2_Trazabilidad_Listar
    @id_unidad INT=NULL,@desde DATETIME=NULL,@hasta DATETIME=NULL,@estado VARCHAR(20)=NULL,
    @grupo_sanguineo VARCHAR(5)=NULL,@tipo_componente NVARCHAR(100)=NULL,@id_usuario INT
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.sp_PN2_Autorizar @id_usuario,'TRAZABILIDAD_VER';
    IF @desde>@hasta THROW 51011,'PN2_INVALID_FILTER',1;
    SELECT m.id_movimiento_unidad,m.id_unidad,u.codigo_identificacion,m.tipo_movimiento,m.estado_anterior,m.estado_nuevo,
        m.observacion,m.fecha_movimiento,m.id_usuario,a.nombre_usuario usuario,m.id_solicitud_transfusional,m.id_reserva,m.id_entrega,
        u.tipo_componente,u.grupo_sanguineo,u.factor_rh
    FROM dbo.MovimientoUnidad m JOIN dbo.Unidad u ON u.id_unidad=m.id_unidad JOIN dbo.Usuario a ON a.id_usuario=m.id_usuario
    WHERE (@id_unidad IS NULL OR m.id_unidad=@id_unidad)
      AND (@desde IS NULL OR m.fecha_movimiento>=@desde)
      AND (@hasta IS NULL OR m.fecha_movimiento<=@hasta)
      AND (@estado IS NULL OR m.estado_nuevo=@estado)
      AND (@grupo_sanguineo IS NULL OR u.grupo_sanguineo=@grupo_sanguineo)
      AND (@tipo_componente IS NULL OR u.tipo_componente=@tipo_componente)
    ORDER BY m.fecha_movimiento,m.id_movimiento_unidad;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_PN2_Reporte_Listar
    @id_unidad INT=NULL,@desde DATETIME=NULL,@hasta DATETIME=NULL,@estado VARCHAR(20)=NULL,
    @grupo_sanguineo VARCHAR(5)=NULL,@tipo_componente NVARCHAR(100)=NULL,@id_usuario INT
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.sp_PN2_Autorizar @id_usuario,'REPORTE_VER';
    IF @desde>@hasta THROW 51011,'PN2_INVALID_FILTER',1;
    -- El inventario representa el stock actual. El periodo se aplica a los
    -- movimientos y a la fecha de las solicitudes/reservas, nunca simula stock pasado.
    SELECT u.* INTO #unidades FROM dbo.Unidad u
    WHERE (@id_unidad IS NULL OR u.id_unidad=@id_unidad)
      AND (@estado IS NULL OR u.estado_operativo=@estado)
      AND (@grupo_sanguineo IS NULL OR u.grupo_sanguineo=@grupo_sanguineo)
      AND (@tipo_componente IS NULL OR u.tipo_componente=@tipo_componente);
    SELECT s.*,c.saldo_pendiente INTO #solicitudes
    FROM dbo.SolicitudTransfusional s JOIN dbo.v_PN2_SolicitudCantidades c ON c.id_solicitud_transfusional=s.id_solicitud_transfusional
    WHERE (@desde IS NULL OR s.fecha_solicitud>=@desde) AND (@hasta IS NULL OR s.fecha_solicitud<=@hasta)
      AND (@tipo_componente IS NULL OR s.tipo_componente=@tipo_componente)
      AND ((@id_unidad IS NULL AND @estado IS NULL AND @grupo_sanguineo IS NULL)
           OR EXISTS(SELECT 1 FROM dbo.Reserva r JOIN #unidades u ON u.id_unidad=r.id_unidad WHERE r.id_solicitud_transfusional=s.id_solicitud_transfusional));

    SELECT CAST('INVENTARIO' AS VARCHAR(30)) seccion,
        CONVERT(NVARCHAR(300),CONCAT(COALESCE(u.tipo_componente,N'(sin clasificar)'),N' / ',COALESCE(u.grupo_sanguineo,'?'),COALESCE(u.factor_rh,'?'),N' / ',u.estado_operativo)) concepto,
        COUNT(*) cantidad,CAST(N'Inventario actual; el período no reconstruye stock histórico.' AS NVARCHAR(500)) detalle
    FROM #unidades u GROUP BY u.tipo_componente,u.grupo_sanguineo,u.factor_rh,u.estado_operativo
    UNION ALL
    SELECT 'SOLICITUDES',CONVERT(NVARCHAR(300),estado_solicitud),COUNT(*),N'Solicitudes registradas en el período seleccionado.'
    FROM #solicitudes GROUP BY estado_solicitud
    UNION ALL
    SELECT 'SOLICITUDES',N'Saldo pendiente',COALESCE(SUM(saldo_pendiente),0),N'Unidades aún no cubiertas por reservas activas o entregas efectivas.' FROM #solicitudes
    UNION ALL
    SELECT 'RESERVAS',N'Activas',COUNT(*),N'Reservas vigentes registradas en el período seleccionado.'
    FROM dbo.Reserva r JOIN #unidades u ON u.id_unidad=r.id_unidad
    WHERE r.estado_reserva='ACTIVA' AND (@desde IS NULL OR r.fecha_reserva>=@desde) AND (@hasta IS NULL OR r.fecha_reserva<=@hasta)
    UNION ALL
    SELECT 'MOVIMIENTOS',CONVERT(NVARCHAR(300),m.tipo_movimiento),COUNT(*),N'Movimientos registrados en el período; filtro de estado según resultado del movimiento.'
    FROM dbo.MovimientoUnidad m JOIN dbo.Unidad u ON u.id_unidad=m.id_unidad
    WHERE (@id_unidad IS NULL OR m.id_unidad=@id_unidad) AND (@desde IS NULL OR m.fecha_movimiento>=@desde) AND (@hasta IS NULL OR m.fecha_movimiento<=@hasta)
      AND (@estado IS NULL OR m.estado_nuevo=@estado) AND (@grupo_sanguineo IS NULL OR u.grupo_sanguineo=@grupo_sanguineo)
      AND (@tipo_componente IS NULL OR u.tipo_componente=@tipo_componente)
    GROUP BY m.tipo_movimiento
    UNION ALL
    SELECT 'ALERTAS',N'Unidades vencidas',COUNT(*),N'Unidades vencidas cuyo recorrido aún no terminó en transfusión o descarte.'
    FROM #unidades WHERE fecha_vencimiento<CONVERT(DATE,GETDATE()) AND estado_operativo NOT IN ('DESCARTADA','TRANSFUNDIDA')
    UNION ALL
    SELECT 'ALERTAS',N'Reservas inhabilitadas',COUNT(*),N'Reservas activas sin estudio validado o referencia, con unidad vencida o sin coincidencia preliminar.'
    FROM dbo.Reserva r JOIN #unidades u ON u.id_unidad=r.id_unidad JOIN dbo.SolicitudTransfusional s ON s.id_solicitud_transfusional=r.id_solicitud_transfusional
    WHERE r.estado_reserva='ACTIVA' AND (u.estado_operativo<>'RESERVADA' OR u.fecha_vencimiento<CONVERT(DATE,GETDATE())
        OR s.id_usuario_validacion IS NULL OR r.observacion IS NULL OR r.observacion NOT LIKE N'Compatibilidad confirmada: _%'
        OR u.tipo_componente IS NULL OR u.tipo_componente<>s.tipo_componente
        OR dbo.fn_PN2_GrupoCandidato(u.tipo_componente,u.grupo_sanguineo,u.factor_rh,s.grupo_receptor,s.rh_receptor)=0)
    UNION ALL
    SELECT 'ALERTAS',N'Solicitudes sin disponibilidad',COUNT(*),N'Solicitudes con saldo sin estudio validado o sin unidades candidatas.'
    FROM #solicitudes s WHERE s.saldo_pendiente>0 AND NOT EXISTS(
        SELECT 1 FROM dbo.Unidad u WHERE u.tipo_componente=s.tipo_componente AND u.estado_operativo='LIBERADA'
          AND s.id_usuario_validacion IS NOT NULL AND u.fecha_vencimiento>=CONVERT(DATE,GETDATE())
          AND dbo.fn_PN2_GrupoCandidato(u.tipo_componente,u.grupo_sanguineo,u.factor_rh,s.grupo_receptor,s.rh_receptor)=1
          AND NOT EXISTS(SELECT 1 FROM dbo.Reserva r WHERE r.id_unidad=u.id_unidad AND r.estado_reserva='ACTIVA'))
    ORDER BY seccion,concepto;
END;
GO
