-- ============================================================
-- Base de Datos: Tecni Salud
-- Script de creacion de tablas
-- Motor sugerido: SQL Server
-- ============================================================

-- Crear base de datos opcional
CREATE DATABASE TecniSalud;
 GO
 USE TecniSalud;
 GO

-- ============================================================
-- TABLAS DE SEGURIDAD, ROLES, PERMISOS E IDIOMA
-- ============================================================

CREATE TABLE Idioma (
    id_idioma INT IDENTITY(1,1) PRIMARY KEY,
    codigo VARCHAR(10) NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    estado_idioma VARCHAR(20) NOT NULL DEFAULT 'ACTIVO',
    CONSTRAINT UQ_Idioma_Codigo UNIQUE (codigo),
    CONSTRAINT CK_Idioma_Estado CHECK (estado_idioma IN ('ACTIVO', 'INACTIVO'))
);
GO

CREATE TABLE IdiomaEstadoHistorial (
    id_idioma_estado_historial INT IDENTITY(1,1) PRIMARY KEY,
    id_idioma INT NOT NULL,
    estado_anterior VARCHAR(20) NULL,
    estado_nuevo VARCHAR(20) NOT NULL,
    motivo VARCHAR(255) NULL,
    fecha_cambio DATETIME NOT NULL DEFAULT GETDATE(),
    id_usuario_responsable INT NULL,
    CONSTRAINT FK_IdiomaEstadoHistorial_Idioma FOREIGN KEY (id_idioma) REFERENCES Idioma(id_idioma)
);
GO

CREATE TABLE Usuario (
    id_usuario INT IDENTITY(1,1) PRIMARY KEY,
    id_idioma INT NULL,
    nombre_usuario VARCHAR(100) NOT NULL,
    email VARCHAR(150) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    nombre VARCHAR(100) NULL,
    apellido VARCHAR(100) NULL,
    estado_usuario VARCHAR(20) NOT NULL DEFAULT 'ACTIVO',
    fecha_alta DATETIME NOT NULL DEFAULT GETDATE(),
    bloqueo_digitoverificador BIT NOT NULL DEFAULT 0,
    dvh VARCHAR(64) NULL,
    CONSTRAINT UQ_Usuario_Nombre UNIQUE (nombre_usuario),
    CONSTRAINT UQ_Usuario_Email UNIQUE (email),
    CONSTRAINT CK_Usuario_Estado CHECK (estado_usuario IN ('ACTIVO', 'INACTIVO', 'BLOQUEADO')),
    CONSTRAINT FK_Usuario_Idioma FOREIGN KEY (id_idioma) REFERENCES Idioma(id_idioma)
);
GO

CREATE TABLE DigitoVerificadorVertical (
    id_digito_verificador_vertical INT IDENTITY(1,1) PRIMARY KEY,
    entidad VARCHAR(100) NOT NULL,
    dvv VARCHAR(64) NOT NULL,
    fecha_calculo DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_DigitoVerificadorVertical_Entidad UNIQUE (entidad)
);
GO

CREATE TABLE Auditoria (
    id_auditoria INT IDENTITY(1,1) PRIMARY KEY,
    entidad NVARCHAR(100) NOT NULL,
    id_entidad INT NOT NULL,
    accion NVARCHAR(50) NOT NULL,
    id_usuario_actor INT NULL,
    identificador_usuario_actor NVARCHAR(255) NULL,
    fecha_evento DATETIME NOT NULL DEFAULT GETDATE(),
    estado_anterior_json NVARCHAR(MAX) NULL,
    estado_nuevo_json NVARCHAR(MAX) NULL,
    cambios_json NVARCHAR(MAX) NOT NULL,
    CONSTRAINT FK_Auditoria_UsuarioActor FOREIGN KEY (id_usuario_actor) REFERENCES Usuario(id_usuario)
);
GO

CREATE INDEX IX_Auditoria_Entidad_IdEntidad_Fecha
    ON Auditoria(entidad, id_entidad, fecha_evento DESC);
GO

CREATE TABLE Rol (
    id_rol INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    descripcion VARCHAR(255) NULL,
    estado_rol VARCHAR(20) NOT NULL DEFAULT 'ACTIVO',
    CONSTRAINT UQ_Rol_Nombre UNIQUE (nombre),
    CONSTRAINT CK_Rol_Estado CHECK (estado_rol IN ('ACTIVO', 'INACTIVO'))
);
GO

CREATE TABLE Permiso (
    id_permiso INT IDENTITY(1,1) PRIMARY KEY,
    codigo VARCHAR(100) NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    descripcion VARCHAR(255) NULL,
    modulo VARCHAR(100) NOT NULL,
    accion VARCHAR(100) NOT NULL,
    estado_permiso VARCHAR(20) NOT NULL DEFAULT 'ACTIVO',
    CONSTRAINT UQ_Permiso_Codigo UNIQUE (codigo),
    CONSTRAINT CK_Permiso_Estado CHECK (estado_permiso IN ('ACTIVO', 'INACTIVO'))
);
GO

CREATE TABLE UsuarioRol (
    id_usuario_rol INT IDENTITY(1,1) PRIMARY KEY,
    id_usuario INT NOT NULL,
    id_rol INT NOT NULL,
    fecha_asignacion DATETIME NOT NULL DEFAULT GETDATE(),
    estado_usuario_rol VARCHAR(20) NOT NULL DEFAULT 'ACTIVO',
    CONSTRAINT UQ_UsuarioRol UNIQUE (id_usuario, id_rol),
    CONSTRAINT CK_UsuarioRol_Estado CHECK (estado_usuario_rol IN ('ACTIVO', 'INACTIVO')),
    CONSTRAINT FK_UsuarioRol_Usuario FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario),
    CONSTRAINT FK_UsuarioRol_Rol FOREIGN KEY (id_rol) REFERENCES Rol(id_rol)
);
GO

CREATE TABLE RolPermiso (
    id_rol_permiso INT IDENTITY(1,1) PRIMARY KEY,
    id_rol INT NOT NULL,
    id_permiso INT NOT NULL,
    CONSTRAINT UQ_RolPermiso UNIQUE (id_rol, id_permiso),
    CONSTRAINT FK_RolPermiso_Rol FOREIGN KEY (id_rol) REFERENCES Rol(id_rol),
    CONSTRAINT FK_RolPermiso_Permiso FOREIGN KEY (id_permiso) REFERENCES Permiso(id_permiso)
);
GO

CREATE TABLE ComponentePermiso (
    id_componente INT IDENTITY(1,1) PRIMARY KEY,
    codigo VARCHAR(100) NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    descripcion VARCHAR(255) NULL,
    tipo VARCHAR(20) NOT NULL,
    estado_componente VARCHAR(20) NOT NULL DEFAULT 'ACTIVO',
    CONSTRAINT UQ_ComponentePermiso_Codigo UNIQUE (codigo),
    CONSTRAINT CK_ComponentePermiso_Tipo CHECK (tipo IN ('FAMILIA', 'PERMISO')),
    CONSTRAINT CK_ComponentePermiso_Estado CHECK (estado_componente IN ('ACTIVO', 'INACTIVO'))
);
GO

CREATE TABLE ComponentePermisoRelacion (
    id_relacion INT IDENTITY(1,1) PRIMARY KEY,
    id_padre INT NOT NULL,
    id_hijo INT NOT NULL,
    CONSTRAINT UQ_ComponentePermisoRelacion UNIQUE (id_padre, id_hijo),
    CONSTRAINT CK_ComponentePermisoRelacion_NoAutoReferencia CHECK (id_padre <> id_hijo),
    CONSTRAINT FK_ComponentePermisoRelacion_Padre FOREIGN KEY (id_padre) REFERENCES ComponentePermiso(id_componente),
    CONSTRAINT FK_ComponentePermisoRelacion_Hijo FOREIGN KEY (id_hijo) REFERENCES ComponentePermiso(id_componente)
);
GO

CREATE TABLE UsuarioComponentePermiso (
    id_usuario_componente INT IDENTITY(1,1) PRIMARY KEY,
    id_usuario INT NOT NULL,
    id_componente INT NOT NULL,
    fecha_asignacion DATETIME NOT NULL DEFAULT GETDATE(),
    estado_usuario_componente VARCHAR(20) NOT NULL DEFAULT 'ACTIVO',
    CONSTRAINT UQ_UsuarioComponentePermiso UNIQUE (id_usuario, id_componente),
    CONSTRAINT CK_UsuarioComponentePermiso_Estado CHECK (estado_usuario_componente IN ('ACTIVO', 'INACTIVO')),
    CONSTRAINT FK_UsuarioComponentePermiso_Usuario FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario),
    CONSTRAINT FK_UsuarioComponentePermiso_Componente FOREIGN KEY (id_componente) REFERENCES ComponentePermiso(id_componente)
);
GO

CREATE UNIQUE INDEX UX_UsuarioComponentePermiso_UsuarioActivo
    ON UsuarioComponentePermiso(id_usuario)
    WHERE estado_usuario_componente = 'ACTIVO';
GO

CREATE TABLE Etiqueta (
    id_etiqueta INT IDENTITY(1,1) PRIMARY KEY,
    clave VARCHAR(150) NOT NULL,
    descripcion VARCHAR(255) NULL,
    CONSTRAINT UQ_Etiqueta_Clave UNIQUE (clave)
);
GO

CREATE TABLE Traduccion (
    id_traduccion INT IDENTITY(1,1) PRIMARY KEY,
    id_etiqueta INT NOT NULL,
    id_idioma INT NOT NULL,
    texto NVARCHAR(500) NOT NULL,
    CONSTRAINT UQ_Traduccion UNIQUE (id_etiqueta, id_idioma),
    CONSTRAINT FK_Traduccion_Etiqueta FOREIGN KEY (id_etiqueta) REFERENCES Etiqueta(id_etiqueta),
    CONSTRAINT FK_Traduccion_Idioma FOREIGN KEY (id_idioma) REFERENCES Idioma(id_idioma)
);
GO

-- ============================================================
-- PACIENTES, PROFESIONALES, CONSULTORIOS Y TURNOS
-- ============================================================

CREATE TABLE Paciente (
    id_paciente INT IDENTITY(1,1) PRIMARY KEY,
    id_usuario INT NULL,
    dni VARCHAR(20) NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    apellido VARCHAR(100) NOT NULL,
    fecha_nacimiento DATE NULL,
    telefono VARCHAR(50) NULL,
    email VARCHAR(150) NULL,
    direccion VARCHAR(255) NULL,
    estado_paciente VARCHAR(20) NOT NULL DEFAULT 'Activo',
    fecha_alta DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_Paciente_DNI UNIQUE (dni),
    CONSTRAINT UQ_Paciente_Usuario UNIQUE (id_usuario),
    CONSTRAINT CK_Paciente_Estado CHECK (estado_paciente IN ('Activo', 'Inactivo')),
    CONSTRAINT FK_Paciente_Usuario FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario)
);
GO

CREATE TABLE ObraSocial (
    id_obra_social INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(150) NOT NULL,
    cuit VARCHAR(20) NULL,
    telefono VARCHAR(50) NULL,
    email VARCHAR(150) NULL,
    estado_obra_social VARCHAR(20) NOT NULL DEFAULT 'Activo',
    CONSTRAINT UQ_ObraSocial_Nombre UNIQUE (nombre),
    CONSTRAINT CK_ObraSocial_Estado CHECK (estado_obra_social IN ('Activo', 'Inactivo'))
);
GO

CREATE TABLE PacienteObraSocial (
    id_paciente_obra_social INT IDENTITY(1,1) PRIMARY KEY,
    id_paciente INT NOT NULL,
    id_obra_social INT NOT NULL,
    numero_afiliado VARCHAR(100) NOT NULL,
    plan_os VARCHAR(100) NULL,
    fecha_desde DATE NOT NULL,
    fecha_hasta DATE NULL,
    estado_paciente_obra_social VARCHAR(20) NOT NULL DEFAULT 'Activo',
    CONSTRAINT CK_PacienteObraSocial_Estado CHECK (estado_paciente_obra_social IN ('Activo', 'Inactivo')),
    CONSTRAINT CK_PacienteObraSocial_Fechas CHECK (fecha_hasta IS NULL OR fecha_hasta >= fecha_desde),
    CONSTRAINT FK_PacienteObraSocial_Paciente FOREIGN KEY (id_paciente) REFERENCES Paciente(id_paciente),
    CONSTRAINT FK_PacienteObraSocial_ObraSocial FOREIGN KEY (id_obra_social) REFERENCES ObraSocial(id_obra_social)
);
GO

CREATE TABLE Especialidad (
    id_especialidad INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    descripcion VARCHAR(255) NULL,
    CONSTRAINT UQ_Especialidad_Nombre UNIQUE (nombre)
);
GO

CREATE TABLE Profesional (
    id_profesional INT IDENTITY(1,1) PRIMARY KEY,
    id_usuario INT NOT NULL,
    id_especialidad INT NOT NULL,
    matricula VARCHAR(50) NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    apellido VARCHAR(100) NOT NULL,
    telefono VARCHAR(50) NULL,
    email VARCHAR(150) NULL,
    estado_profesional VARCHAR(20) NOT NULL DEFAULT 'Activo',
    CONSTRAINT UQ_Profesional_Usuario UNIQUE (id_usuario),
    CONSTRAINT UQ_Profesional_Matricula UNIQUE (matricula),
    CONSTRAINT CK_Profesional_Estado CHECK (estado_profesional IN ('Activo', 'Inactivo')),
    CONSTRAINT FK_Profesional_Usuario FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario),
    CONSTRAINT FK_Profesional_Especialidad FOREIGN KEY (id_especialidad) REFERENCES Especialidad(id_especialidad)
);
GO

CREATE TABLE Consultorio (
    id_consultorio INT IDENTITY(1,1) PRIMARY KEY,
    nombre_o_numero VARCHAR(50) NOT NULL,
    ubicacion VARCHAR(150) NULL,
    estado_consultorio VARCHAR(20) NOT NULL DEFAULT 'Activo',
    CONSTRAINT UQ_Consultorio_Nombre UNIQUE (nombre_o_numero),
    CONSTRAINT CK_Consultorio_Estado CHECK (estado_consultorio IN ('Activo', 'Inactivo', 'Mantenimiento'))
);
GO

CREATE TABLE JornadaProfesional (
    id_jornada INT IDENTITY(1,1) PRIMARY KEY,
    id_profesional INT NOT NULL,
    id_consultorio INT NOT NULL,
    fecha DATE NOT NULL,
    hora_inicio TIME NOT NULL,
    hora_fin TIME NOT NULL,
    duracion_turno_min INT NOT NULL,
    estado_jornada VARCHAR(20) NOT NULL DEFAULT 'Activa',
    CONSTRAINT CK_Jornada_Horario CHECK (hora_fin > hora_inicio),
    CONSTRAINT CK_Jornada_Duracion CHECK (duracion_turno_min > 0),
    CONSTRAINT CK_Jornada_Estado CHECK (estado_jornada IN ('Activa', 'Cancelada', 'Finalizada')),
    CONSTRAINT FK_Jornada_Profesional FOREIGN KEY (id_profesional) REFERENCES Profesional(id_profesional),
    CONSTRAINT FK_Jornada_Consultorio FOREIGN KEY (id_consultorio) REFERENCES Consultorio(id_consultorio)
);
GO

CREATE TABLE Turno (
    id_turno INT IDENTITY(1,1) PRIMARY KEY,
    id_jornada INT NOT NULL,
    id_paciente INT NOT NULL,
    id_paciente_obra_social INT NULL,
    fecha_hora_inicio DATETIME NOT NULL,
    fecha_hora_fin DATETIME NOT NULL,
    estado_turno VARCHAR(30) NOT NULL DEFAULT 'Reservado',
    origen_turno VARCHAR(30) NOT NULL,
    motivo_consulta VARCHAR(255) NULL,
    fecha_reserva DATETIME NOT NULL DEFAULT GETDATE(),
    id_usuario_creador INT NOT NULL,
    CONSTRAINT CK_Turno_Horario CHECK (fecha_hora_fin > fecha_hora_inicio),
    CONSTRAINT CK_Turno_Estado CHECK (estado_turno IN ('Reservado', 'Confirmado', 'Cancelado', 'Atendido', 'Ausente', 'Reprogramado')),
    CONSTRAINT CK_Turno_Origen CHECK (origen_turno IN ('Autogestionado', 'Recepcion', 'Administracion')),
    CONSTRAINT UQ_Turno_Jornada_Inicio UNIQUE (id_jornada, fecha_hora_inicio),
    CONSTRAINT FK_Turno_Jornada FOREIGN KEY (id_jornada) REFERENCES JornadaProfesional(id_jornada),
    CONSTRAINT FK_Turno_Paciente FOREIGN KEY (id_paciente) REFERENCES Paciente(id_paciente),
    CONSTRAINT FK_Turno_PacienteObraSocial FOREIGN KEY (id_paciente_obra_social) REFERENCES PacienteObraSocial(id_paciente_obra_social),
    CONSTRAINT FK_Turno_UsuarioCreador FOREIGN KEY (id_usuario_creador) REFERENCES Usuario(id_usuario)
);
GO

CREATE TABLE HistorialEstadoTurno (
    id_historial INT IDENTITY(1,1) PRIMARY KEY,
    id_turno INT NOT NULL,
    estado_anterior VARCHAR(30) NULL,
    estado_nuevo VARCHAR(30) NOT NULL,
    fecha_cambio DATETIME NOT NULL DEFAULT GETDATE(),
    motivo VARCHAR(255) NULL,
    id_usuario_responsable INT NOT NULL,
    CONSTRAINT FK_HistorialTurno_Turno FOREIGN KEY (id_turno) REFERENCES Turno(id_turno),
    CONSTRAINT FK_HistorialTurno_Usuario FOREIGN KEY (id_usuario_responsable) REFERENCES Usuario(id_usuario)
);
GO

CREATE TABLE Atencion (
    id_atencion INT IDENTITY(1,1) PRIMARY KEY,
    id_turno INT NOT NULL,
    fecha_hora_llegada DATETIME NULL,
    fecha_hora_inicio_atencion DATETIME NULL,
    fecha_hora_fin_atencion DATETIME NULL,
    observaciones_operativas VARCHAR(500) NULL,
    id_usuario_responsable INT NOT NULL,
    CONSTRAINT UQ_Atencion_Turno UNIQUE (id_turno),
    CONSTRAINT CK_Atencion_Horario CHECK (
        fecha_hora_fin_atencion IS NULL OR fecha_hora_inicio_atencion IS NULL OR fecha_hora_fin_atencion >= fecha_hora_inicio_atencion
    ),
    CONSTRAINT FK_Atencion_Turno FOREIGN KEY (id_turno) REFERENCES Turno(id_turno),
    CONSTRAINT FK_Atencion_Usuario FOREIGN KEY (id_usuario_responsable) REFERENCES Usuario(id_usuario)
);
GO

-- ============================================================
-- PAGOS DE PACIENTES
-- ============================================================

CREATE TABLE MedioPago (
    id_medio_pago INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    descripcion VARCHAR(255) NULL,
    estado_medio_pago VARCHAR(20) NOT NULL DEFAULT 'Activo',
    CONSTRAINT UQ_MedioPago_Nombre UNIQUE (nombre),
    CONSTRAINT CK_MedioPago_Estado CHECK (estado_medio_pago IN ('Activo', 'Inactivo'))
);
GO

CREATE TABLE Pago (
    id_pago INT IDENTITY(1,1) PRIMARY KEY,
    id_turno INT NOT NULL,
    id_atencion INT NULL,
    id_paciente INT NOT NULL,
    id_medio_pago INT NOT NULL,
    monto DECIMAL(12,2) NOT NULL,
    fecha_pago DATETIME NULL,
    estado_pago VARCHAR(30) NOT NULL DEFAULT 'Pendiente',
    origen_pago VARCHAR(30) NOT NULL,
    id_usuario_registro INT NOT NULL,
    observaciones VARCHAR(255) NULL,
    CONSTRAINT CK_Pago_Monto CHECK (monto >= 0),
    CONSTRAINT CK_Pago_Estado CHECK (estado_pago IN ('Pendiente', 'Pagado', 'Rechazado', 'Anulado', 'Devuelto')),
    CONSTRAINT CK_Pago_Origen CHECK (origen_pago IN ('Autogestionado', 'Recepcion', 'Administracion')),
    CONSTRAINT FK_Pago_Turno FOREIGN KEY (id_turno) REFERENCES Turno(id_turno),
    CONSTRAINT FK_Pago_Atencion FOREIGN KEY (id_atencion) REFERENCES Atencion(id_atencion),
    CONSTRAINT FK_Pago_Paciente FOREIGN KEY (id_paciente) REFERENCES Paciente(id_paciente),
    CONSTRAINT FK_Pago_MedioPago FOREIGN KEY (id_medio_pago) REFERENCES MedioPago(id_medio_pago),
    CONSTRAINT FK_Pago_Usuario FOREIGN KEY (id_usuario_registro) REFERENCES Usuario(id_usuario)
);
GO

-- ============================================================
-- INVENTARIO E INSUMOS
-- ============================================================

CREATE TABLE CategoriaInsumo (
    id_categoria INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    descripcion VARCHAR(255) NULL,
    CONSTRAINT UQ_CategoriaInsumo_Nombre UNIQUE (nombre)
);
GO

CREATE TABLE Insumo (
    id_insumo INT IDENTITY(1,1) PRIMARY KEY,
    id_categoria INT NOT NULL,
    nombre VARCHAR(150) NOT NULL,
    descripcion VARCHAR(255) NULL,
    unidad_medida VARCHAR(50) NOT NULL,
    stock_minimo DECIMAL(12,2) NOT NULL DEFAULT 0,
    punto_reposicion DECIMAL(12,2) NOT NULL DEFAULT 0,
    estado_insumo VARCHAR(20) NOT NULL DEFAULT 'Activo',
    CONSTRAINT CK_Insumo_StockMinimo CHECK (stock_minimo >= 0),
    CONSTRAINT CK_Insumo_PuntoReposicion CHECK (punto_reposicion >= 0),
    CONSTRAINT CK_Insumo_Estado CHECK (estado_insumo IN ('Activo', 'Inactivo')),
    CONSTRAINT FK_Insumo_Categoria FOREIGN KEY (id_categoria) REFERENCES CategoriaInsumo(id_categoria)
);
GO

CREATE TABLE LoteInsumo (
    id_lote INT IDENTITY(1,1) PRIMARY KEY,
    id_insumo INT NOT NULL,
    numero_lote VARCHAR(100) NOT NULL,
    fecha_ingreso DATE NOT NULL,
    fecha_vencimiento DATE NULL,
    cantidad_inicial DECIMAL(12,2) NOT NULL,
    cantidad_disponible DECIMAL(12,2) NOT NULL,
    CONSTRAINT CK_Lote_CantidadInicial CHECK (cantidad_inicial >= 0),
    CONSTRAINT CK_Lote_CantidadDisponible CHECK (cantidad_disponible >= 0),
    CONSTRAINT CK_Lote_DisponibleInicial CHECK (cantidad_disponible <= cantidad_inicial),
    CONSTRAINT UQ_Lote_Insumo_Numero UNIQUE (id_insumo, numero_lote),
    CONSTRAINT FK_Lote_Insumo FOREIGN KEY (id_insumo) REFERENCES Insumo(id_insumo)
);
GO

CREATE TABLE DetalleConsumoAtencion (
    id_detalle_consumo INT IDENTITY(1,1) PRIMARY KEY,
    id_atencion INT NOT NULL,
    id_insumo INT NOT NULL,
    id_lote INT NOT NULL,
    cantidad_consumida DECIMAL(12,2) NOT NULL,
    CONSTRAINT CK_DetalleConsumo_Cantidad CHECK (cantidad_consumida > 0),
    CONSTRAINT FK_DetalleConsumo_Atencion FOREIGN KEY (id_atencion) REFERENCES Atencion(id_atencion),
    CONSTRAINT FK_DetalleConsumo_Insumo FOREIGN KEY (id_insumo) REFERENCES Insumo(id_insumo),
    CONSTRAINT FK_DetalleConsumo_Lote FOREIGN KEY (id_lote) REFERENCES LoteInsumo(id_lote)
);
GO

CREATE TABLE MovimientoStock (
    id_movimiento INT IDENTITY(1,1) PRIMARY KEY,
    id_insumo INT NOT NULL,
    id_lote INT NOT NULL,
    tipo_movimiento VARCHAR(30) NOT NULL,
    cantidad DECIMAL(12,2) NOT NULL,
    fecha_movimiento DATETIME NOT NULL DEFAULT GETDATE(),
    motivo VARCHAR(255) NULL,
    id_atencion INT NULL,
    id_usuario_responsable INT NOT NULL,
    CONSTRAINT CK_MovimientoStock_Cantidad CHECK (cantidad > 0),
    CONSTRAINT CK_MovimientoStock_Tipo CHECK (tipo_movimiento IN ('Ingreso', 'EgresoPorAtencion', 'AjusteEntrada', 'AjusteSalida', 'Vencimiento', 'Baja', 'Reposicion')),
    CONSTRAINT FK_MovimientoStock_Insumo FOREIGN KEY (id_insumo) REFERENCES Insumo(id_insumo),
    CONSTRAINT FK_MovimientoStock_Lote FOREIGN KEY (id_lote) REFERENCES LoteInsumo(id_lote),
    CONSTRAINT FK_MovimientoStock_Atencion FOREIGN KEY (id_atencion) REFERENCES Atencion(id_atencion),
    CONSTRAINT FK_MovimientoStock_Usuario FOREIGN KEY (id_usuario_responsable) REFERENCES Usuario(id_usuario)
);
GO

-- ============================================================
-- PROVEEDORES, COMPRAS Y PAGOS A PROVEEDORES
-- ============================================================

CREATE TABLE Proveedor (
    id_proveedor INT IDENTITY(1,1) PRIMARY KEY,
    razon_social VARCHAR(150) NOT NULL,
    cuit VARCHAR(20) NULL,
    telefono VARCHAR(50) NULL,
    email VARCHAR(150) NULL,
    direccion VARCHAR(255) NULL,
    estado_proveedor VARCHAR(20) NOT NULL DEFAULT 'Activo',
    CONSTRAINT UQ_Proveedor_CUIT UNIQUE (cuit),
    CONSTRAINT CK_Proveedor_Estado CHECK (estado_proveedor IN ('Activo', 'Inactivo'))
);
GO

CREATE TABLE OrdenCompra (
    id_orden_compra INT IDENTITY(1,1) PRIMARY KEY,
    id_proveedor INT NOT NULL,
    fecha_orden DATETIME NOT NULL DEFAULT GETDATE(),
    estado_orden VARCHAR(30) NOT NULL DEFAULT 'Pendiente',
    observaciones VARCHAR(255) NULL,
    id_usuario_creador INT NOT NULL,
    CONSTRAINT CK_OrdenCompra_Estado CHECK (estado_orden IN ('Pendiente', 'Aprobada', 'Recibida', 'Cancelada')),
    CONSTRAINT FK_OrdenCompra_Proveedor FOREIGN KEY (id_proveedor) REFERENCES Proveedor(id_proveedor),
    CONSTRAINT FK_OrdenCompra_Usuario FOREIGN KEY (id_usuario_creador) REFERENCES Usuario(id_usuario)
);
GO

CREATE TABLE DetalleOrdenCompra (
    id_detalle_orden INT IDENTITY(1,1) PRIMARY KEY,
    id_orden_compra INT NOT NULL,
    id_insumo INT NOT NULL,
    cantidad DECIMAL(12,2) NOT NULL,
    precio_unitario DECIMAL(12,2) NOT NULL,
    CONSTRAINT CK_DetalleOrden_Cantidad CHECK (cantidad > 0),
    CONSTRAINT CK_DetalleOrden_Precio CHECK (precio_unitario >= 0),
    CONSTRAINT FK_DetalleOrden_Orden FOREIGN KEY (id_orden_compra) REFERENCES OrdenCompra(id_orden_compra),
    CONSTRAINT FK_DetalleOrden_Insumo FOREIGN KEY (id_insumo) REFERENCES Insumo(id_insumo)
);
GO

CREATE TABLE FacturaProveedor (
    id_factura_proveedor INT IDENTITY(1,1) PRIMARY KEY,
    id_proveedor INT NOT NULL,
    id_orden_compra INT NULL,
    numero_factura VARCHAR(100) NOT NULL,
    fecha_emision DATE NOT NULL,
    fecha_vencimiento DATE NULL,
    importe_total DECIMAL(12,2) NOT NULL,
    estado_factura VARCHAR(30) NOT NULL DEFAULT 'Pendiente',
    observaciones VARCHAR(255) NULL,
    CONSTRAINT CK_FacturaProveedor_Importe CHECK (importe_total >= 0),
    CONSTRAINT CK_FacturaProveedor_Estado CHECK (estado_factura IN ('Pendiente', 'Pagada', 'PagadaParcial', 'Anulada', 'Vencida')),
    CONSTRAINT UQ_FacturaProveedor UNIQUE (id_proveedor, numero_factura),
    CONSTRAINT FK_FacturaProveedor_Proveedor FOREIGN KEY (id_proveedor) REFERENCES Proveedor(id_proveedor),
    CONSTRAINT FK_FacturaProveedor_Orden FOREIGN KEY (id_orden_compra) REFERENCES OrdenCompra(id_orden_compra)
);
GO

CREATE TABLE PagoProveedor (
    id_pago_proveedor INT IDENTITY(1,1) PRIMARY KEY,
    id_factura_proveedor INT NOT NULL,
    id_medio_pago INT NOT NULL,
    fecha_pago DATETIME NOT NULL DEFAULT GETDATE(),
    importe_pagado DECIMAL(12,2) NOT NULL,
    nro_comprobante VARCHAR(100) NULL,
    observaciones VARCHAR(255) NULL,
    id_usuario_registro INT NOT NULL,
    CONSTRAINT CK_PagoProveedor_Importe CHECK (importe_pagado > 0),
    CONSTRAINT FK_PagoProveedor_Factura FOREIGN KEY (id_factura_proveedor) REFERENCES FacturaProveedor(id_factura_proveedor),
    CONSTRAINT FK_PagoProveedor_MedioPago FOREIGN KEY (id_medio_pago) REFERENCES MedioPago(id_medio_pago),
    CONSTRAINT FK_PagoProveedor_Usuario FOREIGN KEY (id_usuario_registro) REFERENCES Usuario(id_usuario)
);
GO

-- ============================================================
-- INDICES RECOMENDADOS
-- ============================================================

CREATE INDEX IX_Turno_Paciente ON Turno(id_paciente);
CREATE INDEX IX_Turno_Jornada ON Turno(id_jornada);
CREATE INDEX IX_Turno_FechaHora ON Turno(fecha_hora_inicio, fecha_hora_fin);
CREATE INDEX IX_Pago_Turno ON Pago(id_turno);
CREATE INDEX IX_Pago_Paciente ON Pago(id_paciente);
CREATE INDEX IX_MovimientoStock_Insumo ON MovimientoStock(id_insumo);
CREATE INDEX IX_MovimientoStock_Lote ON MovimientoStock(id_lote);
CREATE INDEX IX_LoteInsumo_Vencimiento ON LoteInsumo(fecha_vencimiento);
CREATE INDEX IX_FacturaProveedor_Proveedor ON FacturaProveedor(id_proveedor);
GO

-- ============================================================
-- DATOS INICIALES SUGERIDOS
-- ============================================================

INSERT INTO Idioma (codigo, nombre) VALUES
('es-AR', 'Español Argentina'),
('en-US', 'Inglés Estados Unidos');
GO

INSERT INTO Etiqueta (clave, descripcion) VALUES
('MAIN_TITLE', 'Titulo de la ventana principal'),
('MAIN_USER', 'Usuario autenticado'),
('MAIN_NO_SESSION', 'Usuario sin sesion'),
('MENU_AUDIT', 'Menu bitacora'),
('MENU_CHANGE_AUDIT', 'Menu auditoria de cambios'),
('MENU_USERS', 'Menu usuarios'),
('MENU_PERMISSIONS', 'Menu permisos'),
('MENU_ROLES', 'Menu roles'),
('MENU_LANGUAGES', 'Menu idiomas'),
('MENU_LOGOUT', 'Menu cerrar sesion'),
('LANGUAGE_SELECTOR', 'Selector de idioma'),
('AUDIT_TITLE', 'Titulo bitacora'),
('AUDIT_DESCRIPTION', 'Descripcion bitacora'),
('AUDIT_EMPTY', 'Bitacora sin eventos'),
('AUDIT_COUNT', 'Cantidad de eventos'),
('AUDIT_FILTERS', 'Filtros de bitacora'),
('AUDIT_FILTER_FROM', 'Filtro fecha desde'),
('AUDIT_FILTER_TO', 'Filtro fecha hasta'),
('AUDIT_FILTER_USER', 'Filtro usuario'),
('AUDIT_FILTER_MODULE', 'Filtro modulo'),
('AUDIT_FILTER_ACTION', 'Filtro accion'),
('AUDIT_FILTER_LEVEL', 'Filtro nivel'),
('AUDIT_FILTER_DESCRIPTION', 'Filtro descripcion'),
('BITACORA_ACTION_LOGIN_FAILURE', 'Accion login fallido'),
('BITACORA_ACTION_LOGIN_SUCCESS', 'Accion login exitoso'),
('BITACORA_ACTION_REGISTER_FAILURE', 'Accion registro fallido'),
('BITACORA_LEVEL_ERROR', 'Nivel error'),
('BITACORA_LEVEL_INFORMATION', 'Nivel informacion'),
('BITACORA_LEVEL_WARNING', 'Nivel advertencia'),
('BITACORA_MODULE_SECURITY', 'Modulo seguridad'),
('CHANGE_AUDIT_TITLE', 'Titulo auditoria de cambios'),
('CHANGE_AUDIT_DESCRIPTION', 'Descripcion auditoria de cambios'),
('CHANGE_AUDIT_EMPTY', 'Auditoria de cambios sin eventos'),
('CHANGE_AUDIT_COUNT', 'Cantidad de cambios auditados'),
('CHANGE_AUDIT_USER', 'Usuario auditado'),
('CHANGE_AUDIT_PREVIOUS_STATE', 'Estado anterior'),
('CHANGE_AUDIT_NEW_STATE', 'Estado nuevo'),
('USERS_TITLE', 'Titulo usuarios'),
('USERS_DESCRIPTION', 'Descripcion usuarios'),
('USERS_DETAIL', 'Detalle usuarios'),
('USERS_CREATE_MODE', 'Modo crear usuario'),
('USERS_EDIT_MODE', 'Modo editar usuario'),
('PERMISSIONS_TITLE', 'Titulo permisos'),
('PERMISSIONS_DESCRIPTION', 'Descripcion permisos'),
('PERMISSIONS_AVAILABLE', 'Permisos disponibles'),
('PERMISSIONS_TREE', 'Arbol de permisos'),
('ROLES_TITLE', 'Titulo roles'),
('ROLES_DESCRIPTION', 'Descripcion roles'),
('ROLES_STRUCTURE', 'Estructura de roles'),
('ROLES_ADMIN', 'Administracion de roles'),
('ROLE_CODE', 'Codigo de rol'),
('ROLE_SELECTED_FAMILY', 'Familia seleccionada'),
('ROLE_CHILD_COMPONENT', 'Componente hijo de rol'),
('ROLE_SELECT_FAMILY', 'Seleccionar familia de rol'),
('ROLE_SELECT_FAMILY_AND_COMPONENT', 'Seleccionar familia y componente de rol'),
('ROLE_SELECT_CHILD', 'Seleccionar hijo de rol'),
('ROLE_CREATED', 'Rol creado'),
('ROLE_CREATE_ERROR', 'Error al crear rol'),
('ROLE_RELATION_ADDED', 'Relacion de rol agregada'),
('ROLE_RELATION_ADD_ERROR', 'Error al agregar relacion de rol'),
('ROLE_RELATION_REMOVED', 'Relacion de rol quitada'),
('ROLE_RELATION_REMOVE_ERROR', 'Error al quitar relacion de rol'),
('ROLE_RELATION_IDENTIFY_ERROR', 'Error al identificar relacion de rol'),
('ROLE_SELF_REFERENCE_ERROR', 'Error por autorreferencia de rol'),
('ROLE_INVALID_PARENT', 'Padre de rol invalido'),
('ROLE_INVALID_CHILD', 'Hijo de rol invalido'),
('ROLE_CYCLE_ERROR', 'Ciclo detectado en roles'),
('USER_ROLES', 'Roles de usuario'),
('USER_ROLE_SELECT_HELP', 'Ayuda seleccion rol de usuario'),
('USER_ROLE_EDIT_DENIED', 'Permiso denegado para modificar rol de usuario'),
('USER_ROLE_EMPTY', 'Roles no disponibles para usuario'),
('USER_ROLE_SELECT_ONE', 'Seleccionar un rol de usuario'),
('SECURITY_ACCESS_DENIED', 'Acceso denegado'),
('SECURITY_ROLE_CREATE_DENIED', 'Creacion de rol denegada'),
('SECURITY_ROLE_EDIT_DENIED', 'Edicion de rol denegada'),
('SECURITY_LANGUAGE_CREATE_DENIED', 'Creacion de idioma denegada'),
('SECURITY_LANGUAGE_EDIT_DENIED', 'Edicion de idioma denegada'),
('SECURITY_TRANSLATION_EDIT_DENIED', 'Edicion de traduccion denegada'),
('NO_PERMISSIONS_ASSIGNED', 'Sin permisos asignados'),
('LANGUAGES_TITLE', 'Titulo idiomas'),
('LANGUAGES_DESCRIPTION', 'Descripcion idiomas'),
('LANGUAGE_DETAIL', 'Detalle idioma'),
('LABEL_DETAIL', 'Detalle etiqueta'),
('TRANSLATION_DETAIL', 'Detalle traduccion'),
('LANGUAGE_CODE', 'Codigo de idioma'),
('LANGUAGE_ACTIVE', 'Idioma activo'),
('LABEL_KEY', 'Clave de etiqueta'),
('LABEL_TAG', 'Etiqueta'),
('TRANSLATION_TEXT', 'Texto traducido'),
('LANGUAGE_SAVED', 'Idioma guardado'),
('LABEL_SAVED', 'Etiqueta guardada'),
('TRANSLATION_SAVED', 'Traduccion guardada'),
('SAVE_ERROR', 'Error al guardar'),
('BTN_NEW', 'Boton nuevo'),
('BTN_SAVE', 'Boton guardar'),
('BTN_CREATE', 'Boton crear'),
('BTN_DISABLE', 'Boton inhabilitar'),
('BTN_REFRESH', 'Boton actualizar'),
('BTN_SEARCH', 'Boton buscar'),
('BTN_CLEAR_FILTERS', 'Boton limpiar filtros'),
('BTN_RECALCULATE_DV', 'Boton recalcular digitos verificadores'),
('BTN_ADD', 'Boton agregar'),
('BTN_REMOVE_SELECTED', 'Boton quitar seleccionado'),
('BTN_REMOVE_FROM', 'Boton quitar de'),
('BTN_CREATE_LABEL', 'Boton crear etiqueta'),
('FIELD_USER', 'Campo usuario'),
('FIELD_EMAIL', 'Campo email'),
('FIELD_NAME', 'Campo nombre'),
('FIELD_LASTNAME', 'Campo apellido'),
('FIELD_NEW_PASSWORD', 'Campo contrasena nueva'),
('FIELD_STATUS', 'Campo estado'),
('FILTER_ALL', 'Filtro todos'),
('FILTER_ALL_ACTIONS', 'Filtro todas las acciones'),
('GRID_ID', 'Columna id'),
('GRID_USER', 'Columna usuario'),
('GRID_EMAIL', 'Columna email'),
('GRID_STATUS', 'Columna estado'),
('GRID_DV_BLOCK', 'Columna bloqueo digito verificador'),
('GRID_DATE', 'Columna fecha'),
('GRID_USER_ID', 'Columna id usuario'),
('GRID_MODULE', 'Columna modulo'),
('GRID_ACTION', 'Columna accion'),
('GRID_LEVEL', 'Columna nivel'),
('GRID_DESCRIPTION', 'Columna descripcion'),
('GRID_DEVICE', 'Columna equipo'),
('GRID_ENTITY', 'Columna entidad'),
('GRID_ENTITY_ID', 'Columna id entidad'),
('GRID_FIELD', 'Columna campo'),
('GRID_OLD_VALUE', 'Columna valor anterior'),
('GRID_NEW_VALUE', 'Columna valor nuevo');
GO

DECLARE @es INT = (SELECT id_idioma FROM Idioma WHERE codigo = 'es-AR');
DECLARE @en INT = (SELECT id_idioma FROM Idioma WHERE codigo = 'en-US');

INSERT INTO Traduccion (id_etiqueta, id_idioma, texto)
SELECT e.id_etiqueta, @es, v.texto
FROM Etiqueta e
INNER JOIN (VALUES
('MAIN_TITLE', 'Panel principal'),
('MAIN_USER', 'Usuario: {0}'),
('MAIN_NO_SESSION', 'Usuario: sin sesion'),
('MENU_AUDIT', 'Bitacora'),
('MENU_CHANGE_AUDIT', 'Auditoria de cambios'),
('MENU_USERS', 'Usuarios'),
('MENU_PERMISSIONS', 'Permisos'),
('MENU_ROLES', 'Roles'),
('MENU_LANGUAGES', 'Idiomas'),
('MENU_LOGOUT', 'Cerrar sesion'),
('LANGUAGE_SELECTOR', 'Idioma'),
('AUDIT_TITLE', 'Bitacora'),
('AUDIT_DESCRIPTION', 'Aca vas a poder consultar los eventos del sistema.'),
('AUDIT_EMPTY', 'No hay eventos registrados.'),
('AUDIT_COUNT', '{0} evento(s) registrados.'),
('AUDIT_FILTERS', 'Filtros'),
('AUDIT_FILTER_FROM', 'Desde'),
('AUDIT_FILTER_TO', 'Hasta'),
('AUDIT_FILTER_USER', 'Usuario'),
('AUDIT_FILTER_MODULE', 'Modulo'),
('AUDIT_FILTER_ACTION', 'Accion'),
('AUDIT_FILTER_LEVEL', 'Nivel'),
('AUDIT_FILTER_DESCRIPTION', 'Descripcion'),
('BITACORA_ACTION_LOGIN_FAILURE', 'Login fallido'),
('BITACORA_ACTION_LOGIN_SUCCESS', 'Login exitoso'),
('BITACORA_ACTION_REGISTER_FAILURE', 'Registro fallido'),
('BITACORA_LEVEL_ERROR', 'Error'),
('BITACORA_LEVEL_INFORMATION', 'Informacion'),
('BITACORA_LEVEL_WARNING', 'Advertencia'),
('BITACORA_MODULE_SECURITY', 'Seguridad'),
('CHANGE_AUDIT_TITLE', 'Auditoria de cambios'),
('CHANGE_AUDIT_DESCRIPTION', 'Historial de cambios registrados sobre entidades auditadas.'),
('CHANGE_AUDIT_EMPTY', 'No hay cambios registrados para el usuario seleccionado.'),
('CHANGE_AUDIT_COUNT', '{0} cambio(s) registrado(s).'),
('CHANGE_AUDIT_USER', 'Usuario auditado'),
('CHANGE_AUDIT_PREVIOUS_STATE', 'Estado anterior'),
('CHANGE_AUDIT_NEW_STATE', 'Estado nuevo'),
('USERS_TITLE', 'Usuarios'),
('USERS_DESCRIPTION', 'Alta, modificacion e inhabilitacion de usuarios del sistema.'),
('USERS_DETAIL', 'Detalle'),
('USERS_CREATE_MODE', 'Crear usuario'),
('USERS_EDIT_MODE', 'Modificar usuario'),
('PERMISSIONS_TITLE', 'Permisos'),
('PERMISSIONS_DESCRIPTION', 'Pantalla base para administrar permisos futuros.'),
('PERMISSIONS_AVAILABLE', 'Permisos disponibles'),
('PERMISSIONS_TREE', 'Arbol de familias y permisos'),
('ROLES_TITLE', 'Roles'),
('ROLES_DESCRIPTION', 'Pantalla base para asignacion y gestion de roles.'),
('ROLES_STRUCTURE', 'Estructura de roles'),
('ROLES_ADMIN', 'Administracion de roles'),
('ROLE_CODE', 'Codigo'),
('ROLE_SELECTED_FAMILY', 'Familia seleccionada en el arbol'),
('ROLE_CHILD_COMPONENT', 'Permiso o familia a agregar'),
('ROLE_SELECT_FAMILY', 'Selecciona una familia en el arbol'),
('ROLE_SELECT_FAMILY_AND_COMPONENT', 'Selecciona una familia en el arbol y un componente para agregar.'),
('ROLE_SELECT_CHILD', 'Selecciona un componente hijo dentro del arbol.'),
('ROLE_CREATED', 'Rol creado correctamente.'),
('ROLE_CREATE_ERROR', 'No se pudo crear el rol.'),
('ROLE_RELATION_ADDED', 'Componente agregado correctamente.'),
('ROLE_RELATION_ADD_ERROR', 'No se pudo agregar el componente.'),
('ROLE_RELATION_REMOVED', 'Relacion quitada correctamente.'),
('ROLE_RELATION_REMOVE_ERROR', 'No se pudo quitar la relacion.'),
('ROLE_RELATION_IDENTIFY_ERROR', 'No se pudo identificar la relacion seleccionada.'),
('ROLE_SELF_REFERENCE_ERROR', 'Un rol no puede agregarse a si mismo.'),
('ROLE_INVALID_PARENT', 'El padre debe ser una familia activa.'),
('ROLE_INVALID_CHILD', 'El componente hijo no existe o esta inactivo.'),
('ROLE_CYCLE_ERROR', 'No se puede agregar porque generaria una relacion circular.'),
('USER_ROLES', 'Roles'),
('USER_ROLE_SELECT_HELP', 'Selecciona un usuario para asignar rol.'),
('USER_ROLE_EDIT_DENIED', 'No tenes permiso para modificar roles.'),
('USER_ROLE_EMPTY', 'No hay roles cargados. Revisa el script de permisos.'),
('USER_ROLE_SELECT_ONE', 'Elegi un unico rol y guarda.'),
('SECURITY_ACCESS_DENIED', 'No tenes permisos para acceder a esta funcionalidad.'),
('SECURITY_ROLE_CREATE_DENIED', 'No tenes permisos para crear roles.'),
('SECURITY_ROLE_EDIT_DENIED', 'No tenes permisos para modificar roles.'),
('SECURITY_LANGUAGE_CREATE_DENIED', 'No tenes permisos para crear idiomas.'),
('SECURITY_LANGUAGE_EDIT_DENIED', 'No tenes permisos para modificar idiomas.'),
('SECURITY_TRANSLATION_EDIT_DENIED', 'No tenes permisos para modificar traducciones.'),
('NO_PERMISSIONS_ASSIGNED', 'No tenes permisos asignados para acceder a modulos del sistema.'),
('LANGUAGES_TITLE', 'Idiomas y traducciones'),
('LANGUAGES_DESCRIPTION', 'Administracion dinamica de idiomas, etiquetas y textos visibles.'),
('LANGUAGE_DETAIL', 'Idioma'),
('LABEL_DETAIL', 'Etiqueta'),
('TRANSLATION_DETAIL', 'Traduccion'),
('LANGUAGE_CODE', 'Codigo'),
('LANGUAGE_ACTIVE', 'Activo'),
('LABEL_KEY', 'Clave'),
('LABEL_TAG', 'Etiqueta'),
('TRANSLATION_TEXT', 'Traduccion'),
('LANGUAGE_SAVED', 'Idioma guardado correctamente.'),
('LABEL_SAVED', 'Etiqueta guardada correctamente.'),
('TRANSLATION_SAVED', 'Traduccion guardada correctamente.'),
('SAVE_ERROR', 'No se pudo guardar.'),
('BTN_NEW', 'Nuevo'),
('BTN_SAVE', 'Guardar'),
('BTN_CREATE', 'Crear'),
('BTN_DISABLE', 'Inhabilitar'),
('BTN_REFRESH', 'Actualizar'),
('BTN_SEARCH', 'Buscar'),
('BTN_CLEAR_FILTERS', 'Limpiar'),
('BTN_RECALCULATE_DV', 'Recalcular DV'),
('BTN_ADD', 'Agregar'),
('BTN_REMOVE_SELECTED', 'Quitar seleccionado'),
('BTN_REMOVE_FROM', 'Quitar de {0}'),
('BTN_CREATE_LABEL', 'Crear etiqueta'),
('FIELD_USER', 'Usuario'),
('FIELD_EMAIL', 'Email'),
('FIELD_NAME', 'Nombre'),
('FIELD_LASTNAME', 'Apellido'),
('FIELD_NEW_PASSWORD', 'Contrasena nueva'),
('FIELD_STATUS', 'Estado'),
('FILTER_ALL', 'Todos'),
('FILTER_ALL_ACTIONS', 'Todas'),
('GRID_ID', 'Id'),
('GRID_USER', 'Usuario'),
('GRID_EMAIL', 'Email'),
('GRID_STATUS', 'Estado'),
('GRID_DV_BLOCK', 'Bloqueo DV'),
('GRID_DATE', 'Fecha'),
('GRID_USER_ID', 'Id usuario'),
('GRID_MODULE', 'Modulo'),
('GRID_ACTION', 'Accion'),
('GRID_LEVEL', 'Nivel'),
('GRID_DESCRIPTION', 'Descripcion'),
('GRID_DEVICE', 'Equipo'),
('GRID_ENTITY', 'Entidad'),
('GRID_ENTITY_ID', 'Id entidad'),
('GRID_FIELD', 'Campo'),
('GRID_OLD_VALUE', 'Valor anterior'),
('GRID_NEW_VALUE', 'Valor nuevo')
) v(clave, texto) ON v.clave = e.clave;

INSERT INTO Traduccion (id_etiqueta, id_idioma, texto)
SELECT e.id_etiqueta, @en, v.texto
FROM Etiqueta e
INNER JOIN (VALUES
('MAIN_TITLE', 'Main panel'),
('MAIN_USER', 'User: {0}'),
('MAIN_NO_SESSION', 'User: no session'),
('MENU_AUDIT', 'Audit log'),
('MENU_CHANGE_AUDIT', 'Change audit'),
('MENU_USERS', 'Users'),
('MENU_PERMISSIONS', 'Permissions'),
('MENU_ROLES', 'Roles'),
('MENU_LANGUAGES', 'Languages'),
('MENU_LOGOUT', 'Log out'),
('LANGUAGE_SELECTOR', 'Language'),
('AUDIT_TITLE', 'Audit log'),
('AUDIT_DESCRIPTION', 'Review system events here.'),
('AUDIT_EMPTY', 'No events registered.'),
('AUDIT_COUNT', '{0} event(s) registered.'),
('AUDIT_FILTERS', 'Filters'),
('AUDIT_FILTER_FROM', 'From'),
('AUDIT_FILTER_TO', 'To'),
('AUDIT_FILTER_USER', 'User'),
('AUDIT_FILTER_MODULE', 'Module'),
('AUDIT_FILTER_ACTION', 'Action'),
('AUDIT_FILTER_LEVEL', 'Level'),
('AUDIT_FILTER_DESCRIPTION', 'Description'),
('BITACORA_ACTION_LOGIN_FAILURE', 'Failed login'),
('BITACORA_ACTION_LOGIN_SUCCESS', 'Successful login'),
('BITACORA_ACTION_REGISTER_FAILURE', 'Failed registration'),
('BITACORA_LEVEL_ERROR', 'Error'),
('BITACORA_LEVEL_INFORMATION', 'Information'),
('BITACORA_LEVEL_WARNING', 'Warning'),
('BITACORA_MODULE_SECURITY', 'Security'),
('CHANGE_AUDIT_TITLE', 'Change audit'),
('CHANGE_AUDIT_DESCRIPTION', 'History of recorded changes on audited entities.'),
('CHANGE_AUDIT_EMPTY', 'No changes registered for the selected user.'),
('CHANGE_AUDIT_COUNT', '{0} change(s) registered.'),
('CHANGE_AUDIT_USER', 'Audited user'),
('CHANGE_AUDIT_PREVIOUS_STATE', 'Previous state'),
('CHANGE_AUDIT_NEW_STATE', 'New state'),
('USERS_TITLE', 'Users'),
('USERS_DESCRIPTION', 'Create, edit and disable system users.'),
('USERS_DETAIL', 'Details'),
('USERS_CREATE_MODE', 'Create user'),
('USERS_EDIT_MODE', 'Edit user'),
('PERMISSIONS_TITLE', 'Permissions'),
('PERMISSIONS_DESCRIPTION', 'Base screen for future permission management.'),
('PERMISSIONS_AVAILABLE', 'Available permissions'),
('PERMISSIONS_TREE', 'Families and permissions tree'),
('ROLES_TITLE', 'Roles'),
('ROLES_DESCRIPTION', 'Base screen for role assignment and management.'),
('ROLES_STRUCTURE', 'Role structure'),
('ROLES_ADMIN', 'Role administration'),
('ROLE_CODE', 'Code'),
('ROLE_SELECTED_FAMILY', 'Family selected in the tree'),
('ROLE_CHILD_COMPONENT', 'Permission or family to add'),
('ROLE_SELECT_FAMILY', 'Select a family in the tree'),
('ROLE_SELECT_FAMILY_AND_COMPONENT', 'Select a family in the tree and a component to add.'),
('ROLE_SELECT_CHILD', 'Select a child component in the tree.'),
('ROLE_CREATED', 'Role created successfully.'),
('ROLE_CREATE_ERROR', 'Could not create the role.'),
('ROLE_RELATION_ADDED', 'Component added successfully.'),
('ROLE_RELATION_ADD_ERROR', 'Could not add the component.'),
('ROLE_RELATION_REMOVED', 'Relation removed successfully.'),
('ROLE_RELATION_REMOVE_ERROR', 'Could not remove the relation.'),
('ROLE_RELATION_IDENTIFY_ERROR', 'Could not identify the selected relation.'),
('ROLE_SELF_REFERENCE_ERROR', 'A role cannot be added to itself.'),
('ROLE_INVALID_PARENT', 'The parent must be an active family.'),
('ROLE_INVALID_CHILD', 'The child component does not exist or is inactive.'),
('ROLE_CYCLE_ERROR', 'Cannot add it because it would create a circular relation.'),
('USER_ROLES', 'Roles'),
('USER_ROLE_SELECT_HELP', 'Select a user to assign a role.'),
('USER_ROLE_EDIT_DENIED', 'You do not have permission to modify roles.'),
('USER_ROLE_EMPTY', 'There are no roles loaded. Check the permissions script.'),
('USER_ROLE_SELECT_ONE', 'Choose a single role and save.'),
('SECURITY_ACCESS_DENIED', 'You do not have permission to access this feature.'),
('SECURITY_ROLE_CREATE_DENIED', 'You do not have permission to create roles.'),
('SECURITY_ROLE_EDIT_DENIED', 'You do not have permission to modify roles.'),
('SECURITY_LANGUAGE_CREATE_DENIED', 'You do not have permission to create languages.'),
('SECURITY_LANGUAGE_EDIT_DENIED', 'You do not have permission to modify languages.'),
('SECURITY_TRANSLATION_EDIT_DENIED', 'You do not have permission to modify translations.'),
('NO_PERMISSIONS_ASSIGNED', 'You do not have assigned permissions to access system modules.'),
('LANGUAGES_TITLE', 'Languages and translations'),
('LANGUAGES_DESCRIPTION', 'Dynamic administration of languages, labels and visible texts.'),
('LANGUAGE_DETAIL', 'Language'),
('LABEL_DETAIL', 'Label'),
('TRANSLATION_DETAIL', 'Translation'),
('LANGUAGE_CODE', 'Code'),
('LANGUAGE_ACTIVE', 'Active'),
('LABEL_KEY', 'Key'),
('LABEL_TAG', 'Label'),
('TRANSLATION_TEXT', 'Translation'),
('LANGUAGE_SAVED', 'Language saved successfully.'),
('LABEL_SAVED', 'Label saved successfully.'),
('TRANSLATION_SAVED', 'Translation saved successfully.'),
('SAVE_ERROR', 'Could not save.'),
('BTN_NEW', 'New'),
('BTN_SAVE', 'Save'),
('BTN_CREATE', 'Create'),
('BTN_DISABLE', 'Disable'),
('BTN_REFRESH', 'Refresh'),
('BTN_SEARCH', 'Search'),
('BTN_CLEAR_FILTERS', 'Clear'),
('BTN_RECALCULATE_DV', 'Recalculate DV'),
('BTN_ADD', 'Add'),
('BTN_REMOVE_SELECTED', 'Remove selected'),
('BTN_REMOVE_FROM', 'Remove from {0}'),
('BTN_CREATE_LABEL', 'Create label'),
('FIELD_USER', 'User'),
('FIELD_EMAIL', 'Email'),
('FIELD_NAME', 'Name'),
('FIELD_LASTNAME', 'Last name'),
('FIELD_NEW_PASSWORD', 'New password'),
('FIELD_STATUS', 'Status'),
('FILTER_ALL', 'All'),
('FILTER_ALL_ACTIONS', 'All'),
('GRID_ID', 'Id'),
('GRID_USER', 'User'),
('GRID_EMAIL', 'Email'),
('GRID_STATUS', 'Status'),
('GRID_DV_BLOCK', 'DV block'),
('GRID_DATE', 'Date'),
('GRID_USER_ID', 'User id'),
('GRID_MODULE', 'Module'),
('GRID_ACTION', 'Action'),
('GRID_LEVEL', 'Level'),
('GRID_DESCRIPTION', 'Description'),
('GRID_DEVICE', 'Device'),
('GRID_ENTITY', 'Entity'),
('GRID_ENTITY_ID', 'Entity id'),
('GRID_FIELD', 'Field'),
('GRID_OLD_VALUE', 'Old value'),
('GRID_NEW_VALUE', 'New value')
) v(clave, texto) ON v.clave = e.clave;
GO

INSERT INTO MedioPago (nombre, descripcion) VALUES
('Efectivo', 'Pago en efectivo'),
('Tarjeta de débito', 'Pago con tarjeta de débito'),
('Tarjeta de crédito', 'Pago con tarjeta de crédito'),
('Transferencia', 'Pago por transferencia bancaria'),
('Mercado Pago', 'Pago mediante billetera virtual'),
('Obra social', 'Cobertura mediante obra social');
GO

INSERT INTO Rol (nombre, descripcion) VALUES
('Administrador', 'Acceso total al sistema'),
('Paciente', 'Usuario paciente con autogestión de turnos'),
('Recepcionista', 'Gestión de turnos y pacientes'),
('Profesional', 'Gestión de atenciones'),
('EncargadoStock', 'Gestión de inventario e insumos'),
('Direccion', 'Consulta de reportes e indicadores');
GO

INSERT INTO ComponentePermiso (codigo, nombre, descripcion, tipo) VALUES
('ADMINISTRADOR', 'Administrador', 'Familia con acceso total a los modulos actuales', 'FAMILIA'),
('SEGURIDAD', 'Seguridad', 'Familia para gestion de roles y permisos', 'FAMILIA'),
('AUDITORIA', 'Auditoria', 'Familia para consulta de bitacora e historial de cambios', 'FAMILIA'),
('IDIOMAS_TRADUCCIONES', 'Idiomas y traducciones', 'Familia para gestion de idiomas y traducciones', 'FAMILIA'),
('USUARIO_VER', 'Ver usuarios', 'Permite acceder al modulo de usuarios', 'PERMISO'),
('USUARIO_CREAR', 'Crear usuarios', 'Permite crear usuarios', 'PERMISO'),
('USUARIO_EDITAR', 'Editar usuarios', 'Permite modificar usuarios', 'PERMISO'),
('USUARIO_INHABILITAR', 'Inhabilitar usuarios', 'Permite inhabilitar usuarios', 'PERMISO'),
('ROL_VER', 'Ver roles', 'Permite acceder al modulo de roles', 'PERMISO'),
('ROL_CREAR', 'Crear roles', 'Permite crear familias de permisos', 'PERMISO'),
('ROL_EDITAR', 'Editar roles', 'Permite modificar familias de permisos', 'PERMISO'),
('ROL_INHABILITAR', 'Inhabilitar roles', 'Permite inhabilitar familias de permisos', 'PERMISO'),
('PERMISO_VER', 'Ver permisos', 'Permite acceder al modulo de permisos', 'PERMISO'),
('PERMISO_ASIGNAR', 'Asignar permisos', 'Permite asignar permisos o familias', 'PERMISO'),
('IDIOMA_VER', 'Ver idiomas', 'Permite acceder a la administracion de idiomas', 'PERMISO'),
('IDIOMA_CREAR', 'Crear idiomas', 'Permite crear nuevos idiomas', 'PERMISO'),
('IDIOMA_EDITAR', 'Editar idiomas', 'Permite modificar y activar o desactivar idiomas', 'PERMISO'),
('TRADUCCION_VER', 'Ver traducciones', 'Permite ver el arbol de etiquetas y traducciones de UI', 'PERMISO'),
('TRADUCCION_EDITAR', 'Editar traducciones', 'Permite crear o modificar traducciones detectadas desde la UI', 'PERMISO'),
('BITACORA_VER', 'Ver bitacora', 'Permite consultar la bitacora del sistema', 'PERMISO'),
('AUDITORIA_CAMBIOS_VER', 'Ver auditoria de cambios', 'Permite consultar el historial de cambios de entidades auditadas', 'PERMISO');
GO

INSERT INTO ComponentePermisoRelacion (id_padre, id_hijo)
SELECT padre.id_componente, hijo.id_componente
FROM ComponentePermiso padre
INNER JOIN ComponentePermiso hijo
    ON hijo.codigo IN ('SEGURIDAD', 'AUDITORIA', 'IDIOMAS_TRADUCCIONES', 'USUARIO_VER', 'USUARIO_CREAR', 'USUARIO_EDITAR', 'USUARIO_INHABILITAR')
WHERE padre.codigo = 'ADMINISTRADOR';
GO

INSERT INTO ComponentePermisoRelacion (id_padre, id_hijo)
SELECT padre.id_componente, hijo.id_componente
FROM ComponentePermiso padre
INNER JOIN ComponentePermiso hijo
    ON hijo.codigo IN ('ROL_VER', 'ROL_CREAR', 'ROL_EDITAR', 'ROL_INHABILITAR', 'PERMISO_VER', 'PERMISO_ASIGNAR')
WHERE padre.codigo = 'SEGURIDAD';
GO

INSERT INTO ComponentePermisoRelacion (id_padre, id_hijo)
SELECT padre.id_componente, hijo.id_componente
FROM ComponentePermiso padre
INNER JOIN ComponentePermiso hijo
    ON hijo.codigo IN ('IDIOMA_VER', 'IDIOMA_CREAR', 'IDIOMA_EDITAR', 'TRADUCCION_VER', 'TRADUCCION_EDITAR')
WHERE padre.codigo = 'IDIOMAS_TRADUCCIONES';
GO

INSERT INTO ComponentePermisoRelacion (id_padre, id_hijo)
SELECT padre.id_componente, hijo.id_componente
FROM ComponentePermiso padre
INNER JOIN ComponentePermiso hijo
    ON hijo.codigo IN ('BITACORA_VER', 'AUDITORIA_CAMBIOS_VER')
WHERE padre.codigo = 'AUDITORIA';
GO

INSERT INTO Usuario
(
    id_idioma,
    nombre_usuario,
    email,
    password_hash,
    estado_usuario
)
SELECT TOP (1)
    id_idioma,
    'admin',
    'admin@tecnisalud.local',
    '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',
    'ACTIVO'
FROM Idioma
WHERE codigo = 'es-AR';
GO

INSERT INTO UsuarioComponentePermiso (id_usuario, id_componente)
SELECT u.id_usuario, c.id_componente
FROM Usuario u
INNER JOIN ComponentePermiso c
    ON c.codigo = 'ADMINISTRADOR'
WHERE u.nombre_usuario = 'admin';
GO

-- ============================================================
-- FIN DEL SCRIPT
-- ============================================================
