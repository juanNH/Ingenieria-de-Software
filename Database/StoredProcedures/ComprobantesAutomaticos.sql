-- Instalar después de PN2 e IntegridadPN2. No modifica datos de negocio existentes.
USE [HemovidaGest];
GO
IF OBJECT_ID('dbo.v_IntegridadPN2_Instalada','V') IS NULL
    THROW 51002, 'COMPROBANTES_REQUIEREN_PN2', 1;
GO
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
IF OBJECT_ID('dbo.ComprobantePendiente','U') IS NULL
BEGIN
    CREATE TABLE dbo.ComprobantePendiente
    (
        id_comprobante BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ComprobantePendiente PRIMARY KEY,
        identificador UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_ComprobantePendiente_Identificador DEFAULT NEWID(),
        proceso VARCHAR(3) NOT NULL,
        id_referencia INT NOT NULL,
        id_usuario INT NOT NULL,
        datos XML NOT NULL,
        fecha_evento DATETIME NOT NULL CONSTRAINT DF_ComprobantePendiente_Fecha DEFAULT GETDATE(),
        fecha_generacion DATETIME NULL,
        ruta NVARCHAR(2048) NULL,
        CONSTRAINT UQ_ComprobantePendiente_Referencia UNIQUE(proceso,id_referencia),
        CONSTRAINT CK_ComprobantePendiente_Proceso CHECK(proceso IN ('PN1','PN2')),
        CONSTRAINT FK_ComprobantePendiente_Usuario FOREIGN KEY(id_usuario) REFERENCES dbo.Usuario(id_usuario)
    );
END;
GO
-- El movimiento se inserta dentro de la transacción de negocio. El archivo se
-- genera después del COMMIT; si falla, la instantánea queda pendiente en SQL.
CREATE OR ALTER TRIGGER dbo.tr_MovimientoUnidad_Comprobante ON dbo.MovimientoUnidad
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @movimiento INT, @unidad INT, @usuario INT, @anterior VARCHAR(20),
            @nuevo VARCHAR(20), @entrega INT, @donacion INT, @datos XML;
    DECLARE eventos CURSOR LOCAL FAST_FORWARD FOR
        SELECT id_movimiento_unidad,id_unidad,id_usuario,estado_anterior,estado_nuevo,id_entrega
        FROM inserted WHERE (estado_anterior='EN_REVISION' AND tipo_movimiento IN ('LIBERACION','BLOQUEO','DESCARTE') AND estado_nuevo IN ('LIBERADA','BLOQUEADA','DESCARTADA'))
           OR (estado_anterior='ENTREGADA' AND tipo_movimiento IN ('TRANSFUSION','DEVOLUCION','DESCARTE') AND estado_nuevo IN ('TRANSFUNDIDA','DEVUELTA','DESCARTADA') AND id_entrega IS NOT NULL);
    OPEN eventos;
    FETCH NEXT FROM eventos INTO @movimiento,@unidad,@usuario,@anterior,@nuevo,@entrega;
    WHILE @@FETCH_STATUS=0
    BEGIN
        IF @anterior='EN_REVISION'
        BEGIN
            SELECT @donacion=id_donacion FROM dbo.Unidad WHERE id_unidad=@unidad;
            IF NOT EXISTS(SELECT 1 FROM dbo.ComprobantePendiente WHERE proceso='PN1' AND id_referencia=@donacion)
               AND NOT EXISTS(
                   SELECT 1 FROM dbo.Unidad u WHERE u.id_donacion=@donacion AND NOT EXISTS(
                       SELECT 1 FROM dbo.MovimientoUnidad m WHERE m.id_unidad=u.id_unidad
                       AND m.estado_anterior='EN_REVISION' AND m.tipo_movimiento IN ('LIBERACION','BLOQUEO','DESCARTE') AND m.estado_nuevo IN ('LIBERADA','BLOQUEADA','DESCARTADA')))
            BEGIN
                SET @datos=(SELECT 'PN1' AS [Tipo], us.nombre_usuario AS [Usuario],
                    (SELECT u.id_unidad AS [Id_380_jh],u.codigo_identificacion AS [CodigoIdentificacion_380_jh],
                        u.id_donacion AS [IdDonacion_380_jh],d.id_donante AS [IdDonante_380_jh],
                        d.documento AS [DocumentoDonante_380_jh],d.nombre+' '+d.apellido AS [Donante_380_jh],
                        u.tipo_componente AS [TipoComponente_380_jh],u.fecha_vencimiento AS [FechaVencimiento_380_jh],
                        u.grupo_sanguineo AS [GrupoSanguineo_380_jh],u.factor_rh AS [FactorRh_380_jh],
                        decision.observacion AS [Observaciones_380_jh],decision.estado_nuevo AS [EstadoOperativo_380_jh],
                        u.fecha_alta AS [FechaAlta_380_jh],decision.fecha_movimiento AS [FechaUltimaModificacion_380_jh],
                        decision.id_usuario AS [IdUsuarioUltimaModificacion_380_jh]
                     FROM dbo.Unidad u JOIN dbo.Donacion dn ON dn.id_donacion=u.id_donacion
                     JOIN dbo.Donante d ON d.id_donante=dn.id_donante
                     CROSS APPLY (SELECT TOP(1) m.estado_nuevo,m.observacion,m.fecha_movimiento,m.id_usuario
                                  FROM dbo.MovimientoUnidad m WHERE m.id_unidad=u.id_unidad
                                  AND m.estado_anterior='EN_REVISION' AND m.tipo_movimiento IN ('LIBERACION','BLOQUEO','DESCARTE') AND m.estado_nuevo IN ('LIBERADA','BLOQUEADA','DESCARTADA')
                                  ORDER BY m.id_movimiento_unidad DESC) decision
                     WHERE u.id_donacion=@donacion ORDER BY u.id_unidad
                     FOR XML PATH('Unidad'),ROOT('Unidades'),TYPE)
                    FROM dbo.Usuario us WHERE us.id_usuario=@usuario FOR XML PATH('Comprobante'),TYPE);
                INSERT dbo.ComprobantePendiente(proceso,id_referencia,id_usuario,datos)
                VALUES('PN1',@donacion,@usuario,@datos);
            END;
        END
        ELSE IF NOT EXISTS(SELECT 1 FROM dbo.ComprobantePendiente WHERE proceso='PN2' AND id_referencia=@entrega)
        BEGIN
            SET @datos=(SELECT 'PN2' AS [Tipo], us.nombre_usuario AS [Usuario],
                (SELECT s.id_solicitud_transfusional AS [Id_380_jh],s.paciente_receptor AS [PacienteReceptor_380_jh],
                    s.fecha_solicitud AS [FechaSolicitud_380_jh],s.tipo_componente AS [TipoComponente_380_jh],
                    s.cantidad_requerida AS [CantidadRequerida_380_jh],s.id_usuario_solicitante AS [IdUsuarioSolicitante_380_jh],
                    profesional.nombre+' '+profesional.apellido AS [ProfesionalSolicitante_380_jh],
                    CASE WHEN c.cantidad_entregada>=s.cantidad_requerida THEN 'ENTREGADA'
                         WHEN c.cantidad_reservada+c.cantidad_entregada>=s.cantidad_requerida THEN 'RESERVADA'
                         WHEN c.cantidad_reservada+c.cantidad_entregada>0 THEN 'PARCIAL' ELSE 'PENDIENTE' END AS [EstadoSolicitud_380_jh],
                    c.cantidad_reservada AS [CantidadReservada_380_jh],c.cantidad_entregada AS [CantidadEntregada_380_jh],
                    c.saldo_pendiente AS [SaldoPendiente_380_jh]
                 FROM dbo.Entrega e JOIN dbo.Reserva r ON r.id_reserva=e.id_reserva
                 JOIN dbo.SolicitudTransfusional s ON s.id_solicitud_transfusional=r.id_solicitud_transfusional
                 JOIN dbo.v_PN2_SolicitudCantidades c ON c.id_solicitud_transfusional=s.id_solicitud_transfusional
                 JOIN dbo.Usuario profesional ON profesional.id_usuario=s.id_usuario_solicitante
                 WHERE e.id_entrega=@entrega FOR XML PATH('Solicitud'),TYPE),
                (SELECT e.id_entrega AS [Id_380_jh],r.id_reserva AS [IdReserva_380_jh],
                    r.id_solicitud_transfusional AS [IdSolicitudTransfusional_380_jh],r.id_unidad AS [IdUnidad_380_jh],
                    u.codigo_identificacion AS [CodigoIdentificacion_380_jh],s.paciente_receptor AS [PacienteReceptor_380_jh],
                    s.tipo_componente AS [TipoComponente_380_jh],e.fecha_entrega AS [FechaEntrega_380_jh],
                    e.id_usuario_responsable AS [IdUsuarioResponsable_380_jh],responsable.nombre+' '+responsable.apellido AS [Responsable_380_jh],
                    e.observacion AS [Observacion_380_jh],e.destino AS [Destino_380_jh],e.fecha_destino AS [FechaDestino_380_jh],
                    e.motivo_destino AS [MotivoDestino_380_jh],e.id_usuario_destino AS [IdUsuarioDestino_380_jh]
                 FROM dbo.Entrega e JOIN dbo.Reserva r ON r.id_reserva=e.id_reserva
                 JOIN dbo.SolicitudTransfusional s ON s.id_solicitud_transfusional=r.id_solicitud_transfusional
                 JOIN dbo.Unidad u ON u.id_unidad=r.id_unidad JOIN dbo.Usuario responsable ON responsable.id_usuario=e.id_usuario_responsable
                 WHERE e.id_entrega=@entrega FOR XML PATH('Entrega'),ROOT('Entregas'),TYPE)
                FROM dbo.Usuario us WHERE us.id_usuario=@usuario FOR XML PATH('Comprobante'),TYPE);
            INSERT dbo.ComprobantePendiente(proceso,id_referencia,id_usuario,datos)
            VALUES('PN2',@entrega,@usuario,@datos);
        END;
        FETCH NEXT FROM eventos INTO @movimiento,@unidad,@usuario,@anterior,@nuevo,@entrega;
    END;
    CLOSE eventos; DEALLOCATE eventos;
END;
GO
CREATE OR ALTER PROCEDURE dbo.sp_Comprobante_Pendientes @id_usuario INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS(SELECT 1 FROM dbo.Usuario WHERE id_usuario=@id_usuario AND estado_usuario='ACTIVO' AND bloqueo_digitoverificador=0)
        THROW 51003,'OPERATION_NOT_AUTHORIZED',1;
    SELECT id_comprobante,identificador,proceso,id_referencia,CONVERT(NVARCHAR(MAX),datos) AS datos
    FROM dbo.ComprobantePendiente WHERE fecha_generacion IS NULL AND
      (dbo.fn_PN2_TienePermiso(@id_usuario,'ADMINISTRADOR')=1 OR
       (id_usuario=@id_usuario AND ((proceso='PN1' AND (dbo.fn_PN2_TienePermiso(@id_usuario,'UNIDAD_LIBERAR')=1
           OR dbo.fn_PN2_TienePermiso(@id_usuario,'UNIDAD_BLOQUEAR')=1 OR dbo.fn_PN2_TienePermiso(@id_usuario,'UNIDAD_DESCARTAR')=1))
        OR (proceso='PN2' AND dbo.fn_PN2_TienePermiso(@id_usuario,'DESTINO_REGISTRAR')=1))))
    ORDER BY id_comprobante;
END;
GO
CREATE OR ALTER PROCEDURE dbo.sp_Comprobante_Estado
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN OBJECT_ID('dbo.ComprobantePendiente','U') IS NOT NULL
        AND EXISTS(SELECT 1 FROM sys.triggers WHERE object_id=OBJECT_ID('dbo.tr_MovimientoUnidad_Comprobante') AND is_disabled=0)
        AND OBJECT_ID('dbo.sp_Comprobante_MarcarGenerado','P') IS NOT NULL THEN 1 ELSE 0 END AS disponible;
END;
GO
CREATE OR ALTER PROCEDURE dbo.sp_Comprobante_MarcarGenerado @id_comprobante BIGINT,@id_usuario INT,@ruta NVARCHAR(2048)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS(SELECT 1 FROM dbo.Usuario WHERE id_usuario=@id_usuario AND estado_usuario='ACTIVO' AND bloqueo_digitoverificador=0)
        THROW 51003,'OPERATION_NOT_AUTHORIZED',1;
    UPDATE dbo.ComprobantePendiente SET fecha_generacion=GETDATE(),ruta=@ruta
    WHERE id_comprobante=@id_comprobante AND fecha_generacion IS NULL AND
      (dbo.fn_PN2_TienePermiso(@id_usuario,'ADMINISTRADOR')=1 OR
       (id_usuario=@id_usuario AND ((proceso='PN1' AND (dbo.fn_PN2_TienePermiso(@id_usuario,'UNIDAD_LIBERAR')=1
           OR dbo.fn_PN2_TienePermiso(@id_usuario,'UNIDAD_BLOQUEAR')=1 OR dbo.fn_PN2_TienePermiso(@id_usuario,'UNIDAD_DESCARTAR')=1))
        OR (proceso='PN2' AND dbo.fn_PN2_TienePermiso(@id_usuario,'DESTINO_REGISTRAR')=1))));
    IF @@ROWCOUNT=0 AND NOT EXISTS(SELECT 1 FROM dbo.ComprobantePendiente WHERE id_comprobante=@id_comprobante AND fecha_generacion IS NOT NULL)
        THROW 51003,'OPERATION_NOT_AUTHORIZED',1;
END;
GO
CREATE OR ALTER PROCEDURE dbo.sp_Comprobante_Original @proceso VARCHAR(3),@id_referencia INT,@id_usuario INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS(SELECT 1 FROM dbo.Usuario WHERE id_usuario=@id_usuario AND estado_usuario='ACTIVO' AND bloqueo_digitoverificador=0)
        THROW 51003,'OPERATION_NOT_AUTHORIZED',1;
    IF @proceso='PN1' AND dbo.fn_PN2_TienePermiso(@id_usuario,'UNIDAD_VER')=0
       AND dbo.fn_PN2_TienePermiso(@id_usuario,'UNIDAD_CLASIFICAR')=0
       AND dbo.fn_PN2_TienePermiso(@id_usuario,'UNIDAD_LIBERAR')=0
       AND dbo.fn_PN2_TienePermiso(@id_usuario,'UNIDAD_BLOQUEAR')=0
       AND dbo.fn_PN2_TienePermiso(@id_usuario,'UNIDAD_DESCARTAR')=0
        THROW 51003,'OPERATION_NOT_AUTHORIZED',1;
    IF @proceso='PN2' AND (dbo.fn_PN2_TienePermiso(@id_usuario,'SOLICITUD_VER')=0
        OR dbo.fn_PN2_TienePermiso(@id_usuario,'RESERVA_VER')=0)
        THROW 51003,'OPERATION_NOT_AUTHORIZED',1;
    IF @proceso NOT IN ('PN1','PN2') THROW 51003,'OPERATION_NOT_AUTHORIZED',1;
    SELECT ruta FROM dbo.ComprobantePendiente
    WHERE proceso=@proceso AND id_referencia=@id_referencia AND fecha_generacion IS NOT NULL;
END;
GO
