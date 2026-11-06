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
