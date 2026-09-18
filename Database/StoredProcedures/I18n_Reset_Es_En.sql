USE [HemovidaGest];
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
('CHANGE_AUDIT_TITLE', 'Titulo auditoria de cambios', 'Auditoría de cambios', 'Change audit'),
('CHANGE_AUDIT_DESCRIPTION', 'Descripcion auditoria de cambios', 'Historial de cambios registrados sobre entidades auditadas.', 'History of recorded changes on audited entities.'),
('CHANGE_AUDIT_EMPTY', 'Auditoria de cambios vacia', 'No hay cambios registrados.', 'There are no recorded changes.'),
('CHANGE_AUDIT_COUNT', 'Cantidad de cambios', '{0} cambio(s) registrado(s).', '{0} change(s) registered.'),
('CHANGE_AUDIT_USER', 'Usuario auditado', 'Usuario auditado', 'Audited user'),
('CHANGE_AUDIT_PREVIOUS_STATE', 'Estado anterior', 'Estado anterior', 'Previous state'),
('CHANGE_AUDIT_NEW_STATE', 'Estado nuevo', 'Estado nuevo', 'New state'),
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
('BTN_CREATE_LABEL', 'Boton crear etiqueta', 'Crear etiqueta', 'Create label'),
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
('MENU_PERMISSIONS', 'Menu permisos', 'Permisos', 'Permissions'),
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
('USERS_TITLE', 'Titulo de usuarios', 'Usuarios', 'Users'),
('MENU_CHANGE_AUDIT', 'Menu auditoria de cambios', 'Auditoría de cambios', 'Change audit'),
('MENU_BLOOD_BANK', 'Menu banco de sangre', 'Banco de sangre', 'Blood bank'),
('MENU_DONORS', 'Menu donantes', 'Donantes', 'Donors'),
('MENU_DONATIONS', 'Menu donaciones', 'Donaciones', 'Donations'),
('MENU_UNITS', 'Menu unidades', 'Unidades', 'Units'),
('BITACORA_MODULE_BLOOD_BANK', 'Modulo banco de sangre', 'Banco de sangre', 'Blood bank'),
('BITACORA_ACTION_DONOR_REGISTERED', 'Accion donante registrado', 'Donante registrado', 'Donor registered'),
('BITACORA_ACTION_DONATION_REGISTERED', 'Accion donacion registrada', 'Donación registrada', 'Donation registered'),
('BITACORA_ACTION_UNITS_GENERATED', 'Accion unidades generadas', 'Unidades generadas', 'Units generated'),
('BITACORA_ACTION_UNIT_CLASSIFIED', 'Accion unidad clasificada', 'Unidad clasificada', 'Unit classified'),
('BITACORA_ACTION_UNIT_RELEASED', 'Accion unidad liberada', 'Unidad liberada', 'Unit released'),
('BITACORA_ACTION_UNIT_BLOCKED', 'Accion unidad bloqueada', 'Unidad bloqueada', 'Unit blocked'),
('BITACORA_ACTION_UNIT_DISCARDED', 'Accion unidad descartada', 'Unidad descartada', 'Unit discarded'),
('BITACORA_ACTION_OPERATION_FAILURE', 'Accion operacion fallida', 'Operación fallida', 'Operation failed'),
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
('OPERATION_NOT_AUTHORIZED', 'Operacion no autorizada', 'No tenés permisos para realizar esta operación.', 'You are not authorized to perform this operation.');

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
