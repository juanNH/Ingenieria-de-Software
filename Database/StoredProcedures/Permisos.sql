USE [TecniSalud];
GO

IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_Idioma_Estado'
      AND parent_object_id = OBJECT_ID('dbo.Idioma')
)
BEGIN
    ALTER TABLE dbo.Idioma DROP CONSTRAINT CK_Idioma_Estado;
END
GO

IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_Usuario_Estado'
      AND parent_object_id = OBJECT_ID('dbo.Usuario')
)
BEGIN
    ALTER TABLE dbo.Usuario DROP CONSTRAINT CK_Usuario_Estado;
END
GO

IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_UsuarioRol_Estado'
      AND parent_object_id = OBJECT_ID('dbo.UsuarioRol')
)
BEGIN
    ALTER TABLE dbo.UsuarioRol DROP CONSTRAINT CK_UsuarioRol_Estado;
END
GO

IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_Rol_Estado'
      AND parent_object_id = OBJECT_ID('dbo.Rol')
)
BEGIN
    ALTER TABLE dbo.Rol DROP CONSTRAINT CK_Rol_Estado;
END
GO

UPDATE dbo.Idioma
SET estado_idioma = UPPER(estado_idioma)
WHERE estado_idioma IN ('Activo', 'Inactivo');
GO

UPDATE dbo.Usuario
SET estado_usuario = UPPER(estado_usuario)
WHERE estado_usuario IN ('Activo', 'Inactivo', 'Bloqueado');
GO

UPDATE dbo.UsuarioRol
SET estado_usuario_rol = UPPER(estado_usuario_rol)
WHERE estado_usuario_rol IN ('Activo', 'Inactivo');
GO

UPDATE dbo.Rol
SET estado_rol = UPPER(estado_rol)
WHERE estado_rol IN ('Activo', 'Inactivo');
GO

ALTER TABLE dbo.Idioma
ADD CONSTRAINT CK_Idioma_Estado CHECK (estado_idioma IN ('ACTIVO', 'INACTIVO'));
GO

ALTER TABLE dbo.Usuario
ADD CONSTRAINT CK_Usuario_Estado CHECK (estado_usuario IN ('ACTIVO', 'INACTIVO', 'BLOQUEADO'));
GO

ALTER TABLE dbo.UsuarioRol
ADD CONSTRAINT CK_UsuarioRol_Estado CHECK (estado_usuario_rol IN ('ACTIVO', 'INACTIVO'));
GO

ALTER TABLE dbo.Rol
ADD CONSTRAINT CK_Rol_Estado CHECK (estado_rol IN ('ACTIVO', 'INACTIVO'));
GO

IF OBJECT_ID('dbo.ComponentePermiso', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ComponentePermiso (
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
END
GO

IF OBJECT_ID('dbo.ComponentePermisoRelacion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ComponentePermisoRelacion (
        id_relacion INT IDENTITY(1,1) PRIMARY KEY,
        id_padre INT NOT NULL,
        id_hijo INT NOT NULL,
        CONSTRAINT UQ_ComponentePermisoRelacion UNIQUE (id_padre, id_hijo),
        CONSTRAINT CK_ComponentePermisoRelacion_NoAutoReferencia CHECK (id_padre <> id_hijo),
        CONSTRAINT FK_ComponentePermisoRelacion_Padre FOREIGN KEY (id_padre) REFERENCES dbo.ComponentePermiso(id_componente),
        CONSTRAINT FK_ComponentePermisoRelacion_Hijo FOREIGN KEY (id_hijo) REFERENCES dbo.ComponentePermiso(id_componente)
    );
END
GO

IF OBJECT_ID('dbo.UsuarioComponentePermiso', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UsuarioComponentePermiso (
        id_usuario_componente INT IDENTITY(1,1) PRIMARY KEY,
        id_usuario INT NOT NULL,
        id_componente INT NOT NULL,
        fecha_asignacion DATETIME NOT NULL DEFAULT GETDATE(),
        estado_usuario_componente VARCHAR(20) NOT NULL DEFAULT 'ACTIVO',
        CONSTRAINT UQ_UsuarioComponentePermiso UNIQUE (id_usuario, id_componente),
        CONSTRAINT CK_UsuarioComponentePermiso_Estado CHECK (estado_usuario_componente IN ('ACTIVO', 'INACTIVO')),
        CONSTRAINT FK_UsuarioComponentePermiso_Usuario FOREIGN KEY (id_usuario) REFERENCES dbo.Usuario(id_usuario),
        CONSTRAINT FK_UsuarioComponentePermiso_Componente FOREIGN KEY (id_componente) REFERENCES dbo.ComponentePermiso(id_componente)
    );
END
GO

;WITH RolesActivos AS
(
    SELECT
        id_usuario_componente,
        ROW_NUMBER() OVER (
            PARTITION BY id_usuario
            ORDER BY fecha_asignacion DESC, id_usuario_componente DESC
        ) AS numero
    FROM dbo.UsuarioComponentePermiso
    WHERE estado_usuario_componente = 'ACTIVO'
)
UPDATE uc
SET estado_usuario_componente = 'INACTIVO'
FROM dbo.UsuarioComponentePermiso uc
INNER JOIN RolesActivos ra
    ON ra.id_usuario_componente = uc.id_usuario_componente
WHERE ra.numero > 1;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_UsuarioComponentePermiso_UsuarioActivo'
      AND object_id = OBJECT_ID('dbo.UsuarioComponentePermiso')
)
BEGIN
    CREATE UNIQUE INDEX UX_UsuarioComponentePermiso_UsuarioActivo
        ON dbo.UsuarioComponentePermiso(id_usuario)
        WHERE estado_usuario_componente = 'ACTIVO';
END
GO

IF OBJECT_ID('dbo.sp_ComponentePermiso_AgregarRelacion', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.sp_ComponentePermiso_AgregarRelacion;
END
GO

CREATE PROCEDURE dbo.sp_ComponentePermiso_AgregarRelacion
    @id_padre INT,
    @id_hijo INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @id_padre = @id_hijo
    BEGIN
        SELECT 'AUTO_REFERENCIA' AS codigo_resultado;
        RETURN;
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.ComponentePermiso
        WHERE id_componente = @id_padre
          AND tipo = 'FAMILIA'
          AND UPPER(estado_componente) = 'ACTIVO'
    )
    BEGIN
        SELECT 'PADRE_INVALIDO' AS codigo_resultado;
        RETURN;
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.ComponentePermiso
        WHERE id_componente = @id_hijo
          AND UPPER(estado_componente) = 'ACTIVO'
    )
    BEGIN
        SELECT 'HIJO_INVALIDO' AS codigo_resultado;
        RETURN;
    END;

    DECLARE @ciclo_detectado BIT = 0;

    ;WITH Descendientes AS
    (
        SELECT id_hijo
        FROM dbo.ComponentePermisoRelacion
        WHERE id_padre = @id_hijo

        UNION ALL

        SELECT r.id_hijo
        FROM dbo.ComponentePermisoRelacion r
        INNER JOIN Descendientes d
            ON d.id_hijo = r.id_padre
    )
    SELECT TOP (1) @ciclo_detectado = 1
    FROM Descendientes
    WHERE id_hijo = @id_padre;

    IF @ciclo_detectado = 1
    BEGIN
        SELECT 'CICLO_DETECTADO' AS codigo_resultado;
        RETURN;
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.ComponentePermisoRelacion
        WHERE id_padre = @id_padre
          AND id_hijo = @id_hijo
    )
    BEGIN
        INSERT INTO dbo.ComponentePermisoRelacion (id_padre, id_hijo)
        VALUES (@id_padre, @id_hijo);
    END;

    SELECT 'OK' AS codigo_resultado;
END
GO

DECLARE @componentes TABLE (
    codigo VARCHAR(100) NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    descripcion VARCHAR(255) NULL,
    tipo VARCHAR(20) NOT NULL
);

INSERT INTO @componentes (codigo, nombre, descripcion, tipo) VALUES
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

MERGE dbo.ComponentePermiso AS destino
USING @componentes AS origen
    ON destino.codigo = origen.codigo
WHEN MATCHED THEN
    UPDATE SET
        nombre = origen.nombre,
        descripcion = origen.descripcion,
        tipo = origen.tipo,
        estado_componente = 'ACTIVO'
WHEN NOT MATCHED THEN
    INSERT (codigo, nombre, descripcion, tipo, estado_componente)
    VALUES (origen.codigo, origen.nombre, origen.descripcion, origen.tipo, 'ACTIVO');
GO

DECLARE @relaciones TABLE (
    codigo_padre VARCHAR(100) NOT NULL,
    codigo_hijo VARCHAR(100) NOT NULL
);

INSERT INTO @relaciones (codigo_padre, codigo_hijo) VALUES
('ADMINISTRADOR', 'SEGURIDAD'),
('ADMINISTRADOR', 'AUDITORIA'),
('ADMINISTRADOR', 'IDIOMAS_TRADUCCIONES'),
('ADMINISTRADOR', 'USUARIO_VER'),
('ADMINISTRADOR', 'USUARIO_CREAR'),
('ADMINISTRADOR', 'USUARIO_EDITAR'),
('ADMINISTRADOR', 'USUARIO_INHABILITAR'),
('SEGURIDAD', 'ROL_VER'),
('SEGURIDAD', 'ROL_CREAR'),
('SEGURIDAD', 'ROL_EDITAR'),
('SEGURIDAD', 'ROL_INHABILITAR'),
('SEGURIDAD', 'PERMISO_VER'),
('SEGURIDAD', 'PERMISO_ASIGNAR'),
('IDIOMAS_TRADUCCIONES', 'IDIOMA_VER'),
('IDIOMAS_TRADUCCIONES', 'IDIOMA_CREAR'),
('IDIOMAS_TRADUCCIONES', 'IDIOMA_EDITAR'),
('IDIOMAS_TRADUCCIONES', 'TRADUCCION_VER'),
('IDIOMAS_TRADUCCIONES', 'TRADUCCION_EDITAR'),
('AUDITORIA', 'BITACORA_VER'),
('AUDITORIA', 'AUDITORIA_CAMBIOS_VER');

INSERT INTO dbo.ComponentePermisoRelacion (id_padre, id_hijo)
SELECT padre.id_componente, hijo.id_componente
FROM @relaciones r
INNER JOIN dbo.ComponentePermiso padre
    ON padre.codigo = r.codigo_padre
INNER JOIN dbo.ComponentePermiso hijo
    ON hijo.codigo = r.codigo_hijo
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.ComponentePermisoRelacion existente
    WHERE existente.id_padre = padre.id_componente
      AND existente.id_hijo = hijo.id_componente
);
GO

IF NOT EXISTS (
    SELECT 1
    FROM dbo.Idioma
    WHERE codigo = 'es-AR'
)
BEGIN
    INSERT INTO dbo.Idioma (codigo, nombre, estado_idioma)
    VALUES ('es-AR', 'Espanol Argentina', 'ACTIVO');
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM dbo.Usuario
    WHERE nombre_usuario = 'admin'
       OR email = 'admin@tecnisalud.local'
)
BEGIN
    INSERT INTO dbo.Usuario
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
    FROM dbo.Idioma
    WHERE codigo = 'es-AR';
END
GO

IF EXISTS (
    SELECT 1
    FROM dbo.Usuario
    WHERE nombre_usuario = 'admin'
)
BEGIN
    UPDATE dbo.Usuario
    SET password_hash = '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',
        estado_usuario = 'ACTIVO'
    WHERE nombre_usuario = 'admin';
END
GO

INSERT INTO dbo.UsuarioComponentePermiso (id_usuario, id_componente)
SELECT ur.id_usuario, c.id_componente
FROM dbo.UsuarioRol ur
INNER JOIN dbo.Rol r
    ON r.id_rol = ur.id_rol
INNER JOIN dbo.ComponentePermiso c
    ON c.codigo = 'ADMINISTRADOR'
WHERE r.nombre = 'Administrador'
  AND UPPER(ur.estado_usuario_rol) = 'ACTIVO'
  AND NOT EXISTS (
      SELECT 1
      FROM dbo.UsuarioComponentePermiso activo
      WHERE activo.id_usuario = ur.id_usuario
        AND activo.estado_usuario_componente = 'ACTIVO'
  )
  AND NOT EXISTS (
      SELECT 1
      FROM dbo.UsuarioComponentePermiso existente
      WHERE existente.id_usuario = ur.id_usuario
        AND existente.id_componente = c.id_componente
  );
GO

UPDATE uc
SET estado_usuario_componente = 'INACTIVO'
FROM dbo.UsuarioComponentePermiso uc
INNER JOIN dbo.Usuario u
    ON u.id_usuario = uc.id_usuario
WHERE u.nombre_usuario = 'admin'
  AND uc.estado_usuario_componente = 'ACTIVO';
GO

IF EXISTS (
    SELECT 1
    FROM dbo.UsuarioComponentePermiso uc
    INNER JOIN dbo.Usuario u
        ON u.id_usuario = uc.id_usuario
    INNER JOIN dbo.ComponentePermiso c
        ON c.id_componente = uc.id_componente
    WHERE u.nombre_usuario = 'admin'
      AND c.codigo = 'ADMINISTRADOR'
)
BEGIN
    UPDATE uc
    SET estado_usuario_componente = 'ACTIVO'
    FROM dbo.UsuarioComponentePermiso uc
    INNER JOIN dbo.Usuario u
        ON u.id_usuario = uc.id_usuario
    INNER JOIN dbo.ComponentePermiso c
        ON c.id_componente = uc.id_componente
    WHERE u.nombre_usuario = 'admin'
      AND c.codigo = 'ADMINISTRADOR';
END
ELSE
BEGIN
    INSERT INTO dbo.UsuarioComponentePermiso (id_usuario, id_componente)
    SELECT u.id_usuario, c.id_componente
    FROM dbo.Usuario u
    INNER JOIN dbo.ComponentePermiso c
        ON c.codigo = 'ADMINISTRADOR'
    WHERE u.nombre_usuario = 'admin';
END
GO

IF OBJECT_ID('dbo.Etiqueta', 'U') IS NOT NULL
   AND OBJECT_ID('dbo.Traduccion', 'U') IS NOT NULL
   AND OBJECT_ID('dbo.Idioma', 'U') IS NOT NULL
BEGIN
    DECLARE @etiquetas_i18n TABLE (
        clave VARCHAR(150) NOT NULL,
        descripcion VARCHAR(255) NULL
    );

    INSERT INTO @etiquetas_i18n (clave, descripcion) VALUES
    ('MENU_CHANGE_AUDIT', 'Menu auditoria de cambios'),
    ('CHANGE_AUDIT_TITLE', 'Titulo auditoria de cambios'),
    ('CHANGE_AUDIT_DESCRIPTION', 'Descripcion auditoria de cambios'),
    ('CHANGE_AUDIT_EMPTY', 'Auditoria de cambios sin eventos'),
    ('CHANGE_AUDIT_COUNT', 'Cantidad de cambios auditados'),
    ('CHANGE_AUDIT_USER', 'Usuario auditado'),
    ('CHANGE_AUDIT_PREVIOUS_STATE', 'Estado anterior'),
    ('CHANGE_AUDIT_NEW_STATE', 'Estado nuevo'),
    ('AUDIT_FILTERS', 'Filtros de bitacora'),
    ('AUDIT_FILTER_FROM', 'Filtro fecha desde'),
    ('AUDIT_FILTER_TO', 'Filtro fecha hasta'),
    ('AUDIT_FILTER_USER', 'Filtro usuario'),
    ('AUDIT_FILTER_MODULE', 'Filtro modulo'),
    ('AUDIT_FILTER_ACTION', 'Filtro accion'),
    ('AUDIT_FILTER_LEVEL', 'Filtro nivel'),
    ('AUDIT_FILTER_DESCRIPTION', 'Filtro descripcion'),
    ('BTN_SEARCH', 'Boton buscar'),
    ('BTN_CLEAR_FILTERS', 'Boton limpiar filtros'),
    ('FILTER_ALL', 'Filtro todos'),
    ('FILTER_ALL_ACTIONS', 'Filtro todas las acciones'),
    ('BITACORA_MODULE_SECURITY', 'Modulo seguridad'),
    ('BITACORA_ACTION_LOGIN_SUCCESS', 'Accion login exitoso'),
    ('BITACORA_ACTION_LOGIN_FAILURE', 'Accion login fallido'),
    ('BITACORA_ACTION_REGISTER_FAILURE', 'Accion registro fallido'),
    ('BITACORA_LEVEL_INFORMATION', 'Nivel informacion'),
    ('BITACORA_LEVEL_WARNING', 'Nivel advertencia'),
    ('BITACORA_LEVEL_ERROR', 'Nivel error'),
    ('GRID_ENTITY', 'Columna entidad'),
    ('GRID_ENTITY_ID', 'Columna id entidad'),
    ('GRID_FIELD', 'Columna campo'),
    ('GRID_OLD_VALUE', 'Columna valor anterior'),
    ('GRID_NEW_VALUE', 'Columna valor nuevo'),
    ('GRID_DV_BLOCK', 'Columna bloqueo digito verificador'),
    ('BTN_RECALCULATE_DV', 'Boton recalcular digitos verificadores'),
    ('SECURITY_LANGUAGE_CREATE_DENIED', 'Creacion de idioma denegada'),
    ('SECURITY_LANGUAGE_EDIT_DENIED', 'Edicion de idioma denegada'),
    ('SECURITY_TRANSLATION_EDIT_DENIED', 'Edicion de traduccion denegada');

    MERGE dbo.Etiqueta AS destino
    USING @etiquetas_i18n AS origen
        ON destino.clave = origen.clave
    WHEN MATCHED THEN
        UPDATE SET descripcion = origen.descripcion
    WHEN NOT MATCHED THEN
        INSERT (clave, descripcion)
        VALUES (origen.clave, origen.descripcion);

    DECLARE @traducciones_i18n TABLE (
        codigo_idioma VARCHAR(10) NOT NULL,
        clave VARCHAR(150) NOT NULL,
        texto NVARCHAR(500) NOT NULL
    );

    INSERT INTO @traducciones_i18n (codigo_idioma, clave, texto) VALUES
    ('es-AR', 'MENU_CHANGE_AUDIT', 'Auditoria de cambios'),
    ('es-AR', 'CHANGE_AUDIT_TITLE', 'Auditoria de cambios'),
    ('es-AR', 'CHANGE_AUDIT_DESCRIPTION', 'Historial de cambios registrados sobre entidades auditadas.'),
    ('es-AR', 'CHANGE_AUDIT_EMPTY', 'No hay cambios registrados para el usuario seleccionado.'),
    ('es-AR', 'CHANGE_AUDIT_COUNT', '{0} cambio(s) registrado(s).'),
    ('es-AR', 'CHANGE_AUDIT_USER', 'Usuario auditado'),
    ('es-AR', 'CHANGE_AUDIT_PREVIOUS_STATE', 'Estado anterior'),
    ('es-AR', 'CHANGE_AUDIT_NEW_STATE', 'Estado nuevo'),
    ('es-AR', 'AUDIT_FILTERS', 'Filtros'),
    ('es-AR', 'AUDIT_FILTER_FROM', 'Desde'),
    ('es-AR', 'AUDIT_FILTER_TO', 'Hasta'),
    ('es-AR', 'AUDIT_FILTER_USER', 'Usuario'),
    ('es-AR', 'AUDIT_FILTER_MODULE', 'Modulo'),
    ('es-AR', 'AUDIT_FILTER_ACTION', 'Accion'),
    ('es-AR', 'AUDIT_FILTER_LEVEL', 'Nivel'),
    ('es-AR', 'AUDIT_FILTER_DESCRIPTION', 'Descripcion'),
    ('es-AR', 'BTN_SEARCH', 'Buscar'),
    ('es-AR', 'BTN_CLEAR_FILTERS', 'Limpiar'),
    ('es-AR', 'FILTER_ALL', 'Todos'),
    ('es-AR', 'FILTER_ALL_ACTIONS', 'Todas'),
    ('es-AR', 'BITACORA_MODULE_SECURITY', 'Seguridad'),
    ('es-AR', 'BITACORA_ACTION_LOGIN_SUCCESS', 'Login exitoso'),
    ('es-AR', 'BITACORA_ACTION_LOGIN_FAILURE', 'Login fallido'),
    ('es-AR', 'BITACORA_ACTION_REGISTER_FAILURE', 'Registro fallido'),
    ('es-AR', 'BITACORA_LEVEL_INFORMATION', 'Informacion'),
    ('es-AR', 'BITACORA_LEVEL_WARNING', 'Advertencia'),
    ('es-AR', 'BITACORA_LEVEL_ERROR', 'Error'),
    ('es-AR', 'GRID_ENTITY', 'Entidad'),
    ('es-AR', 'GRID_ENTITY_ID', 'Id entidad'),
    ('es-AR', 'GRID_FIELD', 'Campo'),
    ('es-AR', 'GRID_OLD_VALUE', 'Valor anterior'),
    ('es-AR', 'GRID_NEW_VALUE', 'Valor nuevo'),
    ('es-AR', 'GRID_DV_BLOCK', 'Bloqueo DV'),
    ('es-AR', 'BTN_RECALCULATE_DV', 'Recalcular DV'),
    ('es-AR', 'SECURITY_LANGUAGE_CREATE_DENIED', 'No tenes permisos para crear idiomas.'),
    ('es-AR', 'SECURITY_LANGUAGE_EDIT_DENIED', 'No tenes permisos para modificar idiomas.'),
    ('es-AR', 'SECURITY_TRANSLATION_EDIT_DENIED', 'No tenes permisos para modificar traducciones.'),
    ('en-US', 'SECURITY_LANGUAGE_CREATE_DENIED', 'You do not have permission to create languages.'),
    ('en-US', 'MENU_CHANGE_AUDIT', 'Change audit'),
    ('en-US', 'CHANGE_AUDIT_TITLE', 'Change audit'),
    ('en-US', 'CHANGE_AUDIT_DESCRIPTION', 'History of recorded changes on audited entities.'),
    ('en-US', 'CHANGE_AUDIT_EMPTY', 'No changes registered for the selected user.'),
    ('en-US', 'CHANGE_AUDIT_COUNT', '{0} change(s) registered.'),
    ('en-US', 'CHANGE_AUDIT_USER', 'Audited user'),
    ('en-US', 'CHANGE_AUDIT_PREVIOUS_STATE', 'Previous state'),
    ('en-US', 'CHANGE_AUDIT_NEW_STATE', 'New state'),
    ('en-US', 'AUDIT_FILTERS', 'Filters'),
    ('en-US', 'AUDIT_FILTER_FROM', 'From'),
    ('en-US', 'AUDIT_FILTER_TO', 'To'),
    ('en-US', 'AUDIT_FILTER_USER', 'User'),
    ('en-US', 'AUDIT_FILTER_MODULE', 'Module'),
    ('en-US', 'AUDIT_FILTER_ACTION', 'Action'),
    ('en-US', 'AUDIT_FILTER_LEVEL', 'Level'),
    ('en-US', 'AUDIT_FILTER_DESCRIPTION', 'Description'),
    ('en-US', 'BTN_SEARCH', 'Search'),
    ('en-US', 'BTN_CLEAR_FILTERS', 'Clear'),
    ('en-US', 'FILTER_ALL', 'All'),
    ('en-US', 'FILTER_ALL_ACTIONS', 'All'),
    ('en-US', 'BITACORA_MODULE_SECURITY', 'Security'),
    ('en-US', 'BITACORA_ACTION_LOGIN_SUCCESS', 'Successful login'),
    ('en-US', 'BITACORA_ACTION_LOGIN_FAILURE', 'Failed login'),
    ('en-US', 'BITACORA_ACTION_REGISTER_FAILURE', 'Failed registration'),
    ('en-US', 'BITACORA_LEVEL_INFORMATION', 'Information'),
    ('en-US', 'BITACORA_LEVEL_WARNING', 'Warning'),
    ('en-US', 'BITACORA_LEVEL_ERROR', 'Error'),
    ('en-US', 'GRID_ENTITY', 'Entity'),
    ('en-US', 'GRID_ENTITY_ID', 'Entity id'),
    ('en-US', 'GRID_FIELD', 'Field'),
    ('en-US', 'GRID_OLD_VALUE', 'Old value'),
    ('en-US', 'GRID_NEW_VALUE', 'New value'),
    ('en-US', 'GRID_DV_BLOCK', 'DV block'),
    ('en-US', 'BTN_RECALCULATE_DV', 'Recalculate DV'),
    ('en-US', 'SECURITY_LANGUAGE_EDIT_DENIED', 'You do not have permission to modify languages.'),
    ('en-US', 'SECURITY_TRANSLATION_EDIT_DENIED', 'You do not have permission to modify translations.');

    MERGE dbo.Traduccion AS destino
    USING (
        SELECT
            e.id_etiqueta,
            i.id_idioma,
            t.texto
        FROM @traducciones_i18n t
        INNER JOIN dbo.Etiqueta e
            ON e.clave = t.clave
        INNER JOIN dbo.Idioma i
            ON i.codigo = t.codigo_idioma
    ) AS origen
        ON destino.id_etiqueta = origen.id_etiqueta
       AND destino.id_idioma = origen.id_idioma
    WHEN MATCHED THEN
        UPDATE SET texto = origen.texto
    WHEN NOT MATCHED THEN
        INSERT (id_etiqueta, id_idioma, texto)
        VALUES (origen.id_etiqueta, origen.id_idioma, origen.texto);
END
GO
