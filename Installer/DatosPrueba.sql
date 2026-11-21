-- El instalador ejecuta esta muestra solo cuando se marca la opción de demostración.
DECLARE @id_usuario INT = (
    SELECT id_usuario FROM dbo.Usuario WHERE nombre_usuario = 'admin' AND estado_usuario = 'ACTIVO'
);
DECLARE @componente VARCHAR(100) = 'Concentrado de eritrocitos';
DECLARE @fecha DATETIME = GETDATE();
DECLARE @vencimiento DATE = DATEADD(DAY, 30, @fecha);
DECLARE @fecha_estudio DATE = CONVERT(DATE, @fecha);

IF @id_usuario IS NULL
    THROW 51000, 'No se encontró el usuario inicial para cargar la demostración.', 1;

EXEC dbo.sp_Donante_Registrar
    @documento = 'DEMO-DONANTE-001', @nombre = 'Donante', @apellido = 'Demostración',
    @id_usuario = @id_usuario;

DECLARE @id_donante INT = (
    SELECT id_donante FROM dbo.Donante WHERE documento = 'DEMO-DONANTE-001'
);
IF @id_donante IS NULL
    THROW 51000, 'No se pudo registrar el donante ficticio.', 1;

EXEC dbo.sp_Donacion_Registrar
    @id_donante = @id_donante, @fecha_donacion = @fecha, @cantidad_unidades = 1,
    @tipo_componente = @componente, @fecha_vencimiento = @vencimiento,
    @observaciones = 'Registro ficticio de demostración.', @id_usuario = @id_usuario;

DECLARE @id_unidad INT = (
    SELECT u.id_unidad FROM dbo.Unidad u
    INNER JOIN dbo.Donacion d ON d.id_donacion = u.id_donacion
    WHERE d.id_donante = @id_donante
);
IF @id_unidad IS NULL
    THROW 51000, 'No se pudo generar la unidad ficticia.', 1;

EXEC dbo.sp_Unidad_Clasificar
    @id_unidad = @id_unidad, @tipo_componente = @componente,
    @grupo_sanguineo = 'O', @factor_rh = '+', @fecha_vencimiento = @vencimiento,
    @id_usuario = @id_usuario;
EXEC dbo.sp_Unidad_CambiarEstado
    @id_unidad = @id_unidad, @estado_nuevo = 'LIBERADA', @id_usuario = @id_usuario;

IF NOT EXISTS (
    SELECT 1 FROM dbo.Unidad
    WHERE id_unidad = @id_unidad AND estado_operativo = 'LIBERADA'
      AND grupo_sanguineo = 'O' AND factor_rh = '+'
)
    THROW 51000, 'No se pudo dejar disponible la unidad ficticia.', 1;

EXEC dbo.sp_SolicitudTransfusional_Registrar
    @paciente_receptor = N'Receptor de demostración', @tipo_componente = @componente,
    @cantidad_requerida = 1, @id_usuario = @id_usuario, @documento_receptor = N'DEMO-RECEPTOR-001',
    @grupo_receptor = 'O', @rh_receptor = '+', @laboratorio_estudio = N'Laboratorio de demostración',
    @referencia_estudio = N'DEMO-ESTUDIO-001', @fecha_estudio = @fecha_estudio,
    @resultado_anticuerpos = N'No detectados', @nombre_solicitante = N'Profesional de demostración';

IF NOT EXISTS (
    SELECT 1 FROM dbo.SolicitudTransfusional
    WHERE documento_receptor = N'DEMO-RECEPTOR-001' AND id_usuario_validacion IS NULL
)
    THROW 51000, 'No se pudo registrar la solicitud transfusional ficticia.', 1;

-- Ejemplo de abastecimiento A+: tres unidades disponibles, una reservada y una entregada.
DECLARE @observacion_demo VARCHAR(500) = 'Registro ficticio de demostración del reporte de abastecimiento.';

EXEC dbo.sp_Donante_Registrar
    @documento = 'DEMO-DONANTE-002', @nombre = 'Donante A+', @apellido = 'Demostración',
    @id_usuario = @id_usuario;

DECLARE @id_donante_a INT = (
    SELECT id_donante FROM dbo.Donante WHERE documento = 'DEMO-DONANTE-002'
);
IF @id_donante_a IS NULL
    THROW 51000, 'No se pudo registrar el donante ficticio A+.', 1;

EXEC dbo.sp_Donacion_Registrar
    @id_donante = @id_donante_a, @fecha_donacion = @fecha, @cantidad_unidades = 5,
    @tipo_componente = @componente, @fecha_vencimiento = @vencimiento,
    @observaciones = @observacion_demo, @id_usuario = @id_usuario;

DECLARE @unidades_a TABLE (orden INT PRIMARY KEY, id_unidad INT NOT NULL);
INSERT INTO @unidades_a (orden, id_unidad)
SELECT ROW_NUMBER() OVER (ORDER BY u.id_unidad), u.id_unidad
FROM dbo.Unidad u
INNER JOIN dbo.Donacion d ON d.id_donacion = u.id_donacion
WHERE d.id_donante = @id_donante_a;

IF (SELECT COUNT(*) FROM @unidades_a) <> 5
    THROW 51000, 'No se pudieron generar las cinco unidades ficticias A+.', 1;

DECLARE @orden INT = 1, @vencimiento_unidad DATE;
WHILE @orden <= 5
BEGIN
    SELECT @id_unidad = id_unidad FROM @unidades_a WHERE orden = @orden;
    SET @vencimiento_unidad = DATEADD(DAY, CASE @orden WHEN 1 THEN 2 WHEN 2 THEN 5 ELSE 30 END, @fecha_estudio);

    EXEC dbo.sp_Unidad_Clasificar
        @id_unidad = @id_unidad, @tipo_componente = @componente,
        @grupo_sanguineo = 'A', @factor_rh = '+', @fecha_vencimiento = @vencimiento_unidad,
        @observaciones = @observacion_demo, @id_usuario = @id_usuario;
    EXEC dbo.sp_Unidad_CambiarEstado
        @id_unidad = @id_unidad, @estado_nuevo = 'LIBERADA', @id_usuario = @id_usuario;

    SET @orden += 1;
END;

EXEC dbo.sp_SolicitudTransfusional_Registrar
    @paciente_receptor = N'Receptor A+ de demostración', @tipo_componente = @componente,
    @cantidad_requerida = 10, @id_usuario = @id_usuario, @documento_receptor = N'DEMO-RECEPTOR-002',
    @grupo_receptor = 'A', @rh_receptor = '+', @laboratorio_estudio = N'Laboratorio de demostración',
    @referencia_estudio = N'DEMO-ESTUDIO-002', @fecha_estudio = @fecha_estudio,
    @resultado_anticuerpos = N'No detectados', @nombre_solicitante = N'Profesional de demostración';

DECLARE @id_solicitud_a INT = (
    SELECT id_solicitud_transfusional FROM dbo.SolicitudTransfusional
    WHERE documento_receptor = N'DEMO-RECEPTOR-002'
);
IF @id_solicitud_a IS NULL
    THROW 51000, 'No se pudo registrar la solicitud ficticia A+.', 1;

EXEC dbo.sp_PN2_ValidarEstudio @id_solicitud = @id_solicitud_a, @id_usuario = @id_usuario;

-- Reservar las dos últimas unidades y entregar solamente la última.
SET @orden = 4;
WHILE @orden <= 5
BEGIN
    SELECT @id_unidad = id_unidad FROM @unidades_a WHERE orden = @orden;
    EXEC dbo.sp_Reserva_Registrar
        @id_solicitud_transfusional = @id_solicitud_a, @id_unidad = @id_unidad,
        @observacion = @observacion_demo, @id_usuario = @id_usuario,
        @referencia_compatibilidad = N'DEMO-COMPATIBILIDAD-002';
    SET @orden += 1;
END;

DECLARE @id_reserva_a INT = (
    SELECT id_reserva FROM dbo.Reserva
    WHERE id_solicitud_transfusional = @id_solicitud_a AND id_unidad = @id_unidad
      AND estado_reserva = 'ACTIVA'
);
IF @id_reserva_a IS NULL
    THROW 51000, 'No se pudo reservar la unidad ficticia A+ para entregar.', 1;

EXEC dbo.sp_Entrega_Registrar
    @id_reserva = @id_reserva_a, @observacion = @observacion_demo, @id_usuario = @id_usuario;

IF NOT EXISTS (
    SELECT 1 FROM dbo.v_PN2_SolicitudCantidades
    WHERE id_solicitud_transfusional = @id_solicitud_a
      AND cantidad_reservada = 1 AND cantidad_entregada = 1 AND saldo_pendiente = 8
)
    THROW 51000, 'No se pudo completar el ejemplo de abastecimiento A+.', 1;
