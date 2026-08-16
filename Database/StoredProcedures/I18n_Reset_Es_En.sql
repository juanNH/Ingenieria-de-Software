USE [TecniSalud];
GO

/*
    Script destructivo de reinicio i18n.

    Hace lo siguiente:
    1. Desasocia usuarios de su idioma preferido para evitar conflictos de FK.
    2. Borra traducciones, etiquetas, historial de estados de idioma e idiomas.
    3. Crea primero Espanol Argentina (es-AR).
    4. Crea luego Ingles Estados Unidos (en-US).
    5. Inserta todas las etiquetas y traducciones base en ambos idiomas.
*/

SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH('dbo.Usuario', 'id_idioma') IS NOT NULL
BEGIN
    UPDATE dbo.Usuario
    SET id_idioma = NULL;
END;

DELETE FROM dbo.Traduccion;
DELETE FROM dbo.Etiqueta;

IF OBJECT_ID('dbo.IdiomaEstadoHistorial', 'U') IS NOT NULL
BEGIN
    DELETE FROM dbo.IdiomaEstadoHistorial;
END;

DELETE FROM dbo.Idioma;

DBCC CHECKIDENT ('dbo.Traduccion', RESEED, 0);
DBCC CHECKIDENT ('dbo.Etiqueta', RESEED, 0);
DBCC CHECKIDENT ('dbo.Idioma', RESEED, 0);

IF OBJECT_ID('dbo.IdiomaEstadoHistorial', 'U') IS NOT NULL
BEGIN
    DBCC CHECKIDENT ('dbo.IdiomaEstadoHistorial', RESEED, 0);
END;

INSERT INTO dbo.Idioma (codigo, nombre, estado_idioma)
VALUES ('es-AR', 'Espanol Argentina', 'ACTIVO');

DECLARE @es INT = SCOPE_IDENTITY();

INSERT INTO dbo.Idioma (codigo, nombre, estado_idioma)
VALUES ('en-US', 'Ingles Estados Unidos', 'ACTIVO');

DECLARE @en INT = SCOPE_IDENTITY();

DECLARE @Catalogo TABLE
(
    Clave VARCHAR(150) NOT NULL,
    Descripcion VARCHAR(255) NULL,
    TextoEs NVARCHAR(500) NOT NULL,
    TextoEn NVARCHAR(500) NOT NULL
);

INSERT INTO @Catalogo (Clave, Descripcion, TextoEs, TextoEn)
VALUES
('AUDIT_COUNT', 'Cantidad de eventos registrados', '{0} evento(s) registrados.', '{0} event(s) registered.'),
('AUDIT_DESCRIPTION', 'Descripcion de bitacora', 'Aca vas a poder consultar los eventos del sistema.', 'Here you can review system events.'),
('AUDIT_EMPTY', 'Mensaje sin eventos', 'No hay eventos registrados.', 'No events registered.'),
('AUDIT_TITLE', 'Titulo de bitacora', 'Bitacora', 'Audit log'),
('AUDIT_FILTERS', 'Filtros de bitacora', 'Filtros', 'Filters'),
('AUDIT_FILTER_FROM', 'Filtro fecha desde', 'Desde', 'From'),
('AUDIT_FILTER_TO', 'Filtro fecha hasta', 'Hasta', 'To'),
('AUDIT_FILTER_USER', 'Filtro usuario', 'Usuario', 'User'),
('AUDIT_FILTER_MODULE', 'Filtro modulo', 'Modulo', 'Module'),
('AUDIT_FILTER_ACTION', 'Filtro accion', 'Accion', 'Action'),
('AUDIT_FILTER_LEVEL', 'Filtro nivel', 'Nivel', 'Level'),
('AUDIT_FILTER_DESCRIPTION', 'Filtro descripcion', 'Descripcion', 'Description'),
('BITACORA_ACTION_LOGIN_FAILURE', 'Accion login fallido', 'Login fallido', 'Failed login'),
('BITACORA_ACTION_LOGIN_SUCCESS', 'Accion login exitoso', 'Login exitoso', 'Successful login'),
('BITACORA_ACTION_REGISTER_FAILURE', 'Accion registro fallido', 'Registro fallido', 'Failed registration'),
('BITACORA_LEVEL_ERROR', 'Nivel error', 'Error', 'Error'),
('BITACORA_LEVEL_INFORMATION', 'Nivel informacion', 'Informacion', 'Information'),
('BITACORA_LEVEL_WARNING', 'Nivel advertencia', 'Advertencia', 'Warning'),
('BITACORA_MODULE_SECURITY', 'Modulo seguridad', 'Seguridad', 'Security'),
('BTN_ADD', 'Boton agregar', 'Agregar', 'Add'),
('BTN_CREATE', 'Boton crear', 'Crear', 'Create'),
('BTN_CREATE_ROLE', 'Boton crear rol', 'Crear rol', 'Create role'),
('BTN_DISABLE', 'Boton inhabilitar', 'Inhabilitar', 'Disable'),
('BTN_NEW', 'Boton nuevo', 'Nuevo', 'New'),
('BTN_RECALCULATE_DV', 'Boton recalcular digitos verificadores', 'Recalcular DV', 'Recalculate DV'),
('BTN_REFRESH', 'Boton actualizar', 'Actualizar', 'Refresh'),
('BTN_SEARCH', 'Boton buscar', 'Buscar', 'Search'),
('BTN_CLEAR_FILTERS', 'Boton limpiar filtros', 'Limpiar', 'Clear'),
('BTN_REMOVE_FROM', 'Boton quitar desde familia', 'Quitar de {0}', 'Remove from {0}'),
('BTN_REMOVE_SELECTED', 'Boton quitar seleccionado', 'Quitar seleccionado', 'Remove selected'),
('BTN_SAVE', 'Boton guardar', 'Guardar', 'Save'),
('COMPONENT_SELECTED', 'Componente seleccionado', 'Componente Seleccionado', 'Selected Component'),
('COMPONENT_TREE', 'Arbol de etiquetas', 'Etiquetas', 'Labels'),
('FIELD_EMAIL', 'Campo email', 'Email', 'Email'),
('FIELD_LASTNAME', 'Campo apellido', 'Apellido', 'Last name'),
('FIELD_NAME', 'Campo nombre', 'Nombre', 'Name'),
('FIELD_NEW_PASSWORD', 'Campo contrasena nueva', 'Contrasena nueva', 'New password'),
('FIELD_STATUS', 'Campo estado', 'Estado', 'Status'),
('FIELD_USER', 'Campo usuario', 'Usuario', 'User'),
('FILTER_ALL', 'Filtro todos', 'Todos', 'All'),
('FILTER_ALL_ACTIONS', 'Filtro todas las acciones', 'Todas', 'All'),
('GRID_ACTION', 'Columna accion', 'Accion', 'Action'),
('GRID_DATE', 'Columna fecha', 'Fecha', 'Date'),
('GRID_DESCRIPTION', 'Columna descripcion', 'Descripcion', 'Description'),
('GRID_DEVICE', 'Columna equipo', 'Equipo', 'Device'),
('GRID_DV_BLOCK', 'Columna bloqueo digito verificador', 'Bloqueo DV', 'DV block'),
('GRID_EMAIL', 'Columna email', 'Email', 'Email'),
('GRID_ID', 'Columna id', 'ID', 'ID'),
('GRID_LEVEL', 'Columna nivel', 'Nivel', 'Level'),
('GRID_MODULE', 'Columna modulo', 'Modulo', 'Module'),
('GRID_STATUS', 'Columna estado', 'Estado', 'Status'),
('GRID_USER', 'Columna usuario', 'Usuario', 'User'),
('GRID_USER_ID', 'Columna id usuario', 'ID usuario', 'User ID'),
('LABEL_TAG', 'Etiqueta seleccionada', 'Etiqueta', 'Label'),
('LANGUAGE_ACTIVE', 'Idioma activo', 'Actividad', 'Active'),
('LANGUAGE_CODE', 'Codigo de idioma', 'Codigo de Idioma', 'Language Code'),
('LANGUAGE_DETAIL', 'Detalle de idioma', 'Detalles de Lenguaje', 'Language Details'),
('LANGUAGE_SELECTOR', 'Selector de idioma', 'Idioma seleccionado', 'Selected language'),
('LANGUAGES_DESCRIPTION', 'Descripcion de idiomas', 'Gestiona los idiomas en el siguiente panel:', 'Manage languages in the following panel:'),
('LANGUAGES_TITLE', 'Titulo idiomas', 'Idiomas', 'Languages'),
('MAIN_NO_SESSION', 'Texto sin sesion', 'Usuario: sin sesion', 'User: no session'),
('MAIN_TITLE', 'Titulo de la ventana principal', 'Panel principal', 'Main panel'),
('MAIN_USER', 'Texto de usuario autenticado', 'Usuario: {0}', 'User: {0}'),
('MENU_AUDIT', 'Menu bitacora', 'Bitacora', 'Audit log'),
('MENU_LANGUAGES', 'Menu idiomas', 'Idiomas', 'Languages'),
('MENU_LOGOUT', 'Menu salir', 'Salir', 'Log out'),
('MENU_ROLES', 'Menu roles', 'Roles', 'Roles'),
('MENU_USERS', 'Menu usuarios', 'Usuarios', 'Users'),
('NO_PERMISSIONS_ASSIGNED', 'Sin permisos asignados', 'No tenes permisos asignados.', 'You do not have assigned permissions.'),
('ROLE_CHILD_COMPONENT', 'Permiso o familia a agregar', 'Permiso o familia a agregar', 'Permission or family to add'),
('ROLE_CODE', 'Campo codigo de rol', 'Codigo', 'Code'),
('ROLE_CREATE_ERROR', 'Error al crear rol', 'No se pudo crear el rol.', 'Could not create the role.'),
('ROLE_CREATED', 'Rol creado', 'Rol creado correctamente.', 'Role created successfully.'),
('ROLE_CYCLE_ERROR', 'Ciclo detectado', 'La relacion genera un ciclo.', 'The relation creates a cycle.'),
('ROLE_INVALID_CHILD', 'Hijo invalido', 'El componente seleccionado no es valido.', 'The selected component is invalid.'),
('ROLE_INVALID_PARENT', 'Padre invalido', 'La familia seleccionada no es valida.', 'The selected family is invalid.'),
('ROLE_RELATION_ADD_ERROR', 'Error al agregar relacion', 'No se pudo agregar la relacion.', 'Could not add the relation.'),
('ROLE_RELATION_ADDED', 'Relacion agregada', 'Relacion agregada correctamente.', 'Relation added successfully.'),
('ROLE_RELATION_IDENTIFY_ERROR', 'Error al identificar relacion', 'No se pudo identificar la relacion.', 'Could not identify the relation.'),
('ROLE_RELATION_REMOVE_ERROR', 'Error al quitar relacion', 'No se pudo quitar la relacion.', 'Could not remove the relation.'),
('ROLE_RELATION_REMOVED', 'Relacion quitada', 'Relacion quitada correctamente.', 'Relation removed successfully.'),
('ROLE_SELECT_CHILD', 'Seleccionar permiso hijo', 'Selecciona un permiso o familia para quitar.', 'Select a permission or family to remove.'),
('ROLE_SELECT_FAMILY', 'Seleccionar familia', 'Selecciona una familia del arbol.', 'Select a family from the tree.'),
('ROLE_SELECT_FAMILY_AND_COMPONENT', 'Seleccionar familia y componente', 'Selecciona una familia y un componente.', 'Select a family and a component.'),
('ROLE_SELECTED_FAMILY', 'Familia seleccionada', 'Familia seleccionada en el arbol', 'Family selected in the tree'),
('ROLE_SELF_REFERENCE_ERROR', 'Error por autoreferencia', 'Un rol no puede contenerse a si mismo.', 'A role cannot contain itself.'),
('ROLES_ADMIN', 'Administracion de roles', 'Administracion de roles', 'Role administration'),
('ROLES_DESCRIPTION', 'Descripcion de roles', 'Administracion de roles y permisos del sistema.', 'Manage system roles and permissions.'),
('ROLES_STRUCTURE', 'Estructura de roles', 'Estructura de roles', 'Role structure'),
('ROLES_TITLE', 'Titulo de roles', 'Roles', 'Roles'),
('SECURITY_ACCESS_DENIED', 'Acceso denegado', 'No tenes permisos para acceder a esta seccion.', 'You do not have permission to access this section.'),
('SECURITY_LANGUAGE_CREATE_DENIED', 'Creacion de idioma denegada', 'No tenes permisos para crear idiomas.', 'You do not have permission to create languages.'),
('SECURITY_LANGUAGE_EDIT_DENIED', 'Edicion de idioma denegada', 'No tenes permisos para modificar idiomas.', 'You do not have permission to modify languages.'),
('SECURITY_ROLE_CREATE_DENIED', 'Permiso denegado para crear roles', 'No tenes permisos para crear roles.', 'You do not have permission to create roles.'),
('SECURITY_ROLE_EDIT_DENIED', 'Permiso denegado para modificar roles', 'No tenes permisos para modificar roles.', 'You do not have permission to modify roles.'),
('SECURITY_TRANSLATION_EDIT_DENIED', 'Edicion de traduccion denegada', 'No tenes permisos para modificar traducciones.', 'You do not have permission to modify translations.'),
('TRANSLATION_DETAIL', 'Detalle de traduccion', 'Detalles Idioma', 'Translation Details'),
('TRANSLATION_TEXT', 'Texto de traduccion', 'Traduccion', 'Translation'),
('USER_ROLE_EDIT_DENIED', 'Permiso denegado para roles de usuario', 'No tenes permisos para modificar roles de usuarios.', 'You do not have permission to modify user roles.'),
('USER_ROLE_EMPTY', 'Sin roles disponibles', 'No hay roles disponibles.', 'There are no roles available.'),
('USER_ROLE_SELECT_HELP', 'Ayuda para seleccionar usuario', 'Selecciona un usuario para asignarle un rol.', 'Select a user to assign a role.'),
('USER_ROLE_SELECT_ONE', 'Ayuda para seleccionar rol', 'Selecciona el rol que queres asignar.', 'Select the role you want to assign.'),
('USER_ROLES', 'Grupo roles de usuario', 'Roles de usuario', 'User roles'),
('USERS_CREATE_MODE', 'Modo crear usuario', 'Crear usuario', 'Create user'),
('USERS_DESCRIPTION', 'Descripcion de usuarios', 'Alta, modificacion e inhabilitacion de usuarios del sistema.', 'Create, edit and disable system users.'),
('USERS_DETAIL', 'Detalle de usuario', 'Detalle de usuario', 'User details'),
('USERS_EDIT_MODE', 'Modo modificar usuario', 'Modificar usuario', 'Edit user'),
('USERS_TITLE', 'Titulo de usuarios', 'Usuarios', 'Users');

INSERT INTO dbo.Etiqueta (clave, descripcion)
SELECT Clave, Descripcion
FROM @Catalogo
ORDER BY Clave;

INSERT INTO dbo.Traduccion (id_etiqueta, id_idioma, texto)
SELECT e.id_etiqueta, @es, c.TextoEs
FROM @Catalogo c
INNER JOIN dbo.Etiqueta e ON e.clave = c.Clave
ORDER BY c.Clave;

INSERT INTO dbo.Traduccion (id_etiqueta, id_idioma, texto)
SELECT e.id_etiqueta, @en, c.TextoEn
FROM @Catalogo c
INNER JOIN dbo.Etiqueta e ON e.clave = c.Clave
ORDER BY c.Clave;

UPDATE dbo.Usuario
SET id_idioma = @es
WHERE id_idioma IS NULL;

COMMIT TRANSACTION;

SELECT
    i.codigo,
    COUNT(t.id_traduccion) AS cantidad_traducciones
FROM dbo.Idioma i
LEFT JOIN dbo.Traduccion t ON t.id_idioma = i.id_idioma
GROUP BY i.codigo
ORDER BY i.codigo;
GO
