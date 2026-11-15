-- Reporte operativo por cohortes exactas. No asigna unidades ni calcula compatibilidad.
USE [HemovidaGest];
GO
CREATE OR ALTER PROCEDURE dbo.sp_PN2_Abastecimiento_Listar
    @desde DATE, @hasta DATE, @horizonte_dias INT,
    @id_usuario INT, @tipo_componente NVARCHAR(100)=NULL,
    @grupo_sanguineo VARCHAR(5)=NULL, @factor_rh VARCHAR(5)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    EXEC dbo.sp_PN2_Autorizar @id_usuario,'REPORTE_VER';
    DECLARE @hoy DATE=CONVERT(DATE,GETDATE());
    IF @desde IS NULL OR @hasta IS NULL OR @desde<'17530101' OR @desde>@hasta OR @hasta>@hoy
       OR @horizonte_dias IS NULL OR @horizonte_dias<1 OR @horizonte_dias>365
        THROW 51011,'PN2_INVALID_FILTER',1;
    BEGIN TRY
    BEGIN TRANSACTION;
    EXEC dbo.sp_PN2_Exigir;
    DECLARE @dias INT=DATEDIFF(DAY,@desde,@hasta)+1;
    SET @tipo_componente=NULLIF(LTRIM(RTRIM(@tipo_componente)),N'');
    SET @grupo_sanguineo=NULLIF(LTRIM(RTRIM(@grupo_sanguineo)),'');
    SET @factor_rh=NULLIF(LTRIM(RTRIM(@factor_rh)),'');

    ;WITH Stock AS
    (
        SELECT u.tipo_componente,u.grupo_sanguineo,u.factor_rh,
            COUNT_BIG(*) stock_liberado_vigente,
            SUM(CONVERT(BIGINT,CASE WHEN u.fecha_vencimiento<=DATEADD(DAY,@horizonte_dias,@hoy) THEN 1 ELSE 0 END)) stock_por_vencer
        FROM dbo.Unidad u
        WHERE u.estado_operativo='LIBERADA' AND u.fecha_vencimiento>=@hoy
          AND u.tipo_componente IS NOT NULL AND u.grupo_sanguineo IS NOT NULL AND u.factor_rh IS NOT NULL
          AND NOT EXISTS(SELECT 1 FROM dbo.Reserva r WHERE r.id_unidad=u.id_unidad AND r.estado_reserva='ACTIVA')
        GROUP BY u.tipo_componente,u.grupo_sanguineo,u.factor_rh
    ), SolicitudesBase AS
    (
        -- Cada solicitud aparece una vez. La vista agrega reservas y entregas por separado.
        SELECT s.tipo_componente,s.fecha_solicitud,CONVERT(BIGINT,s.cantidad_requerida) cantidad,
            CONVERT(BIGINT,c.saldo_pendiente) pendiente,
            CASE WHEN s.id_usuario_validacion IS NOT NULL AND s.fecha_validacion IS NOT NULL
                       AND s.grupo_receptor IS NOT NULL AND s.rh_receptor IS NOT NULL THEN s.grupo_receptor END grupo_sanguineo,
            CASE WHEN s.id_usuario_validacion IS NOT NULL AND s.fecha_validacion IS NOT NULL
                       AND s.grupo_receptor IS NOT NULL AND s.rh_receptor IS NOT NULL THEN s.rh_receptor END factor_rh
        FROM dbo.SolicitudTransfusional s
        JOIN dbo.v_PN2_SolicitudCantidades c ON c.id_solicitud_transfusional=s.id_solicitud_transfusional
        WHERE s.fecha_solicitud<DATEADD(DAY,1,@hoy)
    ), Solicitudes AS
    (
        SELECT tipo_componente,grupo_sanguineo,factor_rh,
            SUM(CASE WHEN fecha_solicitud>=@desde AND fecha_solicitud<DATEADD(DAY,1,@hasta) THEN cantidad ELSE CONVERT(BIGINT,0) END) demanda_periodo,
            SUM(pendiente) pendiente_vigente
        FROM SolicitudesBase
        GROUP BY tipo_componente,grupo_sanguineo,factor_rh
        HAVING SUM(pendiente)>0 OR SUM(CASE WHEN fecha_solicitud>=@desde AND fecha_solicitud<DATEADD(DAY,1,@hasta) THEN cantidad ELSE CONVERT(BIGINT,0) END)>0
    ), Cohortes AS
    (
        SELECT tipo_componente,grupo_sanguineo,factor_rh FROM Stock
        UNION
        SELECT tipo_componente,grupo_sanguineo,factor_rh FROM Solicitudes
    ), Metricas AS
    (
        SELECT c.tipo_componente,c.grupo_sanguineo,c.factor_rh,
            CONVERT(BIT,CASE WHEN c.grupo_sanguineo IS NOT NULL AND c.factor_rh IS NOT NULL THEN 1 ELSE 0 END) cohorte_validada,
            CASE WHEN c.grupo_sanguineo IS NOT NULL THEN COALESCE(st.stock_liberado_vigente,0) END stock_liberado_vigente,
            CASE WHEN c.grupo_sanguineo IS NOT NULL THEN COALESCE(st.stock_por_vencer,0) END stock_por_vencer,
            COALESCE(s.demanda_periodo,0) demanda_periodo,COALESCE(s.pendiente_vigente,0) pendiente_vigente
        FROM Cohortes c
        LEFT JOIN Stock st ON st.tipo_componente=c.tipo_componente AND st.grupo_sanguineo=c.grupo_sanguineo AND st.factor_rh=c.factor_rh
        LEFT JOIN Solicitudes s ON s.tipo_componente=c.tipo_componente
          AND (s.grupo_sanguineo=c.grupo_sanguineo OR (s.grupo_sanguineo IS NULL AND c.grupo_sanguineo IS NULL))
          AND (s.factor_rh=c.factor_rh OR (s.factor_rh IS NULL AND c.factor_rh IS NULL))
        WHERE (@tipo_componente IS NULL OR c.tipo_componente=@tipo_componente)
          AND (@grupo_sanguineo IS NULL OR c.grupo_sanguineo=@grupo_sanguineo)
          AND (@factor_rh IS NULL OR c.factor_rh=@factor_rh)
    ), Resultado AS
    (
        SELECT *,
            CONVERT(DECIMAL(28,6),CONVERT(DECIMAL(28,6),demanda_periodo)/@dias) demanda_diaria_promedio,
            CASE WHEN cohorte_validada=1 AND demanda_periodo>0
                 THEN CONVERT(DECIMAL(28,4),CONVERT(DECIMAL(28,6),stock_liberado_vigente)*@dias/demanda_periodo) END cobertura_dias,
            CASE WHEN cohorte_validada=1 THEN CASE WHEN pendiente_vigente>stock_liberado_vigente
                 THEN pendiente_vigente-stock_liberado_vigente ELSE CONVERT(BIGINT,0) END END deficit_unidades
        FROM Metricas
    )
    SELECT tipo_componente,grupo_sanguineo,factor_rh,cohorte_validada,
        stock_liberado_vigente,stock_por_vencer,demanda_periodo,pendiente_vigente,
        demanda_diaria_promedio,cobertura_dias,deficit_unidades,
        CONVERT(VARCHAR(40),CASE WHEN cohorte_validada=0 THEN 'VALIDAR_ESTUDIO'
             WHEN deficit_unidades>0 AND stock_por_vencer>0 THEN 'REPOSICION_Y_VENCIMIENTO'
             WHEN deficit_unidades>0 THEN 'REPOSICION'
             WHEN stock_por_vencer>0 THEN 'VENCIMIENTO' ELSE 'SIN_ALERTA' END) prioridad,
        @hoy fecha_corte
    FROM Resultado
    ORDER BY deficit_unidades DESC,stock_por_vencer DESC,tipo_componente,grupo_sanguineo,factor_rh;
    COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO
