-- ============================================================
-- Tecni Salud - esquema base de PN1
-- Se conservan las tablas transversales de la aplicacion y el
-- alcance funcional del banco de sangre definido para PN1.
-- ============================================================

IF DB_ID(N'HemovidaGest') IS NULL
BEGIN
    EXEC(N'CREATE DATABASE HemovidaGest');
END
GO

USE HemovidaGest;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.Idioma
(
    id_idioma INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Idioma PRIMARY KEY,
    codigo VARCHAR(10) NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    estado_idioma VARCHAR(20) NOT NULL CONSTRAINT DF_Idioma_Estado DEFAULT 'ACTIVO',
    CONSTRAINT UQ_Idioma_Codigo UNIQUE (codigo),
    CONSTRAINT CK_Idioma_Estado CHECK (estado_idioma IN ('ACTIVO', 'INACTIVO'))
);
GO

CREATE TABLE dbo.Usuario
(
    id_usuario INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Usuario PRIMARY KEY,
    id_idioma INT NULL,
    nombre_usuario VARCHAR(100) NOT NULL,
    email VARCHAR(150) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    nombre VARCHAR(100) NULL,
    apellido VARCHAR(100) NULL,
    estado_usuario VARCHAR(20) NOT NULL CONSTRAINT DF_Usuario_Estado DEFAULT 'ACTIVO',
    intentos_login_fallidos INT NOT NULL CONSTRAINT DF_Usuario_Intentos DEFAULT 0,
    fecha_alta DATETIME NOT NULL CONSTRAINT DF_Usuario_FechaAlta DEFAULT GETDATE(),
    bloqueo_digitoverificador BIT NOT NULL CONSTRAINT DF_Usuario_BloqueoDV DEFAULT 0,
    dvh VARCHAR(64) NULL,
    CONSTRAINT UQ_Usuario_Nombre UNIQUE (nombre_usuario),
    CONSTRAINT UQ_Usuario_Email UNIQUE (email),
    CONSTRAINT CK_Usuario_Estado CHECK (estado_usuario IN ('ACTIVO', 'INACTIVO', 'BLOQUEADO')),
    CONSTRAINT CK_Usuario_Intentos CHECK (intentos_login_fallidos >= 0),
    CONSTRAINT FK_Usuario_Idioma FOREIGN KEY (id_idioma) REFERENCES dbo.Idioma(id_idioma)
);
GO

CREATE TABLE dbo.IdiomaEstadoHistorial
(
    id_idioma_estado_historial INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_IdiomaEstadoHistorial PRIMARY KEY,
    id_idioma INT NOT NULL,
    estado_anterior VARCHAR(20) NULL,
    estado_nuevo VARCHAR(20) NOT NULL,
    motivo VARCHAR(255) NULL,
    fecha_cambio DATETIME NOT NULL CONSTRAINT DF_IdiomaEstadoHistorial_Fecha DEFAULT GETDATE(),
    id_usuario_responsable INT NULL,
    CONSTRAINT FK_IdiomaEstadoHistorial_Idioma FOREIGN KEY (id_idioma) REFERENCES dbo.Idioma(id_idioma),
    CONSTRAINT FK_IdiomaEstadoHistorial_Usuario FOREIGN KEY (id_usuario_responsable) REFERENCES dbo.Usuario(id_usuario)
);
GO

CREATE TABLE dbo.DigitoVerificadorVertical
(
    id_digito_verificador_vertical INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DigitoVerificadorVertical PRIMARY KEY,
    entidad VARCHAR(100) NOT NULL,
    dvv VARCHAR(64) NOT NULL,
    fecha_calculo DATETIME NOT NULL CONSTRAINT DF_DVV_Fecha DEFAULT GETDATE(),
    CONSTRAINT UQ_DVV_Entidad UNIQUE (entidad)
);
GO

CREATE TABLE dbo.Auditoria
(
    dvh_integridad VARCHAR(64) NULL,
    id_auditoria INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Auditoria PRIMARY KEY,
    entidad NVARCHAR(100) NOT NULL,
    id_entidad INT NOT NULL,
    accion NVARCHAR(50) NOT NULL,
    id_usuario_actor INT NULL,
    identificador_usuario_actor NVARCHAR(255) NULL,
    fecha_evento DATETIME NOT NULL CONSTRAINT DF_Auditoria_Fecha DEFAULT GETDATE(),
    estado_anterior_json NVARCHAR(MAX) NULL,
    estado_nuevo_json NVARCHAR(MAX) NULL,
    cambios_json NVARCHAR(MAX) NOT NULL,
    CONSTRAINT FK_Auditoria_UsuarioActor FOREIGN KEY (id_usuario_actor) REFERENCES dbo.Usuario(id_usuario)
);
GO

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
    fecha_evento DATETIME NOT NULL CONSTRAINT DF_Bitacora_Fecha DEFAULT GETDATE(),
    CONSTRAINT FK_Bitacora_Usuario FOREIGN KEY (id_usuario) REFERENCES dbo.Usuario(id_usuario)
);
GO

CREATE TABLE dbo.Rol
(
    id_rol INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Rol PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    descripcion VARCHAR(255) NULL,
    estado_rol VARCHAR(20) NOT NULL CONSTRAINT DF_Rol_Estado DEFAULT 'ACTIVO',
    CONSTRAINT UQ_Rol_Nombre UNIQUE (nombre),
    CONSTRAINT CK_Rol_Estado CHECK (estado_rol IN ('ACTIVO', 'INACTIVO'))
);
GO

CREATE TABLE dbo.Permiso
(
    id_permiso INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Permiso PRIMARY KEY,
    codigo VARCHAR(100) NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    descripcion VARCHAR(255) NULL,
    modulo VARCHAR(100) NOT NULL,
    accion VARCHAR(100) NOT NULL,
    estado_permiso VARCHAR(20) NOT NULL CONSTRAINT DF_Permiso_Estado DEFAULT 'ACTIVO',
    CONSTRAINT UQ_Permiso_Codigo UNIQUE (codigo),
    CONSTRAINT CK_Permiso_Estado CHECK (estado_permiso IN ('ACTIVO', 'INACTIVO'))
);
GO

CREATE TABLE dbo.UsuarioRol
(
    id_usuario_rol INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_UsuarioRol PRIMARY KEY,
    id_usuario INT NOT NULL,
    id_rol INT NOT NULL,
    fecha_asignacion DATETIME NOT NULL CONSTRAINT DF_UsuarioRol_Fecha DEFAULT GETDATE(),
    estado_usuario_rol VARCHAR(20) NOT NULL CONSTRAINT DF_UsuarioRol_Estado DEFAULT 'ACTIVO',
    CONSTRAINT UQ_UsuarioRol UNIQUE (id_usuario, id_rol),
    CONSTRAINT CK_UsuarioRol_Estado CHECK (estado_usuario_rol IN ('ACTIVO', 'INACTIVO')),
    CONSTRAINT FK_UsuarioRol_Usuario FOREIGN KEY (id_usuario) REFERENCES dbo.Usuario(id_usuario),
    CONSTRAINT FK_UsuarioRol_Rol FOREIGN KEY (id_rol) REFERENCES dbo.Rol(id_rol)
);
GO

CREATE TABLE dbo.RolPermiso
(
    id_rol_permiso INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RolPermiso PRIMARY KEY,
    id_rol INT NOT NULL,
    id_permiso INT NOT NULL,
    CONSTRAINT UQ_RolPermiso UNIQUE (id_rol, id_permiso),
    CONSTRAINT FK_RolPermiso_Rol FOREIGN KEY (id_rol) REFERENCES dbo.Rol(id_rol),
    CONSTRAINT FK_RolPermiso_Permiso FOREIGN KEY (id_permiso) REFERENCES dbo.Permiso(id_permiso)
);
GO

CREATE TABLE dbo.ComponentePermiso
(
    id_componente INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ComponentePermiso PRIMARY KEY,
    codigo VARCHAR(100) NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    descripcion VARCHAR(255) NULL,
    tipo VARCHAR(20) NOT NULL,
    estado_componente VARCHAR(20) NOT NULL CONSTRAINT DF_ComponentePermiso_Estado DEFAULT 'ACTIVO',
    CONSTRAINT UQ_ComponentePermiso_Codigo UNIQUE (codigo),
    CONSTRAINT CK_ComponentePermiso_Tipo CHECK (tipo IN ('FAMILIA', 'PERMISO')),
    CONSTRAINT CK_ComponentePermiso_Estado CHECK (estado_componente IN ('ACTIVO', 'INACTIVO'))
);
GO

CREATE TABLE dbo.ComponentePermisoRelacion
(
    id_relacion INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ComponentePermisoRelacion PRIMARY KEY,
    id_padre INT NOT NULL,
    id_hijo INT NOT NULL,
    CONSTRAINT UQ_ComponentePermisoRelacion UNIQUE (id_padre, id_hijo),
    CONSTRAINT CK_ComponentePermisoRelacion_NoAutoReferencia CHECK (id_padre <> id_hijo),
    CONSTRAINT FK_ComponentePermisoRelacion_Padre FOREIGN KEY (id_padre) REFERENCES dbo.ComponentePermiso(id_componente),
    CONSTRAINT FK_ComponentePermisoRelacion_Hijo FOREIGN KEY (id_hijo) REFERENCES dbo.ComponentePermiso(id_componente)
);
GO

CREATE TABLE dbo.UsuarioComponentePermiso
(
    id_usuario_componente INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_UsuarioComponentePermiso PRIMARY KEY,
    id_usuario INT NOT NULL,
    id_componente INT NOT NULL,
    fecha_asignacion DATETIME NOT NULL CONSTRAINT DF_UsuarioComponentePermiso_Fecha DEFAULT GETDATE(),
    estado_usuario_componente VARCHAR(20) NOT NULL CONSTRAINT DF_UsuarioComponentePermiso_Estado DEFAULT 'ACTIVO',
    CONSTRAINT UQ_UsuarioComponentePermiso UNIQUE (id_usuario, id_componente),
    CONSTRAINT CK_UsuarioComponentePermiso_Estado CHECK (estado_usuario_componente IN ('ACTIVO', 'INACTIVO')),
    CONSTRAINT FK_UsuarioComponentePermiso_Usuario FOREIGN KEY (id_usuario) REFERENCES dbo.Usuario(id_usuario),
    CONSTRAINT FK_UsuarioComponentePermiso_Componente FOREIGN KEY (id_componente) REFERENCES dbo.ComponentePermiso(id_componente)
);
GO

CREATE TABLE dbo.Etiqueta
(
    id_etiqueta INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Etiqueta PRIMARY KEY,
    clave VARCHAR(150) NOT NULL,
    descripcion VARCHAR(255) NULL,
    CONSTRAINT UQ_Etiqueta_Clave UNIQUE (clave)
);
GO

CREATE TABLE dbo.Traduccion
(
    id_traduccion INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Traduccion PRIMARY KEY,
    id_etiqueta INT NOT NULL,
    id_idioma INT NOT NULL,
    texto NVARCHAR(500) NOT NULL,
    CONSTRAINT UQ_Traduccion UNIQUE (id_etiqueta, id_idioma),
    CONSTRAINT FK_Traduccion_Etiqueta FOREIGN KEY (id_etiqueta) REFERENCES dbo.Etiqueta(id_etiqueta),
    CONSTRAINT FK_Traduccion_Idioma FOREIGN KEY (id_idioma) REFERENCES dbo.Idioma(id_idioma)
);
GO

-- ============================================================
-- PN1: registro y trazabilidad de donantes, donaciones y unidades
-- ============================================================

CREATE TABLE dbo.Donante
(
    dvh VARCHAR(64) NULL,
    id_donante INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Donante PRIMARY KEY,
    documento VARCHAR(30) NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    apellido VARCHAR(100) NOT NULL,
    fecha_nacimiento DATE NULL,
    telefono VARCHAR(50) NULL,
    email VARCHAR(150) NULL,
    domicilio VARCHAR(255) NULL,
    estado_donante VARCHAR(20) NOT NULL CONSTRAINT DF_Donante_Estado DEFAULT 'ACTIVO',
    fecha_alta DATETIME NOT NULL CONSTRAINT DF_Donante_FechaAlta DEFAULT GETDATE(),
    id_usuario_alta INT NOT NULL,
    CONSTRAINT UQ_Donante_Documento UNIQUE (documento),
    CONSTRAINT CK_Donante_Estado CHECK (estado_donante IN ('ACTIVO', 'INACTIVO')),
    CONSTRAINT FK_Donante_UsuarioAlta FOREIGN KEY (id_usuario_alta) REFERENCES dbo.Usuario(id_usuario)
);
GO

CREATE TABLE dbo.Donacion
(
    dvh VARCHAR(64) NULL,
    id_donacion INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Donacion PRIMARY KEY,
    id_donante INT NOT NULL,
    fecha_donacion DATETIME NOT NULL,
    cantidad_unidades INT NOT NULL,
    tipo_componente VARCHAR(100) NOT NULL,
    fecha_vencimiento DATE NOT NULL,
    observaciones VARCHAR(500) NULL,
    id_usuario_responsable INT NOT NULL,
    fecha_alta DATETIME NOT NULL CONSTRAINT DF_Donacion_FechaAlta DEFAULT GETDATE(),
    CONSTRAINT CK_Donacion_Cantidad CHECK (cantidad_unidades > 0),
    CONSTRAINT CK_Donacion_Vencimiento CHECK (fecha_vencimiento >= CONVERT(DATE, fecha_donacion)),
    CONSTRAINT FK_Donacion_Donante FOREIGN KEY (id_donante) REFERENCES dbo.Donante(id_donante),
    CONSTRAINT FK_Donacion_UsuarioResponsable FOREIGN KEY (id_usuario_responsable) REFERENCES dbo.Usuario(id_usuario)
);
GO

CREATE TABLE dbo.Unidad
(
    dvh VARCHAR(64) NULL,
    id_unidad INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Unidad PRIMARY KEY,
    codigo_identificacion VARCHAR(50) NOT NULL,
    id_donacion INT NOT NULL,
    tipo_componente VARCHAR(100) NULL,
    fecha_vencimiento DATE NOT NULL,
    grupo_sanguineo VARCHAR(5) NULL,
    factor_rh VARCHAR(5) NULL,
    observaciones VARCHAR(500) NULL,
    estado_operativo VARCHAR(20) NOT NULL CONSTRAINT DF_Unidad_Estado DEFAULT 'EN_REVISION',
    fecha_alta DATETIME NOT NULL CONSTRAINT DF_Unidad_FechaAlta DEFAULT GETDATE(),
    fecha_ultima_modificacion DATETIME NOT NULL CONSTRAINT DF_Unidad_FechaModificacion DEFAULT GETDATE(),
    id_usuario_ultima_modificacion INT NOT NULL,
    CONSTRAINT UQ_Unidad_Codigo UNIQUE (codigo_identificacion),
    CONSTRAINT CK_Unidad_Estado CHECK (estado_operativo IN ('EN_REVISION', 'LIBERADA', 'BLOQUEADA', 'DESCARTADA')),
    CONSTRAINT CK_Unidad_Grupo CHECK (grupo_sanguineo IS NULL OR grupo_sanguineo IN ('A', 'B', 'AB', 'O')),
    CONSTRAINT CK_Unidad_Rh CHECK (factor_rh IS NULL OR factor_rh IN ('+', '-')),
    CONSTRAINT FK_Unidad_Donacion FOREIGN KEY (id_donacion) REFERENCES dbo.Donacion(id_donacion),
    CONSTRAINT FK_Unidad_UsuarioModificacion FOREIGN KEY (id_usuario_ultima_modificacion) REFERENCES dbo.Usuario(id_usuario)
);
GO

CREATE TABLE dbo.MovimientoUnidad
(
    dvh VARCHAR(64) NULL,
    id_movimiento_unidad INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MovimientoUnidad PRIMARY KEY,
    id_unidad INT NOT NULL,
    tipo_movimiento VARCHAR(30) NOT NULL,
    estado_anterior VARCHAR(20) NULL,
    estado_nuevo VARCHAR(20) NOT NULL,
    observacion VARCHAR(500) NULL,
    fecha_movimiento DATETIME NOT NULL CONSTRAINT DF_MovimientoUnidad_Fecha DEFAULT GETDATE(),
    id_usuario INT NOT NULL,
    CONSTRAINT CK_MovimientoUnidad_Tipo CHECK (tipo_movimiento IN ('REGISTRO', 'CLASIFICACION', 'LIBERACION', 'BLOQUEO', 'DESCARTE', 'RESTAURACION')),
    CONSTRAINT CK_MovimientoUnidad_EstadoNuevo CHECK (estado_nuevo IN ('EN_REVISION', 'LIBERADA', 'BLOQUEADA', 'DESCARTADA')),
    CONSTRAINT FK_MovimientoUnidad_Unidad FOREIGN KEY (id_unidad) REFERENCES dbo.Unidad(id_unidad),
    CONSTRAINT FK_MovimientoUnidad_Usuario FOREIGN KEY (id_usuario) REFERENCES dbo.Usuario(id_usuario)
);
GO

CREATE INDEX IX_Auditoria_Entidad_IdEntidad_Fecha ON dbo.Auditoria(entidad, id_entidad, fecha_evento DESC);
CREATE INDEX IX_Bitacora_Fecha_Modulo ON dbo.Bitacora(fecha_evento DESC, modulo);
CREATE INDEX IX_Donante_Nombre ON dbo.Donante(apellido, nombre);
CREATE INDEX IX_Donacion_Donante_Fecha ON dbo.Donacion(id_donante, fecha_donacion DESC);
CREATE INDEX IX_Unidad_Estado_Vencimiento ON dbo.Unidad(estado_operativo, fecha_vencimiento);
CREATE INDEX IX_MovimientoUnidad_Unidad_Fecha ON dbo.MovimientoUnidad(id_unidad, fecha_movimiento DESC);
CREATE UNIQUE INDEX UX_UsuarioComponentePermiso_UsuarioActivo
    ON dbo.UsuarioComponentePermiso(id_usuario)
    WHERE estado_usuario_componente = 'ACTIVO';
GO

-- ============================================================
-- Datos base transversales y catálogo inicial de UI
-- ============================================================

INSERT INTO dbo.Idioma (codigo, nombre, estado_idioma)
VALUES ('es-AR', 'Español Argentina', 'ACTIVO'), ('en-US', 'English', 'ACTIVO');
GO

DECLARE @Catalogo TABLE
(
    clave VARCHAR(150) NOT NULL PRIMARY KEY,
    descripcion VARCHAR(255) NULL,
    texto_es NVARCHAR(500) NOT NULL,
    texto_en NVARCHAR(500) NOT NULL
);

INSERT INTO @Catalogo (clave, descripcion, texto_es, texto_en) VALUES
('MAIN_TITLE', 'Titulo de la ventana principal', 'Tecni Salud', 'Tecni Salud'),
('MAIN_USER', 'Usuario autenticado', 'Usuario: {0}', 'User: {0}'),
('MAIN_NO_SESSION', 'Sin sesion', 'Sin sesión activa', 'No active session'),
('MENU_AUDIT', 'Menu bitacora', 'Bitácora', 'Logbook'),
('MENU_CHANGE_AUDIT', 'Menu auditoria', 'Auditoría de cambios', 'Change audit'),
('MENU_USERS', 'Menu usuarios', 'Usuarios', 'Users'),
('MENU_PERMISSIONS', 'Menu permisos', 'Permisos', 'Permissions'),
('MENU_ROLES', 'Menu roles', 'Roles', 'Roles'),
('MENU_LANGUAGES', 'Menu idiomas', 'Idiomas y traducciones', 'Languages and translations'),
('MENU_LOGOUT', 'Menu salir', 'Cerrar sesión', 'Log out'),
('MENU_BLOOD_BANK', 'Menu banco de sangre', 'Banco de sangre', 'Blood bank'),
('MENU_DONORS', 'Menu donantes', 'Donantes', 'Donors'),
('MENU_DONATIONS', 'Menu donaciones', 'Donaciones', 'Donations'),
('MENU_UNITS', 'Menu unidades', 'Unidades', 'Units'),
('LANGUAGE_SELECTOR', 'Selector de idioma', 'Idioma', 'Language'),
('AUDIT_TITLE', 'Titulo de bitacora', 'Bitácora del sistema', 'System logbook'),
('AUDIT_DESCRIPTION', 'Descripcion de bitacora', 'Eventos operativos y de seguridad registrados.', 'Recorded operational and security events.'),
('AUDIT_EMPTY', 'Bitacora vacia', 'No hay eventos registrados.', 'There are no recorded events.'),
('AUDIT_COUNT', 'Cantidad de eventos', '{0} evento(s) registrado(s).', '{0} event(s) registered.'),
('AUDIT_FILTERS', 'Filtros', 'Filtros', 'Filters'),
('AUDIT_FILTER_FROM', 'Desde', 'Desde', 'From'),
('AUDIT_FILTER_TO', 'Hasta', 'Hasta', 'To'),
('AUDIT_FILTER_USER', 'Usuario', 'Usuario', 'User'),
('AUDIT_FILTER_MODULE', 'Modulo', 'Módulo', 'Module'),
('AUDIT_FILTER_ACTION', 'Accion', 'Acción', 'Action'),
('AUDIT_FILTER_LEVEL', 'Nivel', 'Nivel', 'Level'),
('AUDIT_FILTER_DESCRIPTION', 'Descripcion', 'Descripción', 'Description'),
('CHANGE_AUDIT_TITLE', 'Titulo auditoria cambios', 'Auditoría de cambios', 'Change audit'),
('CHANGE_AUDIT_DESCRIPTION', 'Descripcion auditoria cambios', 'Historial de cambios registrados sobre las entidades auditadas.', 'History of recorded changes on audited entities.'),
('CHANGE_AUDIT_EMPTY', 'Auditoria cambios vacia', 'No hay cambios registrados.', 'There are no recorded changes.'),
('CHANGE_AUDIT_COUNT', 'Cantidad cambios', '{0} cambio(s) registrado(s).', '{0} change(s) registered.'),
('CHANGE_AUDIT_USER', 'Usuario auditado', 'Usuario auditado', 'Audited user'),
('CHANGE_AUDIT_PREVIOUS_STATE', 'Estado anterior', 'Estado anterior', 'Previous state'),
('CHANGE_AUDIT_NEW_STATE', 'Estado nuevo', 'Estado nuevo', 'New state'),
('FILTER_ALL', 'Todos', 'Todos', 'All'),
('FILTER_ALL_ACTIONS', 'Todas las acciones', 'Todas', 'All'),
('BITACORA_MODULE_SECURITY', 'Modulo seguridad', 'Seguridad', 'Security'),
('BITACORA_MODULE_BLOOD_BANK', 'Modulo banco de sangre', 'Banco de sangre', 'Blood bank'),
('BITACORA_ACTION_LOGIN_SUCCESS', 'Login exitoso', 'Inicio de sesión exitoso', 'Successful login'),
('BITACORA_ACTION_LOGIN_FAILURE', 'Login fallido', 'Inicio de sesión fallido', 'Failed login'),
('BITACORA_ACTION_REGISTER_FAILURE', 'Registro fallido', 'Registro fallido', 'Failed registration'),
('BITACORA_ACTION_DONOR_REGISTERED', 'Donante registrado', 'Donante registrado', 'Donor registered'),
('BITACORA_ACTION_DONATION_REGISTERED', 'Donacion registrada', 'Donación registrada', 'Donation registered'),
('BITACORA_ACTION_UNITS_GENERATED', 'Unidades generadas', 'Unidades generadas', 'Units generated'),
('BITACORA_ACTION_UNIT_CLASSIFIED', 'Unidad clasificada', 'Unidad clasificada', 'Unit classified'),
('BITACORA_ACTION_UNIT_RELEASED', 'Unidad liberada', 'Unidad liberada', 'Unit released'),
('BITACORA_ACTION_UNIT_BLOCKED', 'Unidad bloqueada', 'Unidad bloqueada', 'Unit blocked'),
('BITACORA_ACTION_UNIT_DISCARDED', 'Unidad descartada', 'Unidad descartada', 'Unit discarded'),
('BITACORA_ACTION_OPERATION_FAILURE', 'Operacion fallida', 'Operación fallida', 'Operation failed'),
('BITACORA_LEVEL_INFORMATION', 'Nivel informacion', 'Información', 'Information'),
('BITACORA_LEVEL_WARNING', 'Nivel advertencia', 'Advertencia', 'Warning'),
('BITACORA_LEVEL_ERROR', 'Nivel error', 'Error', 'Error'),
('BTN_ADD', 'Boton agregar', 'Agregar', 'Add'),
('BTN_CREATE', 'Boton crear', 'Crear', 'Create'),
('BTN_CREATE_ROLE', 'Boton crear rol', 'Crear rol', 'Create role'),
('BTN_DISABLE', 'Boton inhabilitar', 'Inhabilitar', 'Disable'),
('BTN_NEW', 'Boton nuevo', 'Nuevo', 'New'),
('BTN_SAVE', 'Boton guardar', 'Guardar', 'Save'),
('BTN_REFRESH', 'Boton actualizar', 'Actualizar', 'Refresh'),
('BTN_SEARCH', 'Boton buscar', 'Buscar', 'Search'),
('BTN_CLEAR_FILTERS', 'Boton limpiar', 'Limpiar', 'Clear'),
('BTN_RECALCULATE_DV', 'Recalcular DV', 'Recalcular DV', 'Recalculate DV'),
('BTN_REMOVE_SELECTED', 'Quitar seleccionado', 'Quitar seleccionado', 'Remove selected'),
('BTN_REMOVE_FROM', 'Quitar desde familia', 'Quitar desde {0}', 'Remove from {0}'),
('BTN_CREATE_LABEL', 'Crear etiqueta', 'Crear etiqueta', 'Create label'),
('FIELD_USER', 'Campo usuario', 'Usuario', 'User'),
('FIELD_EMAIL', 'Campo email', 'Email', 'Email'),
('FIELD_NAME', 'Campo nombre', 'Nombre', 'Name'),
('FIELD_LASTNAME', 'Campo apellido', 'Apellido', 'Last name'),
('FIELD_NEW_PASSWORD', 'Campo contrasena', 'Contraseña nueva', 'New password'),
('FIELD_STATUS', 'Campo estado', 'Estado', 'Status'),
('GRID_ID', 'Columna id', 'Id', 'Id'),
('GRID_DATE', 'Columna fecha', 'Fecha', 'Date'),
('GRID_USER_ID', 'Columna id usuario', 'Id usuario', 'User id'),
('GRID_USER', 'Columna usuario', 'Usuario', 'User'),
('GRID_EMAIL', 'Columna email', 'Email', 'Email'),
('GRID_STATUS', 'Columna estado', 'Estado', 'Status'),
('GRID_DV_BLOCK', 'Columna bloqueo DV', 'Bloqueo DV', 'DV block'),
('GRID_MODULE', 'Columna modulo', 'Módulo', 'Module'),
('GRID_ACTION', 'Columna accion', 'Acción', 'Action'),
('GRID_LEVEL', 'Columna nivel', 'Nivel', 'Level'),
('GRID_DESCRIPTION', 'Columna descripcion', 'Descripción', 'Description'),
('GRID_DEVICE', 'Columna equipo', 'Equipo', 'Device'),
('GRID_ENTITY', 'Columna entidad', 'Entidad', 'Entity'),
('GRID_ENTITY_ID', 'Columna id entidad', 'Id entidad', 'Entity id'),
('GRID_FIELD', 'Columna campo', 'Campo', 'Field'),
('GRID_OLD_VALUE', 'Columna valor anterior', 'Valor anterior', 'Old value'),
('GRID_NEW_VALUE', 'Columna valor nuevo', 'Valor nuevo', 'New value'),
('SECURITY_ACCESS_DENIED', 'Acceso denegado', 'No tenés permisos para acceder a esta funcionalidad.', 'You do not have permission to access this feature.'),
('NO_PERMISSIONS_ASSIGNED', 'Sin permisos', 'No tenés permisos asignados.', 'You have no permissions assigned.'),
('SECURITY_ROLE_CREATE_DENIED', 'Crear roles denegado', 'No tenés permisos para crear roles.', 'You do not have permission to create roles.'),
('SECURITY_ROLE_EDIT_DENIED', 'Editar roles denegado', 'No tenés permisos para modificar roles.', 'You do not have permission to modify roles.'),
('SECURITY_LANGUAGE_CREATE_DENIED', 'Crear idiomas denegado', 'No tenés permisos para crear idiomas.', 'You do not have permission to create languages.'),
('SECURITY_LANGUAGE_EDIT_DENIED', 'Editar idiomas denegado', 'No tenés permisos para modificar idiomas.', 'You do not have permission to modify languages.'),
('SECURITY_TRANSLATION_EDIT_DENIED', 'Editar traducciones denegado', 'No tenés permisos para modificar traducciones.', 'You do not have permission to modify translations.'),
('USERS_TITLE', 'Titulo usuarios', 'Usuarios', 'Users'),
('USERS_DESCRIPTION', 'Descripcion usuarios', 'Administración de usuarios y permisos asignados.', 'User administration and assigned permissions.'),
('USERS_DETAIL', 'Detalle usuario', 'Detalle de usuario', 'User detail'),
('USERS_CREATE_MODE', 'Modo crear usuario', 'Crear usuario', 'Create user'),
('USERS_EDIT_MODE', 'Modo editar usuario', 'Editar usuario', 'Edit user'),
('USER_ROLES', 'Roles de usuario', 'Roles del usuario', 'User roles'),
('USER_ROLE_SELECT_HELP', 'Ayuda roles usuario', 'Seleccioná un usuario para asignarle un rol.', 'Select a user to assign a role.'),
('USER_ROLE_SELECT_ONE', 'Seleccionar rol', 'Elegí un rol y guardá.', 'Choose a role and save.'),
('USER_ROLE_EMPTY', 'Sin roles', 'No hay roles cargados.', 'There are no roles loaded.'),
('USER_ROLE_EDIT_DENIED', 'Editar roles usuario denegado', 'No tenés permisos para modificar roles de usuarios.', 'You do not have permission to modify user roles.'),
('ROLES_TITLE', 'Titulo roles', 'Roles y permisos', 'Roles and permissions'),
('ROLES_DESCRIPTION', 'Descripcion roles', 'Administración de familias y relaciones de permisos.', 'Administration of permission families and relations.'),
('ROLES_STRUCTURE', 'Estructura roles', 'Estructura', 'Structure'),
('ROLES_ADMIN', 'Administracion roles', 'Administración', 'Administration'),
('ROLE_CODE', 'Codigo rol', 'Código', 'Code'),
('ROLE_SELECTED_FAMILY', 'Familia seleccionada', 'Familia seleccionada', 'Selected family'),
('ROLE_CHILD_COMPONENT', 'Componente hijo', 'Permiso o familia hija', 'Child permission or family'),
('ROLE_SELECT_FAMILY', 'Seleccionar familia', 'Seleccioná una familia.', 'Select a family.'),
('ROLE_SELECT_CHILD', 'Seleccionar hijo', 'Seleccioná un permiso o familia.', 'Select a permission or family.'),
('ROLE_SELECT_FAMILY_AND_COMPONENT', 'Seleccionar familia y componente', 'Seleccioná la familia y el componente.', 'Select the family and component.'),
('ROLE_CREATED', 'Rol creado', 'Rol creado correctamente.', 'Role created successfully.'),
('ROLE_CREATE_ERROR', 'Error crear rol', 'No se pudo crear el rol.', 'Could not create the role.'),
('ROLE_RELATION_ADDED', 'Relacion agregada', 'Relación agregada correctamente.', 'Relation added successfully.'),
('ROLE_RELATION_ADD_ERROR', 'Error agregar relacion', 'No se pudo agregar la relación.', 'Could not add the relation.'),
('ROLE_RELATION_REMOVED', 'Relacion quitada', 'Relación quitada correctamente.', 'Relation removed successfully.'),
('ROLE_RELATION_REMOVE_ERROR', 'Error quitar relacion', 'No se pudo quitar la relación.', 'Could not remove the relation.'),
('ROLE_RELATION_IDENTIFY_ERROR', 'Error identificar relacion', 'No se pudo identificar la relación.', 'Could not identify the relation.'),
('ROLE_SELF_REFERENCE_ERROR', 'Autoreferencia', 'Un rol no puede contenerse a sí mismo.', 'A role cannot be added to itself.'),
('ROLE_INVALID_PARENT', 'Padre invalido', 'El padre debe ser una familia activa.', 'The parent must be an active family.'),
('ROLE_INVALID_CHILD', 'Hijo invalido', 'El componente hijo no existe o está inactivo.', 'The child component does not exist or is inactive.'),
('ROLE_CYCLE_ERROR', 'Ciclo detectado', 'La relación generaría un ciclo.', 'The relation would create a cycle.'),
('PERMISSIONS_TITLE', 'Titulo permisos', 'Permisos', 'Permissions'),
('PERMISSIONS_DESCRIPTION', 'Descripcion permisos', 'Componentes disponibles para asignar.', 'Available components to assign.'),
('PERMISSIONS_TREE', 'Arbol permisos', 'Árbol de permisos', 'Permission tree'),
('PERMISSIONS_AVAILABLE', 'Permisos disponibles', 'Componentes disponibles', 'Available components'),
('COMPONENT_SELECTED', 'Componente seleccionado', 'Componente seleccionado', 'Selected component'),
('COMPONENT_TREE', 'Arbol de componentes', 'Componentes', 'Components'),
('LANGUAGES_TITLE', 'Titulo idiomas', 'Idiomas y traducciones', 'Languages and translations'),
('LANGUAGES_DESCRIPTION', 'Descripcion idiomas', 'Administración de idiomas, etiquetas y textos visibles.', 'Administration of languages, labels and visible texts.'),
('LANGUAGE_DETAIL', 'Detalle idioma', 'Idioma', 'Language'),
('LABEL_DETAIL', 'Detalle etiqueta', 'Etiqueta', 'Label'),
('TRANSLATION_DETAIL', 'Detalle traduccion', 'Traducción', 'Translation'),
('LANGUAGE_CODE', 'Codigo idioma', 'Código', 'Code'),
('LANGUAGE_ACTIVE', 'Idioma activo', 'Activo', 'Active'),
('LABEL_KEY', 'Clave etiqueta', 'Clave', 'Key'),
('LABEL_TAG', 'Etiqueta', 'Etiqueta', 'Label'),
('TRANSLATION_TEXT', 'Texto traduccion', 'Traducción', 'Translation'),
('LANGUAGE_SAVED', 'Idioma guardado', 'Idioma guardado correctamente.', 'Language saved successfully.'),
('LABEL_SAVED', 'Etiqueta guardada', 'Etiqueta guardada correctamente.', 'Label saved successfully.'),
('TRANSLATION_SAVED', 'Traduccion guardada', 'Traducción guardada correctamente.', 'Translation saved successfully.'),
('SAVE_ERROR', 'Error guardar', 'No se pudo guardar.', 'Could not save.'),
('DONORS_TITLE', 'Titulo donantes', 'Donantes', 'Donors'),
('DONORS_DESCRIPTION', 'Descripcion donantes', 'Registro y consulta de donantes habilitados.', 'Registration and lookup of active donors.'),
('DONORS_DETAIL', 'Detalle donante', 'Datos del donante', 'Donor details'),
('DONOR_DOCUMENT', 'Documento donante', 'Documento', 'Document'),
('DONOR_NAME', 'Nombre donante', 'Nombre', 'First name'),
('DONOR_LASTNAME', 'Apellido donante', 'Apellido', 'Last name'),
('DONOR_BIRTHDATE', 'Nacimiento donante', 'Fecha de nacimiento', 'Birth date'),
('DONOR_PHONE', 'Telefono donante', 'Teléfono', 'Phone'),
('DONOR_EMAIL', 'Email donante', 'Email', 'Email'),
('DONOR_ADDRESS', 'Domicilio donante', 'Domicilio', 'Address'),
('DONOR_STATUS', 'Estado donante', 'Estado', 'Status'),
('DONOR_REGISTERED', 'Donante registrado', 'Donante registrado correctamente.', 'Donor registered successfully.'),
('DONOR_DUPLICATE', 'Donante duplicado', 'Ya existe un donante con ese documento.', 'A donor with that document already exists.'),
('DONOR_INVALID', 'Donante invalido', 'Completá documento, nombre y apellido.', 'Document, first name and last name are required.'),
('DONOR_ERROR', 'Error donante', 'No se pudo registrar el donante.', 'Could not register the donor.'),
('DONATION_TITLE', 'Titulo donaciones', 'Donaciones', 'Donations'),
('DONATION_DESCRIPTION', 'Descripcion donaciones', 'Registro de donaciones y generación de unidades en revisión.', 'Register donations and generate units under review.'),
('DONATION_DETAIL', 'Detalle donacion', 'Datos de la donación', 'Donation details'),
('DONATION_DONOR', 'Donante donacion', 'Donante', 'Donor'),
('DONATION_DATE', 'Fecha donacion', 'Fecha de donación', 'Donation date'),
('DONATION_QUANTITY', 'Cantidad unidades', 'Cantidad de unidades', 'Number of units'),
('DONATION_COMPONENT', 'Componente donacion', 'Componente informado', 'Reported component'),
('DONATION_EXPIRATION', 'Vencimiento donacion', 'Vencimiento', 'Expiration date'),
('DONATION_OBSERVATIONS', 'Observaciones donacion', 'Observaciones', 'Notes'),
('DONATION_REGISTERED', 'Donacion registrada', 'Donación registrada y unidades generadas.', 'Donation registered and units generated.'),
('DONATION_INVALID', 'Donacion invalida', 'Completá los datos de la donación y verificá el vencimiento.', 'Complete the donation data and verify the expiration date.'),
('DONATION_ERROR', 'Error donacion', 'No se pudo registrar la donación.', 'Could not register the donation.'),
('UNITS_TITLE', 'Titulo unidades', 'Unidades', 'Units'),
('UNITS_DESCRIPTION', 'Descripcion unidades', 'Clasificación, liberación, bloqueo y descarte de unidades.', 'Classify, release, block and discard units.'),
('UNIT_DETAIL', 'Detalle unidad', 'Detalle de unidad', 'Unit details'),
('UNIT_CODE', 'Codigo unidad', 'Código', 'Code'),
('UNIT_DONOR', 'Donante unidad', 'Donante', 'Donor'),
('UNIT_COMPONENT', 'Componente unidad', 'Componente', 'Component'),
('UNIT_EXPIRATION', 'Vencimiento unidad', 'Vencimiento', 'Expiration'),
('UNIT_BLOOD_GROUP', 'Grupo sanguineo', 'Grupo sanguíneo', 'Blood group'),
('UNIT_RH', 'Factor RH', 'Factor Rh', 'Rh factor'),
('UNIT_STATUS', 'Estado unidad', 'Estado operativo', 'Operational status'),
('UNIT_OBSERVATIONS', 'Observaciones unidad', 'Observaciones', 'Notes'),
('UNIT_CLASSIFY', 'Clasificar unidad', 'Clasificar', 'Classify'),
('UNIT_RELEASE', 'Liberar unidad', 'Liberar', 'Release'),
('UNIT_BLOCK', 'Bloquear unidad', 'Bloquear', 'Block'),
('UNIT_DISCARD', 'Descartar unidad', 'Descartar', 'Discard'),
('UNIT_CLASSIFIED', 'Unidad clasificada', 'Unidad clasificada correctamente.', 'Unit classified successfully.'),
('UNIT_RELEASED', 'Unidad liberada', 'Unidad liberada correctamente.', 'Unit released successfully.'),
('UNIT_BLOCKED', 'Unidad bloqueada', 'Unidad bloqueada correctamente.', 'Unit blocked successfully.'),
('UNIT_DISCARDED', 'Unidad descartada', 'Unidad descartada correctamente.', 'Unit discarded successfully.'),
('UNIT_INVALID', 'Unidad invalida', 'Completá los datos requeridos de la unidad.', 'Complete the required unit data.'),
('UNIT_CONFLICT', 'Conflicto unidad', 'La unidad no está en un estado válido para esta operación.', 'The unit is not in a valid state for this operation.'),
('UNIT_ERROR', 'Error unidad', 'No se pudo actualizar la unidad.', 'Could not update the unit.'),
('UNIT_STATE_EN_REVISION', 'En revision', 'En revisión', 'Under review'),
('UNIT_STATE_LIBERADA', 'Liberada', 'Liberada', 'Released'),
('UNIT_STATE_BLOQUEADA', 'Bloqueada', 'Bloqueada', 'Blocked'),
('UNIT_STATE_DESCARTADA', 'Descartada', 'Descartada', 'Discarded'),
('STATUS_ACTIVE', 'Activo', 'Activo', 'Active'),
('STATUS_INACTIVE', 'Inactivo', 'Inactivo', 'Inactive'),
('BTN_REGISTER', 'Boton registrar', 'Registrar', 'Register'),
('BTN_CLEAR', 'Boton limpiar', 'Limpiar', 'Clear'),
('OPERATION_NOT_AUTHORIZED', 'Operacion no autorizada', 'No tenés permisos para realizar esta operación.', 'You are not authorized to perform this operation.');

INSERT INTO dbo.Etiqueta (clave, descripcion)
SELECT clave, descripcion FROM @Catalogo;

INSERT INTO dbo.Traduccion (id_etiqueta, id_idioma, texto)
SELECT e.id_etiqueta, i.id_idioma, c.texto_es
FROM @Catalogo c
INNER JOIN dbo.Etiqueta e ON e.clave = c.clave
INNER JOIN dbo.Idioma i ON i.codigo = 'es-AR';

INSERT INTO dbo.Traduccion (id_etiqueta, id_idioma, texto)
SELECT e.id_etiqueta, i.id_idioma, c.texto_en
FROM @Catalogo c
INNER JOIN dbo.Etiqueta e ON e.clave = c.clave
INNER JOIN dbo.Idioma i ON i.codigo = 'en-US';
GO

-- Roles funcionales definidos para PN1.
INSERT INTO dbo.Rol (nombre, descripcion, estado_rol) VALUES
('Administrador', 'Acceso total a la administracion y al negocio.', 'ACTIVO'),
('Personal de extracción', 'Registra donantes y donaciones.', 'ACTIVO'),
('Operador autorizado', 'Clasifica unidades en revisión.', 'ACTIVO'),
('Responsable autorizado', 'Libera, bloquea o descarta unidades clasificadas.', 'ACTIVO');
GO

DECLARE @Permisos TABLE
(
    codigo VARCHAR(100) NOT NULL PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    descripcion VARCHAR(255) NULL,
    modulo VARCHAR(100) NOT NULL,
    accion VARCHAR(100) NOT NULL
);

INSERT INTO @Permisos (codigo, nombre, descripcion, modulo, accion) VALUES
('USUARIO_VER', 'Ver usuarios', 'Accede al modulo de usuarios.', 'SEGURIDAD', 'VER'),
('USUARIO_CREAR', 'Crear usuarios', 'Crea usuarios.', 'SEGURIDAD', 'CREAR'),
('USUARIO_EDITAR', 'Editar usuarios', 'Modifica usuarios.', 'SEGURIDAD', 'EDITAR'),
('USUARIO_INHABILITAR', 'Inhabilitar usuarios', 'Inhabilita usuarios.', 'SEGURIDAD', 'INHABILITAR'),
('ROL_VER', 'Ver roles', 'Accede al modulo de roles.', 'SEGURIDAD', 'VER'),
('ROL_CREAR', 'Crear roles', 'Crea familias de permisos.', 'SEGURIDAD', 'CREAR'),
('ROL_EDITAR', 'Editar roles', 'Modifica relaciones de permisos.', 'SEGURIDAD', 'EDITAR'),
('ROL_INHABILITAR', 'Inhabilitar roles', 'Inhabilita roles.', 'SEGURIDAD', 'INHABILITAR'),
('PERMISO_VER', 'Ver permisos', 'Consulta permisos.', 'SEGURIDAD', 'VER_PERMISOS'),
('PERMISO_ASIGNAR', 'Asignar permisos', 'Asigna permisos y familias.', 'SEGURIDAD', 'ASIGNAR'),
('IDIOMA_VER', 'Ver idiomas', 'Accede a idiomas.', 'IDIOMAS_TRADUCCIONES', 'VER'),
('IDIOMA_CREAR', 'Crear idiomas', 'Crea idiomas.', 'IDIOMAS_TRADUCCIONES', 'CREAR'),
('IDIOMA_EDITAR', 'Editar idiomas', 'Modifica idiomas.', 'IDIOMAS_TRADUCCIONES', 'EDITAR'),
('TRADUCCION_VER', 'Ver traducciones', 'Consulta traducciones.', 'IDIOMAS_TRADUCCIONES', 'VER'),
('TRADUCCION_EDITAR', 'Editar traducciones', 'Modifica traducciones.', 'IDIOMAS_TRADUCCIONES', 'EDITAR'),
('BITACORA_VER', 'Ver bitacora', 'Consulta la bitacora.', 'AUDITORIA', 'VER_BITACORA'),
('AUDITORIA_CAMBIOS_VER', 'Ver auditoria', 'Consulta auditoria de cambios.', 'AUDITORIA', 'VER_AUDITORIA'),
('DONANTE_VER', 'Ver donantes', 'Consulta donantes.', 'PN1', 'DONANTE_VER'),
('DONANTE_CREAR', 'Registrar donantes', 'Registra donantes.', 'PN1', 'DONANTE_CREAR'),
('DONACION_CREAR', 'Registrar donaciones', 'Registra donaciones y genera unidades.', 'PN1', 'DONACION_CREAR'),
('UNIDAD_VER', 'Ver unidades', 'Consulta unidades.', 'PN1', 'UNIDAD_VER'),
('UNIDAD_CLASIFICAR', 'Clasificar unidades', 'Clasifica unidades en revisión.', 'PN1', 'UNIDAD_CLASIFICAR'),
('UNIDAD_LIBERAR', 'Liberar unidades', 'Libera unidades clasificadas.', 'PN1', 'UNIDAD_LIBERAR'),
('UNIDAD_BLOQUEAR', 'Bloquear unidades', 'Bloquea unidades clasificadas.', 'PN1', 'UNIDAD_BLOQUEAR'),
('UNIDAD_DESCARTAR', 'Descartar unidades', 'Descarta unidades clasificadas.', 'PN1', 'UNIDAD_DESCARTAR');

INSERT INTO dbo.Permiso (codigo, nombre, descripcion, modulo, accion)
SELECT codigo, nombre, descripcion, modulo, accion FROM @Permisos;
GO

DECLARE @Componentes TABLE
(
    codigo VARCHAR(100) NOT NULL PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    descripcion VARCHAR(255) NULL,
    tipo VARCHAR(20) NOT NULL
);

INSERT INTO @Componentes (codigo, nombre, descripcion, tipo) VALUES
('ADMINISTRADOR', 'Administrador', 'Familia con acceso total.', 'FAMILIA'),
('SEGURIDAD', 'Seguridad', 'Familia de usuarios, roles y permisos.', 'FAMILIA'),
('AUDITORIA', 'Auditoria', 'Familia de bitacora y auditoria.', 'FAMILIA'),
('IDIOMAS_TRADUCCIONES', 'Idiomas y traducciones', 'Familia de internacionalizacion.', 'FAMILIA'),
('PN1_PERSONAL_EXTRACCION', 'Personal de extracción', 'Donantes y donaciones.', 'FAMILIA'),
('PN1_OPERADOR_AUTORIZADO', 'Operador autorizado', 'Clasificación de unidades.', 'FAMILIA'),
('PN1_RESPONSABLE_AUTORIZADO', 'Responsable autorizado', 'Estados finales de unidades.', 'FAMILIA'),
('USUARIO_VER', 'Ver usuarios', 'Permiso de consulta.', 'PERMISO'),
('USUARIO_CREAR', 'Crear usuarios', 'Permiso de alta.', 'PERMISO'),
('USUARIO_EDITAR', 'Editar usuarios', 'Permiso de edición.', 'PERMISO'),
('USUARIO_INHABILITAR', 'Inhabilitar usuarios', 'Permiso de baja lógica.', 'PERMISO'),
('ROL_VER', 'Ver roles', 'Permiso de consulta.', 'PERMISO'),
('ROL_CREAR', 'Crear roles', 'Permiso de alta.', 'PERMISO'),
('ROL_EDITAR', 'Editar roles', 'Permiso de edición.', 'PERMISO'),
('ROL_INHABILITAR', 'Inhabilitar roles', 'Permiso de baja lógica.', 'PERMISO'),
('PERMISO_VER', 'Ver permisos', 'Permiso de consulta.', 'PERMISO'),
('PERMISO_ASIGNAR', 'Asignar permisos', 'Permiso de asignación.', 'PERMISO'),
('IDIOMA_VER', 'Ver idiomas', 'Permiso de consulta.', 'PERMISO'),
('IDIOMA_CREAR', 'Crear idiomas', 'Permiso de alta.', 'PERMISO'),
('IDIOMA_EDITAR', 'Editar idiomas', 'Permiso de edición.', 'PERMISO'),
('TRADUCCION_VER', 'Ver traducciones', 'Permiso de consulta.', 'PERMISO'),
('TRADUCCION_EDITAR', 'Editar traducciones', 'Permiso de edición.', 'PERMISO'),
('BITACORA_VER', 'Ver bitacora', 'Permiso de consulta.', 'PERMISO'),
('AUDITORIA_CAMBIOS_VER', 'Ver auditoria', 'Permiso de consulta.', 'PERMISO'),
('DONANTE_VER', 'Ver donantes', 'Permiso de consulta.', 'PERMISO'),
('DONANTE_CREAR', 'Registrar donantes', 'Permiso de alta.', 'PERMISO'),
('DONACION_CREAR', 'Registrar donaciones', 'Permiso de alta y generación.', 'PERMISO'),
('UNIDAD_VER', 'Ver unidades', 'Permiso de consulta.', 'PERMISO'),
('UNIDAD_CLASIFICAR', 'Clasificar unidades', 'Permiso de clasificación.', 'PERMISO'),
('UNIDAD_LIBERAR', 'Liberar unidades', 'Permiso de liberación.', 'PERMISO'),
('UNIDAD_BLOQUEAR', 'Bloquear unidades', 'Permiso de bloqueo.', 'PERMISO'),
('UNIDAD_DESCARTAR', 'Descartar unidades', 'Permiso de descarte.', 'PERMISO');

INSERT INTO dbo.ComponentePermiso (codigo, nombre, descripcion, tipo)
SELECT codigo, nombre, descripcion, tipo FROM @Componentes;

DECLARE @Relaciones TABLE (padre VARCHAR(100), hijo VARCHAR(100));
INSERT INTO @Relaciones (padre, hijo) VALUES
('ADMINISTRADOR', 'SEGURIDAD'), ('ADMINISTRADOR', 'AUDITORIA'), ('ADMINISTRADOR', 'IDIOMAS_TRADUCCIONES'),
('ADMINISTRADOR', 'PN1_PERSONAL_EXTRACCION'), ('ADMINISTRADOR', 'PN1_OPERADOR_AUTORIZADO'), ('ADMINISTRADOR', 'PN1_RESPONSABLE_AUTORIZADO'),
('SEGURIDAD', 'USUARIO_VER'), ('SEGURIDAD', 'USUARIO_CREAR'), ('SEGURIDAD', 'USUARIO_EDITAR'), ('SEGURIDAD', 'USUARIO_INHABILITAR'),
('SEGURIDAD', 'ROL_VER'), ('SEGURIDAD', 'ROL_CREAR'), ('SEGURIDAD', 'ROL_EDITAR'), ('SEGURIDAD', 'ROL_INHABILITAR'),
('SEGURIDAD', 'PERMISO_VER'), ('SEGURIDAD', 'PERMISO_ASIGNAR'),
('AUDITORIA', 'BITACORA_VER'), ('AUDITORIA', 'AUDITORIA_CAMBIOS_VER'),
('IDIOMAS_TRADUCCIONES', 'IDIOMA_VER'), ('IDIOMAS_TRADUCCIONES', 'IDIOMA_CREAR'), ('IDIOMAS_TRADUCCIONES', 'IDIOMA_EDITAR'),
('IDIOMAS_TRADUCCIONES', 'TRADUCCION_VER'), ('IDIOMAS_TRADUCCIONES', 'TRADUCCION_EDITAR'),
('PN1_PERSONAL_EXTRACCION', 'DONANTE_VER'), ('PN1_PERSONAL_EXTRACCION', 'DONANTE_CREAR'), ('PN1_PERSONAL_EXTRACCION', 'DONACION_CREAR'),
('PN1_OPERADOR_AUTORIZADO', 'UNIDAD_VER'), ('PN1_OPERADOR_AUTORIZADO', 'UNIDAD_CLASIFICAR'),
('PN1_RESPONSABLE_AUTORIZADO', 'UNIDAD_VER'), ('PN1_RESPONSABLE_AUTORIZADO', 'UNIDAD_LIBERAR'),
('PN1_RESPONSABLE_AUTORIZADO', 'UNIDAD_BLOQUEAR'), ('PN1_RESPONSABLE_AUTORIZADO', 'UNIDAD_DESCARTAR');

INSERT INTO dbo.ComponentePermisoRelacion (id_padre, id_hijo)
SELECT padre.id_componente, hijo.id_componente
FROM @Relaciones r
INNER JOIN dbo.ComponentePermiso padre ON padre.codigo = r.padre
INNER JOIN dbo.ComponentePermiso hijo ON hijo.codigo = r.hijo;
GO

DECLARE @RolPermisos TABLE (rol VARCHAR(100), permiso VARCHAR(100));
INSERT INTO @RolPermisos (rol, permiso) VALUES
('Administrador', 'USUARIO_VER'), ('Administrador', 'USUARIO_CREAR'), ('Administrador', 'USUARIO_EDITAR'), ('Administrador', 'USUARIO_INHABILITAR'),
('Administrador', 'ROL_VER'), ('Administrador', 'ROL_CREAR'), ('Administrador', 'ROL_EDITAR'), ('Administrador', 'ROL_INHABILITAR'),
('Administrador', 'PERMISO_VER'), ('Administrador', 'PERMISO_ASIGNAR'), ('Administrador', 'IDIOMA_VER'), ('Administrador', 'IDIOMA_CREAR'),
('Administrador', 'IDIOMA_EDITAR'), ('Administrador', 'TRADUCCION_VER'), ('Administrador', 'TRADUCCION_EDITAR'),
('Administrador', 'BITACORA_VER'), ('Administrador', 'AUDITORIA_CAMBIOS_VER'), ('Administrador', 'DONANTE_VER'), ('Administrador', 'DONANTE_CREAR'),
('Administrador', 'DONACION_CREAR'), ('Administrador', 'UNIDAD_VER'), ('Administrador', 'UNIDAD_CLASIFICAR'), ('Administrador', 'UNIDAD_LIBERAR'),
('Administrador', 'UNIDAD_BLOQUEAR'), ('Administrador', 'UNIDAD_DESCARTAR'),
('Personal de extracción', 'DONANTE_VER'), ('Personal de extracción', 'DONANTE_CREAR'), ('Personal de extracción', 'DONACION_CREAR'),
('Operador autorizado', 'UNIDAD_VER'), ('Operador autorizado', 'UNIDAD_CLASIFICAR'),
('Responsable autorizado', 'UNIDAD_VER'), ('Responsable autorizado', 'UNIDAD_LIBERAR'), ('Responsable autorizado', 'UNIDAD_BLOQUEAR'),
('Responsable autorizado', 'UNIDAD_DESCARTAR');

INSERT INTO dbo.RolPermiso (id_rol, id_permiso)
SELECT r.id_rol, p.id_permiso
FROM @RolPermisos rp
INNER JOIN dbo.Rol r ON r.nombre = rp.rol
INNER JOIN dbo.Permiso p ON p.codigo = rp.permiso;
GO

DECLARE @IdIdiomaEspanol INT = (SELECT id_idioma FROM dbo.Idioma WHERE codigo = 'es-AR');

INSERT INTO dbo.Usuario (id_idioma, nombre_usuario, email, password_hash, nombre, apellido, estado_usuario)
VALUES (@IdIdiomaEspanol, 'admin', 'admin@tecnisalud.local', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', 'Administrador', 'Sistema', 'ACTIVO');

INSERT INTO dbo.UsuarioRol (id_usuario, id_rol)
SELECT u.id_usuario, r.id_rol
FROM dbo.Usuario u CROSS JOIN dbo.Rol r
WHERE u.nombre_usuario = 'admin' AND r.nombre = 'Administrador';

INSERT INTO dbo.UsuarioComponentePermiso (id_usuario, id_componente)
SELECT u.id_usuario, c.id_componente
FROM dbo.Usuario u CROSS JOIN dbo.ComponentePermiso c
WHERE u.nombre_usuario = 'admin' AND c.codigo = 'ADMINISTRADOR';
GO

-- El administrador queda con el arbol completo; los roles funcionales
-- se asignan a los usuarios desde la administracion de seguridad.
-- ============================================================
-- FIN DEL SCRIPT PN1
-- ============================================================

-- Permisos nuevos: solo el administrador los recibe de forma predeterminada.
DECLARE @permisos_integridad TABLE(codigo VARCHAR(100), nombre VARCHAR(100), accion VARCHAR(100));
INSERT @permisos_integridad VALUES
('INTEGRIDAD_VER','Consultar integridad','VER'),
('INTEGRIDAD_RESTAURAR','Restaurar integridad','RESTAURAR');
INSERT dbo.Permiso(codigo,nombre,descripcion,modulo,accion)
SELECT p.codigo,p.nombre,p.nombre,'INTEGRIDAD',p.accion FROM @permisos_integridad p
WHERE NOT EXISTS(SELECT 1 FROM dbo.Permiso d WHERE d.codigo=p.codigo);
UPDATE d SET nombre=p.nombre,descripcion=p.nombre,modulo='INTEGRIDAD',accion=p.accion
FROM dbo.Permiso d JOIN @permisos_integridad p ON p.codigo=d.codigo;
INSERT dbo.ComponentePermiso(codigo,nombre,descripcion,tipo)
SELECT p.codigo,p.nombre,p.nombre,'PERMISO' FROM @permisos_integridad p
WHERE NOT EXISTS(SELECT 1 FROM dbo.ComponentePermiso d WHERE d.codigo=p.codigo);
UPDATE d SET nombre=p.nombre,descripcion=p.nombre,tipo='PERMISO',estado_componente='ACTIVO'
FROM dbo.ComponentePermiso d JOIN @permisos_integridad p ON p.codigo=d.codigo;
INSERT dbo.ComponentePermisoRelacion(id_padre,id_hijo)
SELECT a.id_componente,c.id_componente FROM dbo.ComponentePermiso a
CROSS JOIN dbo.ComponentePermiso c WHERE a.codigo='ADMINISTRADOR'
AND c.codigo IN ('INTEGRIDAD_VER','INTEGRIDAD_RESTAURAR')
AND NOT EXISTS(SELECT 1 FROM dbo.ComponentePermisoRelacion r WHERE r.id_padre=a.id_componente AND r.id_hijo=c.id_componente);
INSERT dbo.RolPermiso(id_rol,id_permiso)
SELECT r.id_rol,p.id_permiso FROM dbo.Rol r CROSS JOIN dbo.Permiso p
WHERE r.nombre='Administrador' AND p.codigo IN ('INTEGRIDAD_VER','INTEGRIDAD_RESTAURAR')
AND NOT EXISTS(SELECT 1 FROM dbo.RolPermiso rp WHERE rp.id_rol=r.id_rol AND rp.id_permiso=p.id_permiso);
GO
-- Catalogo incremental de integridad: no elimina idiomas ni traducciones personalizadas.
DECLARE @i18n_integridad TABLE(clave VARCHAR(150), es NVARCHAR(500), en NVARCHAR(500));
INSERT @i18n_integridad VALUES
('MENU_INTEGRITY',N'Integridad',N'Integrity'),
('BITACORA_MODULE_INTEGRITY',N'Integridad',N'Integrity'),
('INTEGRITY_OK',N'Integridad: última verificación correcta.',N'Integrity: last verification passed.'),
('INTEGRITY_BLOCKED',N'Modo solo consulta: integridad comprometida. Los datos afectados no son confiables; contactá al administrador.',N'Read-only mode: integrity compromised. Affected data is untrusted; contact the administrator.'),
('INTEGRITY_UNAVAILABLE',N'Operaciones protegidas bloqueadas: no se pudo verificar la integridad. Revisá la conexión y las migraciones SQL.',N'Protected operations are blocked: integrity could not be verified. Check the connection and SQL migrations.'),
('INTEGRITY_PENDING',N'Pendiente: revisar los datos y aceptar la base inicial.',N'Pending: review the data and accept the initial baseline.'),
('INTEGRITY_DESCRIPTION',N'Se verifican filas (DVH), tablas (DVV) e historiales de los procesos protegidos. Restaurar recupera las últimas versiones verificadas; no deshace operaciones legítimas.',N'Checks rows (DVH), tables (DVV), and histories for protected processes. Restore recovers the latest verified versions; it does not undo legitimate operations.'),
('INTEGRITY_VERIFY',N'Verificar / actualizar',N'Verify / refresh'),
('INTEGRITY_INITIALIZE',N'Aceptar base inicial',N'Accept initial baseline'),
('INTEGRITY_RESTORE',N'Restaurar desde historial',N'Restore from history'),
('INTEGRITY_ENTITY',N'Entidad',N'Entity'),
('INTEGRITY_ID',N'ID',N'ID'),
('INTEGRITY_TYPE',N'Incidencia',N'Issue'),
('INTEGRITY_DATE',N'Detectada',N'Detected'),
('INTEGRITY_CURRENT',N'Registro actual (JSON; vacío si falta)',N'Current record (JSON; empty if missing)'),
('INTEGRITY_TRUSTED',N'Última versión del historial (usar solo si es íntegro)',N'Latest history version (use only when intact)'),
('INTEGRITY_CONFIRM_BASELINE',N'¿Aceptás los datos actuales como base inicial? Confirmá solo después de revisarlos y respaldarlos. Esta acción no corrige datos previos y se realiza una sola vez.',N'Accept current data as the initial baseline? Confirm only after reviewing and backing it up. This does not correct existing data and can only be done once.'),
('INTEGRITY_CONFIRM_RESTORE',N'¿Restaurar todas las inconsistencias recuperables desde las últimas versiones verificadas? Se conservarán los identificadores y se auditará la operación. No se aceptarán datos alterados como válidos.',N'Restore all recoverable inconsistencies from the latest verified versions? IDs will be preserved and the operation audited. Altered data will not be accepted as valid.'),
('INTEGRITY_RECOVERED',N'Operación completada e integridad verificada.',N'Operation completed and integrity verified.'),
('INTEGRITY_RECOVERY_FAILED',N'No se confirmó la recuperación. Verificá nuevamente el estado y revisá la bitácora antes de reintentar.',N'Recovery was not confirmed. Verify the state again and review the log before retrying.'),
('INTEGRITY_STALE_PREVIEW',N'Los datos cambiaron desde la revisión. Actualizá el informe antes de confirmar.',N'Data changed since the preview. Refresh the report before confirming.'),
('INTEGRITY_BACKUP_REQUIRED',N'La fuente no permite una recuperación segura. Necesitás un backup confiable; no recalcules para ocultar el error.',N'The source cannot support safe recovery. A trusted backup is required; do not recalculate to hide the error.'),
('INTEGRITY_TYPE_SIN_INICIALIZAR',N'Base inicial ausente o incompleta',N'Missing or incomplete baseline'),
('INTEGRITY_TYPE_DVH',N'Dígito de fila incorrecto',N'Row checksum mismatch'),
('INTEGRITY_TYPE_DVV',N'Dígito de tabla incorrecto',N'Table checksum mismatch'),
('INTEGRITY_TYPE_HISTORIAL_INVALIDO',N'Historial alterado',N'Altered history'),
('INTEGRITY_TYPE_FALTANTE',N'Registro eliminado',N'Deleted record'),
('INTEGRITY_TYPE_SIN_VERSION',N'Registro sin versión confiable',N'Record without a trusted version'),
('INTEGRITY_TYPE_VERSION_DIFERENTE',N'Datos distintos del historial',N'Data differs from history'),
('BITACORA_ACTION_INTEGRITY_DETECTED',N'Error de integridad detectado',N'Integrity error detected'),
('BITACORA_ACTION_INTEGRITY_INITIALIZED',N'Base inicial de integridad aceptada',N'Initial integrity baseline accepted'),
('BITACORA_ACTION_INTEGRITY_RESTORED',N'Integridad restaurada',N'Integrity restored'),
('BITACORA_ACTION_RESTORE_FAILED',N'Recuperación fallida',N'Recovery failed');
INSERT dbo.Etiqueta(clave,descripcion)
SELECT c.clave,c.clave FROM @i18n_integridad c
WHERE NOT EXISTS(SELECT 1 FROM dbo.Etiqueta e WHERE e.clave=c.clave);
INSERT dbo.Traduccion(id_etiqueta,id_idioma,texto)
SELECT e.id_etiqueta,i.id_idioma,CASE WHEN i.codigo='en-US' THEN c.en ELSE c.es END
FROM @i18n_integridad c JOIN dbo.Etiqueta e ON e.clave=c.clave
CROSS JOIN dbo.Idioma i WHERE i.codigo IN ('es-AR','en-US')
AND NOT EXISTS(SELECT 1 FROM dbo.Traduccion t WHERE t.id_etiqueta=e.id_etiqueta AND t.id_idioma=i.id_idioma);
GO
