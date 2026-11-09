-- Catalogos PN1. Ejecutar despues de 05_IntegridadPN1.sql y antes de 07_PN1.sql.
-- Preserva las opciones y los valores historicos de una base existente.
USE [HemovidaGest];
GO
SET XACT_ABORT ON;
BEGIN TRY
 BEGIN TRANSACTION;
 IF OBJECT_ID('dbo.sp_IntegridadPN1_Exigir','P') IS NULL THROW 51010,'MAESTROS_REQUIERE_INTEGRIDAD_PN1',1;
 EXEC dbo.sp_IntegridadPN1_Exigir;
 IF OBJECT_ID('dbo.ComponenteTransfusional','U') IS NULL
  CREATE TABLE dbo.ComponenteTransfusional(id INT IDENTITY PRIMARY KEY,codigo VARCHAR(100) NOT NULL UNIQUE,descripcion NVARCHAR(150) NOT NULL,activo BIT NOT NULL DEFAULT 1);
 IF OBJECT_ID('dbo.GrupoSanguineo','U') IS NULL
  CREATE TABLE dbo.GrupoSanguineo(id INT IDENTITY PRIMARY KEY,codigo VARCHAR(5) NOT NULL UNIQUE,descripcion NVARCHAR(150) NOT NULL,activo BIT NOT NULL DEFAULT 1);
 IF OBJECT_ID('dbo.FactorRh','U') IS NULL
  CREATE TABLE dbo.FactorRh(id INT IDENTITY PRIMARY KEY,codigo VARCHAR(5) NOT NULL UNIQUE,descripcion NVARCHAR(150) NOT NULL,activo BIT NOT NULL DEFAULT 1);
 DECLARE @controlNuevo BIT=0;
 IF OBJECT_ID('dbo.MaestrosPN1Control','U') IS NULL
 BEGIN
  CREATE TABLE dbo.MaestrosPN1Control(id INT NOT NULL PRIMARY KEY CHECK(id=1),huella VARCHAR(64) NOT NULL);
  SET @controlNuevo=1;
 END;

 DECLARE @hash VARCHAR(64), @destinoExistia BIT=CASE WHEN OBJECT_ID('dbo.DestinoEntrega','U') IS NULL THEN 0 ELSE 1 END,
         @estadoUnidadExistia BIT=CASE WHEN OBJECT_ID('dbo.EstadoUnidad','U') IS NULL THEN 0 ELSE 1 END,
         @jsonDestinos NVARCHAR(MAX)=N'',@jsonEstados NVARCHAR(MAX)=N'';
 IF @destinoExistia=1
 BEGIN
  IF COL_LENGTH('dbo.DestinoEntrega','estado_operativo') IS NOT NULL
   EXEC sys.sp_executesql N'SELECT @j=COALESCE((SELECT id,codigo,descripcion,activo,estado_operativo,motivo_obligatorio FROM dbo.DestinoEntrega ORDER BY id FOR JSON PATH,INCLUDE_NULL_VALUES),N''[]'')',N'@j NVARCHAR(MAX) OUTPUT',@j=@jsonDestinos OUTPUT;
  ELSE
   EXEC sys.sp_executesql N'SELECT @j=COALESCE((SELECT id,codigo,descripcion,activo,motivo_obligatorio FROM dbo.DestinoEntrega ORDER BY id FOR JSON PATH,INCLUDE_NULL_VALUES),N''[]'')',N'@j NVARCHAR(MAX) OUTPUT',@j=@jsonDestinos OUTPUT;
 END;
 IF @estadoUnidadExistia=1
 BEGIN
  SELECT @jsonEstados=COALESCE((SELECT codigo,descripcion FROM dbo.EstadoUnidad ORDER BY codigo FOR JSON PATH,INCLUDE_NULL_VALUES),N'[]');
  SELECT @hash=CONVERT(VARCHAR(64),HASHBYTES('SHA2_256',CONVERT(NVARCHAR(MAX),
   (SELECT id,codigo,descripcion,activo FROM dbo.ComponenteTransfusional ORDER BY id FOR JSON PATH,INCLUDE_NULL_VALUES) + N'|'+
   (SELECT id,codigo,descripcion,activo FROM dbo.GrupoSanguineo ORDER BY id FOR JSON PATH,INCLUDE_NULL_VALUES) + N'|'+
   (SELECT id,codigo,descripcion,activo FROM dbo.FactorRh ORDER BY id FOR JSON PATH,INCLUDE_NULL_VALUES) + N'|'+@jsonEstados+N'|'+@jsonDestinos+N'|'+
   COALESCE((SELECT id_auditoria,entidad,id_entidad,accion,id_usuario_actor,identificador_usuario_actor,fecha_evento,estado_anterior_json,estado_nuevo_json,cambios_json FROM dbo.Auditoria WHERE entidad IN ('MaestroComponente','MaestroGrupo','MaestroRh','MaestroDestino') ORDER BY id_auditoria FOR JSON PATH,INCLUDE_NULL_VALUES),N'[]'))),2);
 END
 ELSE
  SELECT @hash=CONVERT(VARCHAR(64),HASHBYTES('SHA2_256',CONVERT(NVARCHAR(MAX),
   (SELECT id,codigo,descripcion,activo FROM dbo.ComponenteTransfusional ORDER BY id FOR JSON PATH,INCLUDE_NULL_VALUES) + N'|'+
   (SELECT id,codigo,descripcion,activo FROM dbo.GrupoSanguineo ORDER BY id FOR JSON PATH,INCLUDE_NULL_VALUES) + N'|'+
   (SELECT id,codigo,descripcion,activo FROM dbo.FactorRh ORDER BY id FOR JSON PATH,INCLUDE_NULL_VALUES) +
   CASE WHEN @destinoExistia=1 THEN N'|'+@jsonDestinos ELSE N'' END + N'|'+
   COALESCE((SELECT id_auditoria,entidad,id_entidad,accion,id_usuario_actor,identificador_usuario_actor,fecha_evento,estado_anterior_json,estado_nuevo_json,cambios_json FROM dbo.Auditoria WHERE entidad IN ('MaestroComponente','MaestroGrupo','MaestroRh') OR (@destinoExistia=1 AND entidad='MaestroDestino') ORDER BY id_auditoria FOR JSON PATH,INCLUDE_NULL_VALUES),N'[]'))),2);
 IF @controlNuevo=0
    AND NOT EXISTS(SELECT 1 FROM dbo.MaestrosPN1Control WHERE id=1 AND huella=@hash)
  THROW 51014,'MAESTROS_INTEGRIDAD_INVALIDA',1;

 IF OBJECT_ID('dbo.EstadoUnidad','U') IS NULL
  CREATE TABLE dbo.EstadoUnidad(codigo VARCHAR(20) NOT NULL CONSTRAINT PK_EstadoUnidad PRIMARY KEY,descripcion NVARCHAR(150) NOT NULL);
 INSERT dbo.EstadoUnidad(codigo,descripcion)
 SELECT v.codigo,v.descripcion FROM (VALUES
  ('EN_REVISION',N'En revisión'),('LIBERADA',N'Liberada'),('BLOQUEADA',N'Bloqueada'),('DESCARTADA',N'Descartada'),
  ('RESERVADA',N'Reservada'),('ENTREGADA',N'Entregada'),('TRANSFUNDIDA',N'Transfundida'),('DEVUELTA',N'Devuelta'),('ANULADA',N'Anulada')) v(codigo,descripcion)
 WHERE NOT EXISTS(SELECT 1 FROM dbo.EstadoUnidad e WHERE e.codigo=v.codigo);
 IF OBJECT_ID('dbo.DestinoEntrega','U') IS NULL
  CREATE TABLE dbo.DestinoEntrega(id INT IDENTITY PRIMARY KEY,codigo VARCHAR(20) NOT NULL UNIQUE,descripcion NVARCHAR(150) NOT NULL,
   activo BIT NOT NULL DEFAULT 1,motivo_obligatorio BIT NOT NULL,
   CONSTRAINT CK_DestinoEntrega_Codigo CHECK(codigo<>'SIN_DESTINO'),
   CONSTRAINT CK_DestinoEntrega_Motivo CHECK(codigo NOT IN ('DEVUELTA','DESCARTADA') OR motivo_obligatorio=1));
 IF COL_LENGTH('dbo.DestinoEntrega','estado_operativo') IS NOT NULL
  EXEC sys.sp_executesql N'
   INSERT dbo.DestinoEntrega(codigo,descripcion,estado_operativo,motivo_obligatorio)
   SELECT v.codigo,v.descripcion,v.codigo,v.motivo FROM (VALUES
    (''TRANSFUNDIDA'',N''Transfundida'',CONVERT(BIT,0)),(''DEVUELTA'',N''Devuelta'',CONVERT(BIT,1)),
    (''DESCARTADA'',N''Descartada'',CONVERT(BIT,1))) v(codigo,descripcion,motivo)
   WHERE NOT EXISTS(SELECT 1 FROM dbo.DestinoEntrega d WHERE d.codigo=v.codigo);';
 ELSE
  INSERT dbo.DestinoEntrega(codigo,descripcion,motivo_obligatorio)
  SELECT v.codigo,v.descripcion,v.motivo FROM (VALUES
   ('TRANSFUNDIDA',N'Transfundida',CONVERT(BIT,0)),('DEVUELTA',N'Devuelta',CONVERT(BIT,1)),
   ('DESCARTADA',N'Descartada',CONVERT(BIT,1))) v(codigo,descripcion,motivo)
  WHERE NOT EXISTS(SELECT 1 FROM dbo.DestinoEntrega d WHERE d.codigo=v.codigo);
 INSERT dbo.EstadoUnidad(codigo,descripcion)
 SELECT d.codigo,d.descripcion FROM dbo.DestinoEntrega d
 WHERE NOT EXISTS(SELECT 1 FROM dbo.EstadoUnidad e WHERE e.codigo=d.codigo);
 INSERT dbo.EstadoUnidad(codigo,descripcion)
 SELECT DISTINCT u.estado_operativo,u.estado_operativo FROM dbo.Unidad u
 WHERE NOT EXISTS(SELECT 1 FROM dbo.EstadoUnidad e WHERE e.codigo=u.estado_operativo);
 INSERT dbo.EstadoUnidad(codigo,descripcion)
 SELECT DISTINCT m.estado_nuevo,m.estado_nuevo FROM dbo.MovimientoUnidad m
 WHERE NOT EXISTS(SELECT 1 FROM dbo.EstadoUnidad e WHERE e.codigo=m.estado_nuevo);
 INSERT dbo.EstadoUnidad(codigo,descripcion)
 SELECT DISTINCT m.estado_anterior,m.estado_anterior FROM dbo.MovimientoUnidad m
 WHERE m.estado_anterior IS NOT NULL AND NOT EXISTS(SELECT 1 FROM dbo.EstadoUnidad e WHERE e.codigo=m.estado_anterior);
 IF OBJECT_ID('dbo.CK_DestinoEntrega_Estado','C') IS NOT NULL ALTER TABLE dbo.DestinoEntrega DROP CONSTRAINT CK_DestinoEntrega_Estado;
 IF OBJECT_ID('dbo.CK_DestinoEntrega_Motivo','C') IS NOT NULL ALTER TABLE dbo.DestinoEntrega DROP CONSTRAINT CK_DestinoEntrega_Motivo;
 ALTER TABLE dbo.DestinoEntrega WITH CHECK ADD CONSTRAINT CK_DestinoEntrega_Motivo CHECK(codigo NOT IN ('DEVUELTA','DESCARTADA') OR motivo_obligatorio=1);
 IF COL_LENGTH('dbo.DestinoEntrega','estado_operativo') IS NOT NULL
 BEGIN
  DECLARE @legacyFk sysname,@legacyDefault sysname,@legacyDdl NVARCHAR(500);
  SELECT TOP(1) @legacyFk=fk.name FROM sys.foreign_keys fk
   JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id=fk.object_id
   WHERE fkc.parent_object_id=OBJECT_ID('dbo.DestinoEntrega')
     AND fkc.parent_column_id=COLUMNPROPERTY(OBJECT_ID('dbo.DestinoEntrega'),'estado_operativo','ColumnId');
  IF @legacyFk IS NOT NULL
  BEGIN
   SET @legacyDdl=N'ALTER TABLE dbo.DestinoEntrega DROP CONSTRAINT '+QUOTENAME(@legacyFk);
   EXEC sys.sp_executesql @legacyDdl;
  END;
  SELECT @legacyDefault=dc.name FROM sys.default_constraints dc
   JOIN sys.columns c ON c.object_id=dc.parent_object_id AND c.column_id=dc.parent_column_id
   WHERE dc.parent_object_id=OBJECT_ID('dbo.DestinoEntrega') AND c.name='estado_operativo';
  IF @legacyDefault IS NOT NULL
  BEGIN
   SET @legacyDdl=N'ALTER TABLE dbo.DestinoEntrega DROP CONSTRAINT '+QUOTENAME(@legacyDefault);
   EXEC sys.sp_executesql @legacyDdl;
  END;
  ALTER TABLE dbo.DestinoEntrega DROP COLUMN estado_operativo;
 END;
 IF OBJECT_ID('dbo.CK_Unidad_Estado','C') IS NOT NULL ALTER TABLE dbo.Unidad DROP CONSTRAINT CK_Unidad_Estado;
 IF OBJECT_ID('dbo.CK_MovimientoUnidad_EstadoNuevo','C') IS NOT NULL ALTER TABLE dbo.MovimientoUnidad DROP CONSTRAINT CK_MovimientoUnidad_EstadoNuevo;
 IF OBJECT_ID('dbo.CK_MovimientoUnidad_Tipo','C') IS NOT NULL ALTER TABLE dbo.MovimientoUnidad DROP CONSTRAINT CK_MovimientoUnidad_Tipo;
 ALTER TABLE dbo.MovimientoUnidad ADD CONSTRAINT CK_MovimientoUnidad_Tipo CHECK
  (tipo_movimiento IN ('REGISTRO','CLASIFICACION','TIPIFICACION_DONANTE','LIBERACION','BLOQUEO','DESCARTE','RESTAURACION','UBICACION','RESERVA','ENTREGA','TRANSFUSION','DEVOLUCION','CANCELACION','REINGRESO','DESTINO','ANULACION_CANTIDAD'));
 IF OBJECT_ID('dbo.FK_DestinoEntrega_Codigo_EstadoUnidad','F') IS NULL ALTER TABLE dbo.DestinoEntrega WITH CHECK ADD CONSTRAINT FK_DestinoEntrega_Codigo_EstadoUnidad FOREIGN KEY(codigo) REFERENCES dbo.EstadoUnidad(codigo);
 IF OBJECT_ID('dbo.FK_Unidad_EstadoUnidad','F') IS NULL ALTER TABLE dbo.Unidad WITH CHECK ADD CONSTRAINT FK_Unidad_EstadoUnidad FOREIGN KEY(estado_operativo) REFERENCES dbo.EstadoUnidad(codigo);
 IF OBJECT_ID('dbo.FK_MovimientoUnidad_EstadoAnterior','F') IS NULL ALTER TABLE dbo.MovimientoUnidad WITH CHECK ADD CONSTRAINT FK_MovimientoUnidad_EstadoAnterior FOREIGN KEY(estado_anterior) REFERENCES dbo.EstadoUnidad(codigo);
 IF OBJECT_ID('dbo.FK_MovimientoUnidad_EstadoNuevo','F') IS NULL ALTER TABLE dbo.MovimientoUnidad WITH CHECK ADD CONSTRAINT FK_MovimientoUnidad_EstadoNuevo FOREIGN KEY(estado_nuevo) REFERENCES dbo.EstadoUnidad(codigo);
 IF OBJECT_ID('dbo.Entrega','U') IS NOT NULL
  EXEC sys.sp_executesql N'
   IF OBJECT_ID(''dbo.CK_Entrega_Destino'',''C'') IS NOT NULL ALTER TABLE dbo.Entrega DROP CONSTRAINT CK_Entrega_Destino;
   IF OBJECT_ID(''dbo.FK_Entrega_Destino'',''F'') IS NULL ALTER TABLE dbo.Entrega WITH CHECK ADD CONSTRAINT FK_Entrega_Destino FOREIGN KEY(destino) REFERENCES dbo.DestinoEntrega(codigo);';

 INSERT dbo.ComponenteTransfusional(codigo,descripcion)
 SELECT v.codigo,v.codigo FROM (VALUES ('Concentrado de eritrocitos'),('Plasma fresco congelado'),('Concentrado de plaquetas')) v(codigo)
 WHERE NOT EXISTS(SELECT 1 FROM dbo.ComponenteTransfusional c WHERE c.codigo=v.codigo);
 INSERT dbo.GrupoSanguineo(codigo,descripcion)
 SELECT v.codigo,v.codigo FROM (VALUES ('A'),('B'),('AB'),('O')) v(codigo)
 WHERE NOT EXISTS(SELECT 1 FROM dbo.GrupoSanguineo c WHERE c.codigo=v.codigo);
 INSERT dbo.FactorRh(codigo,descripcion)
 SELECT v.codigo,v.descripcion FROM (VALUES ('+',N'Positivo'),('-',N'Negativo')) v(codigo,descripcion)
 WHERE NOT EXISTS(SELECT 1 FROM dbo.FactorRh c WHERE c.codigo=v.codigo);

 IF EXISTS(SELECT 1 FROM dbo.Donacion WHERE NULLIF(LTRIM(RTRIM(tipo_componente)),'') IS NULL)
    OR EXISTS(SELECT 1 FROM dbo.Unidad WHERE tipo_componente IS NOT NULL AND NULLIF(LTRIM(RTRIM(tipo_componente)),'') IS NULL)
  THROW 51013,'MAESTROS_COMPONENTE_HISTORICO_INVALIDO',1;
 INSERT dbo.ComponenteTransfusional(codigo,descripcion,activo)
 SELECT DISTINCT h.codigo,h.codigo,0 FROM
 (SELECT tipo_componente codigo FROM dbo.Donacion UNION SELECT tipo_componente FROM dbo.Unidad WHERE tipo_componente IS NOT NULL) h
 WHERE NOT EXISTS(SELECT 1 FROM dbo.ComponenteTransfusional c WHERE c.codigo=h.codigo);
 INSERT dbo.GrupoSanguineo(codigo,descripcion,activo)
 SELECT DISTINCT u.grupo_sanguineo,u.grupo_sanguineo,0 FROM dbo.Unidad u
 WHERE u.grupo_sanguineo IS NOT NULL AND NOT EXISTS(SELECT 1 FROM dbo.GrupoSanguineo c WHERE c.codigo=u.grupo_sanguineo);
 INSERT dbo.FactorRh(codigo,descripcion,activo)
 SELECT DISTINCT u.factor_rh,u.factor_rh,0 FROM dbo.Unidad u
 WHERE u.factor_rh IS NOT NULL AND NOT EXISTS(SELECT 1 FROM dbo.FactorRh c WHERE c.codigo=u.factor_rh);

 -- PN2 puede contener solicitudes antiguas que no aparecen en las unidades.
 IF OBJECT_ID('dbo.SolicitudTransfusional','U') IS NOT NULL
  EXEC sys.sp_executesql N'
   IF EXISTS(SELECT 1 FROM dbo.SolicitudTransfusional WHERE
      CONVERT(NVARCHAR(100),CONVERT(VARCHAR(100),tipo_componente)) COLLATE Latin1_General_100_BIN2 <> tipo_componente COLLATE Latin1_General_100_BIN2)
    THROW 51013,''MAESTROS_COMPONENTE_PN2_NO_REPRESENTABLE'',1;
   INSERT dbo.ComponenteTransfusional(codigo,descripcion,activo)
   SELECT DISTINCT CONVERT(VARCHAR(100),s.tipo_componente),s.tipo_componente,0 FROM dbo.SolicitudTransfusional s
   WHERE NULLIF(LTRIM(RTRIM(s.tipo_componente)),N'''') IS NOT NULL
     AND NOT EXISTS(SELECT 1 FROM dbo.ComponenteTransfusional c WHERE c.codigo=s.tipo_componente);
   INSERT dbo.GrupoSanguineo(codigo,descripcion,activo)
   SELECT DISTINCT s.grupo_receptor,s.grupo_receptor,0 FROM dbo.SolicitudTransfusional s
   WHERE NULLIF(LTRIM(RTRIM(s.grupo_receptor)),'''') IS NOT NULL
     AND NOT EXISTS(SELECT 1 FROM dbo.GrupoSanguineo c WHERE c.codigo=s.grupo_receptor);
   INSERT dbo.FactorRh(codigo,descripcion,activo)
   SELECT DISTINCT s.rh_receptor,s.rh_receptor,0 FROM dbo.SolicitudTransfusional s
   WHERE NULLIF(LTRIM(RTRIM(s.rh_receptor)),'''') IS NOT NULL
     AND NOT EXISTS(SELECT 1 FROM dbo.FactorRh c WHERE c.codigo=s.rh_receptor);';

 SELECT @hash=CONVERT(VARCHAR(64),HASHBYTES('SHA2_256',CONVERT(NVARCHAR(MAX),
   (SELECT id,codigo,descripcion,activo FROM dbo.ComponenteTransfusional ORDER BY id FOR JSON PATH,INCLUDE_NULL_VALUES) + N'|'+
   (SELECT id,codigo,descripcion,activo FROM dbo.GrupoSanguineo ORDER BY id FOR JSON PATH,INCLUDE_NULL_VALUES) + N'|'+
   (SELECT id,codigo,descripcion,activo FROM dbo.FactorRh ORDER BY id FOR JSON PATH,INCLUDE_NULL_VALUES) + N'|'+
   COALESCE((SELECT codigo,descripcion FROM dbo.EstadoUnidad ORDER BY codigo FOR JSON PATH,INCLUDE_NULL_VALUES),N'[]') + N'|'+
   COALESCE((SELECT id,codigo,descripcion,activo,motivo_obligatorio FROM dbo.DestinoEntrega ORDER BY id FOR JSON PATH,INCLUDE_NULL_VALUES),N'[]') + N'|'+
   COALESCE((SELECT id_auditoria,entidad,id_entidad,accion,id_usuario_actor,identificador_usuario_actor,fecha_evento,estado_anterior_json,estado_nuevo_json,cambios_json FROM dbo.Auditoria WHERE entidad IN ('MaestroComponente','MaestroGrupo','MaestroRh','MaestroDestino') ORDER BY id_auditoria FOR JSON PATH,INCLUDE_NULL_VALUES),N'[]'))),2);
 IF @controlNuevo=1 INSERT dbo.MaestrosPN1Control(id,huella) VALUES(1,@hash);
 ELSE UPDATE dbo.MaestrosPN1Control SET huella=@hash WHERE id=1;

 IF OBJECT_ID('dbo.CK_Unidad_Grupo','C') IS NOT NULL ALTER TABLE dbo.Unidad DROP CONSTRAINT CK_Unidad_Grupo;
 IF OBJECT_ID('dbo.CK_Unidad_Rh','C') IS NOT NULL ALTER TABLE dbo.Unidad DROP CONSTRAINT CK_Unidad_Rh;
 IF OBJECT_ID('dbo.FK_Donacion_Componente','F') IS NULL ALTER TABLE dbo.Donacion WITH CHECK ADD CONSTRAINT FK_Donacion_Componente FOREIGN KEY(tipo_componente) REFERENCES dbo.ComponenteTransfusional(codigo);
 IF OBJECT_ID('dbo.FK_Unidad_Componente','F') IS NULL ALTER TABLE dbo.Unidad WITH CHECK ADD CONSTRAINT FK_Unidad_Componente FOREIGN KEY(tipo_componente) REFERENCES dbo.ComponenteTransfusional(codigo);
 IF OBJECT_ID('dbo.FK_Unidad_Grupo','F') IS NULL ALTER TABLE dbo.Unidad WITH CHECK ADD CONSTRAINT FK_Unidad_Grupo FOREIGN KEY(grupo_sanguineo) REFERENCES dbo.GrupoSanguineo(codigo);
 IF OBJECT_ID('dbo.FK_Unidad_Rh','F') IS NULL ALTER TABLE dbo.Unidad WITH CHECK ADD CONSTRAINT FK_Unidad_Rh FOREIGN KEY(factor_rh) REFERENCES dbo.FactorRh(codigo);

 IF NOT EXISTS(SELECT 1 FROM dbo.Permiso WHERE codigo='MAESTROS_PN1_ADMINISTRAR')
  INSERT dbo.Permiso(codigo,nombre,descripcion,modulo,accion,estado_permiso) VALUES('MAESTROS_PN1_ADMINISTRAR','Administrar maestros PN1','Administrar catalogos del banco de sangre','PN1','ADMINISTRAR','ACTIVO');
 IF NOT EXISTS(SELECT 1 FROM dbo.ComponentePermiso WHERE codigo='MAESTROS_PN1_ADMINISTRAR')
  INSERT dbo.ComponentePermiso(codigo,nombre,descripcion,tipo,estado_componente) VALUES('MAESTROS_PN1_ADMINISTRAR','Administrar maestros PN1','Administrar catalogos del banco de sangre','PERMISO','ACTIVO');
 INSERT dbo.ComponentePermisoRelacion(id_padre,id_hijo)
 SELECT a.id_componente,p.id_componente FROM dbo.ComponentePermiso a CROSS JOIN dbo.ComponentePermiso p
 WHERE a.codigo='ADMINISTRADOR' AND p.codigo='MAESTROS_PN1_ADMINISTRAR'
 AND NOT EXISTS(SELECT 1 FROM dbo.ComponentePermisoRelacion r WHERE r.id_padre=a.id_componente AND r.id_hijo=p.id_componente);
 INSERT dbo.RolPermiso(id_rol,id_permiso)
 SELECT r.id_rol,p.id_permiso FROM dbo.Rol r CROSS JOIN dbo.Permiso p
 WHERE r.nombre='Administrador' AND p.codigo='MAESTROS_PN1_ADMINISTRAR'
 AND NOT EXISTS(SELECT 1 FROM dbo.RolPermiso x WHERE x.id_rol=r.id_rol AND x.id_permiso=p.id_permiso);
 DECLARE @textos TABLE(clave VARCHAR(150),es NVARCHAR(500),en NVARCHAR(500));
 INSERT @textos VALUES
 ('MASTERS_TITLE',N'Maestros',N'Master data'),
 ('MASTERS_COMPONENTS',N'Componentes',N'Components'),
 ('MASTERS_GROUPS',N'Grupos sanguíneos',N'Blood groups'),
 ('MASTERS_RH',N'Factor Rh',N'Rh factor'),
 ('MASTERS_CODE',N'Código',N'Code'),
 ('MASTERS_DESCRIPTION',N'Descripción',N'Description'),
 ('MASTERS_ACTIVE',N'Activo',N'Active'),
 ('MASTERS_SAVE',N'Guardar',N'Save'),
 ('MASTERS_DEACTIVATE',N'Dar de baja',N'Deactivate'),
 ('MASTERS_SELECT',N'Seleccioná un registro.',N'Select a record.'),
 ('MASTERS_LOAD_ERROR',N'No se pudieron cargar los maestros. Verificá la instalación y la conexión con la base de datos.',N'Master data could not be loaded. Check the installation and database connection.'),
 ('MASTERS_RESULT_OK',N'Los cambios se guardaron.',N'Changes saved.'),
 ('MASTERS_RESULT_DATOS_INVALIDOS',N'Completá un código y una descripción válidos.',N'Enter a valid code and description.'),
 ('MASTERS_RESULT_DUPLICADO',N'Ya existe un registro con ese código.',N'A record with that code already exists.'),
 ('MASTERS_RESULT_NO_AUTORIZADO',N'No tenés permiso para administrar maestros.',N'You do not have permission to manage master data.'),
 ('MASTERS_RESULT_NO_ENCONTRADO',N'El registro cambió o ya no existe. Actualizá la lista.',N'The record changed or no longer exists. Refresh the list.'),
 ('MASTERS_RESULT_INTEGRIDAD_INVALIDA',N'La operación está bloqueada por integridad.',N'The operation is blocked by the integrity check.'),
 ('MASTERS_RESULT_ERROR',N'No se pudieron guardar los cambios.',N'The changes could not be saved.'),
 ('MASTERS_COMPATIBILITY_NOTE',N'Las opciones sin una regla de compatibilidad conocida no generan unidades candidatas en PN2.',N'Options without a known compatibility rule do not produce candidate units in PN2.'),
 ('MASTERS_HELP',N'Seleccioná una fila para editarla o elegí Nuevo para agregar una opción. Los códigos se conservan en el historial.',N'Select a row to edit it or choose New to add an option. Codes are preserved in history.'),
 ('MASTERS_DESTINATIONS',N'Destinos de entrega',N'Delivery states'),
 ('MASTERS_DESTINATION_STATE',N'Estado (código)',N'State (code)'),
 ('MASTERS_REASON_REQUIRED',N'Motivo obligatorio',N'Reason required'),
 ('MASTERS_DESTINATION_NOTE',N'El código es un estado propio de la unidad y queda fijo al dar de alta el destino.',N'The code is an independent unit state and is fixed when the destination is created.'),
 ('PN2_QUANTITY_NOTE',N'Se permite atención parcial. Solo Transfundida cubre el pedido; cualquier otro estado de destino reabre el saldo y queda en el historial.',N'Partial fulfillment is allowed. Only Transfused fulfills the request; any other delivery state reopens the balance and remains in history.'),
 ('PN2_DESTINATIONS_DESCRIPTION',N'Elegí el estado de destino configurado en Maestros. Transfundida cubre el pedido; los demás estados quedan en el historial y reabren el saldo.',N'Choose a delivery state configured in Master Data. Transfused fulfills the request; other states remain in history and reopen the balance.'),
 ('PN2_DESTINATION_NOTE',N'Devolución y descarte requieren motivo. El motivo de otros estados se configura en Maestros. Solo DEVUELTA habilita el reingreso autorizado de la unidad.',N'Returns and discards require a reason. The reason rule for other states is configured in Master Data. Only DEVUELTA enables authorized unit reentry.'),
 ('PN2_CHOOSE_DESTINATION',N'Elegí un destino para la entrega.',N'Choose a delivery state.'),
 ('PN2_DEST_EFFECT_REQUIRED',N'La unidad quedará en estado {0}. El motivo es obligatorio.',N'The unit will move to state {0}. A reason is required.'),
 ('PN2_DEST_EFFECT_OPTIONAL',N'La unidad quedará en estado {0}. El motivo es opcional.',N'The unit will move to state {0}. A reason is optional.'),
 ('PN2_DESTINATION_ALREADY_SET',N'Esta entrega ya tiene un destino registrado.',N'This delivery already has a recorded state.'),
 ('INTEGRITY_TYPE_MAESTROS_INVALIDOS',N'Maestros alterados; recuperar desde una copia confiable',N'Master data changed; restore from a trusted backup');
 INSERT dbo.Etiqueta(clave,descripcion)
 SELECT t.clave,t.clave FROM @textos t WHERE NOT EXISTS(SELECT 1 FROM dbo.Etiqueta e WHERE e.clave=t.clave);
 INSERT dbo.Traduccion(id_etiqueta,id_idioma,texto)
 SELECT e.id_etiqueta,i.id_idioma,CASE WHEN i.codigo='en-US' THEN t.en ELSE t.es END
 FROM @textos t JOIN dbo.Etiqueta e ON e.clave=t.clave JOIN dbo.Idioma i ON i.codigo IN ('es-AR','en-US')
 WHERE NOT EXISTS(SELECT 1 FROM dbo.Traduccion d WHERE d.id_etiqueta=e.id_etiqueta AND d.id_idioma=i.id_idioma);
 DECLARE @textosAnteriores TABLE(clave VARCHAR(150),esAnterior NVARCHAR(500),enAnterior NVARCHAR(500),esNuevo NVARCHAR(500),enNuevo NVARCHAR(500));
 INSERT @textosAnteriores VALUES
 ('MASTERS_DESTINATIONS',N'Destinos de entrega',N'Delivery outcomes',N'Destinos de entrega',N'Delivery states'),
 ('MASTERS_DESTINATION_NOTE',N'El resultado define el estado de la unidad y queda fijo al dar de alta el destino.',N'The outcome defines the unit status and is fixed when the destination is created.',N'El código es un estado propio de la unidad y queda fijo al dar de alta el destino.',N'The code is an independent unit state and is fixed when the destination is created.'),
 ('PN2_CHOOSE_DESTINATION',N'Elegí un destino para la entrega.',N'Choose a delivery outcome.',N'Elegí un destino para la entrega.',N'Choose a delivery state.'),
 ('PN2_DEST_EFFECT_REQUIRED',N'La unidad quedará: {0}. El motivo es obligatorio.',N'Unit status: {0}. A reason is required.',N'La unidad quedará en estado {0}. El motivo es obligatorio.',N'The unit will move to state {0}. A reason is required.'),
 ('PN2_DEST_EFFECT_OPTIONAL',N'La unidad quedará: {0}. El motivo es opcional.',N'Unit status: {0}. A reason is optional.',N'La unidad quedará en estado {0}. El motivo es opcional.',N'The unit will move to state {0}. A reason is optional.'),
 ('PN2_DESTINATION_ALREADY_SET',N'Esta entrega ya tiene un destino registrado.',N'This delivery already has a recorded outcome.',N'Esta entrega ya tiene un destino registrado.',N'This delivery already has a recorded state.');
 INSERT @textosAnteriores VALUES
 ('PN2_QUANTITY_NOTE',N'Se permite atención parcial. Las devoluciones y los descartes reabren el saldo; el historial conserva todas las entregas.',N'Partial fulfillment is allowed. Returns and discards reopen the balance; all deliveries remain in history.',N'Se permite atención parcial. Solo Transfundida cubre el pedido; cualquier otro estado de destino reabre el saldo y queda en el historial.',N'Partial fulfillment is allowed. Only Transfused fulfills the request; any other delivery state reopens the balance and remains in history.'),
 ('PN2_DESTINATIONS_DESCRIPTION',N'Registrá la transfusión, devolución o descarte de una entrega. El listado conserva también los destinos ya registrados.',N'Record a delivery''s transfusion, return or discard. The list also preserves previously recorded destinations.',N'Elegí el estado de destino configurado en Maestros. Transfundida cubre el pedido; los demás estados quedan en el historial y reabren el saldo.',N'Choose a delivery state configured in Master Data. Transfused fulfills the request; other states remain in history and reopen the balance.'),
 ('PN2_DESTINATION_NOTE',N'Devolución y descarte requieren motivo y reabren el saldo. La devolución requiere reingreso autorizado antes de reutilizar la unidad.',N'Returns and discards require a reason and reopen the balance. Returned units require authorized reentry before reuse.',N'Devolución y descarte requieren motivo. El motivo de otros estados se configura en Maestros. Solo DEVUELTA habilita el reingreso autorizado de la unidad.',N'Returns and discards require a reason. The reason rule for other states is configured in Master Data. Only DEVUELTA enables authorized unit reentry.');
 UPDATE d SET texto=CASE WHEN i.codigo='en-US' THEN t.enNuevo ELSE t.esNuevo END
 FROM dbo.Traduccion d JOIN dbo.Etiqueta e ON e.id_etiqueta=d.id_etiqueta JOIN dbo.Idioma i ON i.id_idioma=d.id_idioma
 JOIN @textosAnteriores t ON t.clave=e.clave
 WHERE (i.codigo='en-US' AND d.texto=t.enAnterior) OR (i.codigo<>'en-US' AND d.texto=t.esAnterior);
 COMMIT;
END TRY
BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH;
GO

CREATE OR ALTER PROCEDURE dbo.sp_MaestroPN1_Exigir AS
BEGIN
 SET NOCOUNT ON;
 IF @@TRANCOUNT=0 THROW 51002,'INTEGRITY_TRANSACTION_REQUIRED',1;
 DECLARE @n BIGINT,@hash VARCHAR(64);
 SELECT @n=COUNT_BIG(*) FROM dbo.ComponenteTransfusional WITH(TABLOCK,HOLDLOCK);
 SELECT @n=COUNT_BIG(*) FROM dbo.GrupoSanguineo WITH(TABLOCK,HOLDLOCK);
 SELECT @n=COUNT_BIG(*) FROM dbo.FactorRh WITH(TABLOCK,HOLDLOCK);
 SELECT @n=COUNT_BIG(*) FROM dbo.EstadoUnidad WITH(TABLOCK,HOLDLOCK);
 SELECT @n=COUNT_BIG(*) FROM dbo.DestinoEntrega WITH(TABLOCK,HOLDLOCK);
 SELECT @n=COUNT_BIG(*) FROM dbo.Auditoria WITH(TABLOCK,HOLDLOCK) WHERE entidad IN ('MaestroComponente','MaestroGrupo','MaestroRh','MaestroDestino');
 SELECT @hash=CONVERT(VARCHAR(64),HASHBYTES('SHA2_256',CONVERT(NVARCHAR(MAX),
   (SELECT id,codigo,descripcion,activo FROM dbo.ComponenteTransfusional ORDER BY id FOR JSON PATH,INCLUDE_NULL_VALUES) + N'|'+
   (SELECT id,codigo,descripcion,activo FROM dbo.GrupoSanguineo ORDER BY id FOR JSON PATH,INCLUDE_NULL_VALUES) + N'|'+
   (SELECT id,codigo,descripcion,activo FROM dbo.FactorRh ORDER BY id FOR JSON PATH,INCLUDE_NULL_VALUES) + N'|'+
   COALESCE((SELECT codigo,descripcion FROM dbo.EstadoUnidad ORDER BY codigo FOR JSON PATH,INCLUDE_NULL_VALUES),N'[]') + N'|'+
   COALESCE((SELECT id,codigo,descripcion,activo,motivo_obligatorio FROM dbo.DestinoEntrega ORDER BY id FOR JSON PATH,INCLUDE_NULL_VALUES),N'[]') + N'|'+
   COALESCE((SELECT id_auditoria,entidad,id_entidad,accion,id_usuario_actor,identificador_usuario_actor,fecha_evento,estado_anterior_json,estado_nuevo_json,cambios_json FROM dbo.Auditoria WHERE entidad IN ('MaestroComponente','MaestroGrupo','MaestroRh','MaestroDestino') ORDER BY id_auditoria FOR JSON PATH,INCLUDE_NULL_VALUES),N'[]'))),2);
 IF NOT EXISTS(SELECT 1 FROM dbo.MaestrosPN1Control WITH(HOLDLOCK) WHERE id=1 AND huella=@hash)
  THROW 51014,'MAESTROS_INTEGRIDAD_INVALIDA',1;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_MaestroPN1_Listar @tipo VARCHAR(20), @solo_activos BIT=1 AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 BEGIN TRY
  BEGIN TRANSACTION;
  DECLARE @lock INT;
  EXEC @lock=sys.sp_getapplock @Resource='HemovidaGest.PN1.Integridad',@LockMode='Shared',@LockOwner='Transaction',@LockTimeout=10000;
  IF @lock<0 THROW 51002,'INTEGRITY_LOCK_TIMEOUT',1;
  EXEC dbo.sp_MaestroPN1_Exigir;
  IF @tipo='COMPONENTE' SELECT id,codigo,descripcion,activo FROM dbo.ComponenteTransfusional WHERE @solo_activos=0 OR activo=1 ORDER BY codigo;
  ELSE IF @tipo='GRUPO' SELECT id,codigo,descripcion,activo FROM dbo.GrupoSanguineo WHERE @solo_activos=0 OR activo=1 ORDER BY codigo;
  ELSE IF @tipo='RH' SELECT id,codigo,descripcion,activo FROM dbo.FactorRh WHERE @solo_activos=0 OR activo=1 ORDER BY codigo;
  ELSE IF @tipo='DESTINO' SELECT id,codigo,descripcion,activo,motivo_obligatorio FROM dbo.DestinoEntrega WHERE @solo_activos=0 OR activo=1 ORDER BY codigo;
  ELSE THROW 51013,'MAESTRO_INVALIDO',1;
  COMMIT;
 END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_MaestroPN1_Guardar
 @tipo VARCHAR(20),@id INT,@codigo VARCHAR(100),@descripcion NVARCHAR(150),@activo BIT,@id_usuario INT,
 @motivo_obligatorio BIT=NULL
AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 BEGIN TRY
  BEGIN TRANSACTION;
  EXEC dbo.sp_IntegridadPN1_Autorizar @id_usuario,'MAESTROS_PN1_ADMINISTRAR';
  EXEC dbo.sp_IntegridadPN1_Exigir;
  EXEC dbo.sp_MaestroPN1_Exigir;
  SET @codigo=LTRIM(RTRIM(@codigo)); SET @descripcion=LTRIM(RTRIM(@descripcion));
  IF @tipo NOT IN ('COMPONENTE','GRUPO','RH','DESTINO') OR NULLIF(@codigo,'') IS NULL OR NULLIF(@descripcion,N'') IS NULL
     OR (@tipo IN ('GRUPO','RH') AND LEN(@codigo)>5) OR @activo IS NULL OR @id<0
     OR (@tipo='DESTINO' AND (LEN(@codigo)>20 OR @codigo='SIN_DESTINO' OR @motivo_obligatorio IS NULL OR (@codigo IN ('DEVUELTA','DESCARTADA') AND @motivo_obligatorio=0)))
  BEGIN ROLLBACK; SELECT 'DATOS_INVALIDOS' codigo_resultado; RETURN; END;
  DECLARE @anterior NVARCHAR(MAX),@nuevo NVARCHAR(MAX),@entidad NVARCHAR(100),@codigo_original VARCHAR(100);
  IF @tipo='COMPONENTE' SELECT @codigo_original=codigo,@anterior=(SELECT id,codigo,descripcion,activo FOR JSON PATH,WITHOUT_ARRAY_WRAPPER) FROM dbo.ComponenteTransfusional WHERE id=@id;
  IF @tipo='GRUPO' SELECT @codigo_original=codigo,@anterior=(SELECT id,codigo,descripcion,activo FOR JSON PATH,WITHOUT_ARRAY_WRAPPER) FROM dbo.GrupoSanguineo WHERE id=@id;
  IF @tipo='RH' SELECT @codigo_original=codigo,@anterior=(SELECT id,codigo,descripcion,activo FOR JSON PATH,WITHOUT_ARRAY_WRAPPER) FROM dbo.FactorRh WHERE id=@id;
  IF @tipo='DESTINO' SELECT @codigo_original=codigo,@anterior=(SELECT id,codigo,descripcion,activo,motivo_obligatorio FOR JSON PATH,WITHOUT_ARRAY_WRAPPER) FROM dbo.DestinoEntrega WHERE id=@id;
  IF @id>0 AND (@codigo_original IS NULL OR @codigo_original<>@codigo)
  BEGIN ROLLBACK; SELECT 'NO_ENCONTRADO' codigo_resultado; RETURN; END;
  IF @tipo='DESTINO' AND @id=0 AND EXISTS(SELECT 1 FROM dbo.EstadoUnidad WHERE codigo=@codigo)
  BEGIN ROLLBACK; SELECT 'DUPLICADO' codigo_resultado; RETURN; END;
  IF @id=0
  BEGIN
   IF @tipo='COMPONENTE' INSERT dbo.ComponenteTransfusional(codigo,descripcion,activo) VALUES(@codigo,@descripcion,@activo);
   IF @tipo='GRUPO' INSERT dbo.GrupoSanguineo(codigo,descripcion,activo) VALUES(@codigo,@descripcion,@activo);
   IF @tipo='RH' INSERT dbo.FactorRh(codigo,descripcion,activo) VALUES(@codigo,@descripcion,@activo);
   IF @tipo='DESTINO'
   BEGIN
    INSERT dbo.EstadoUnidad(codigo,descripcion) VALUES(@codigo,@descripcion);
    INSERT dbo.DestinoEntrega(codigo,descripcion,activo,motivo_obligatorio) VALUES(@codigo,@descripcion,@activo,@motivo_obligatorio);
   END;
   SET @id=CONVERT(INT,SCOPE_IDENTITY());
  END
  ELSE
  BEGIN
   IF @tipo='COMPONENTE' UPDATE dbo.ComponenteTransfusional SET descripcion=@descripcion,activo=@activo WHERE id=@id;
   IF @tipo='GRUPO' UPDATE dbo.GrupoSanguineo SET descripcion=@descripcion,activo=@activo WHERE id=@id;
   IF @tipo='RH' UPDATE dbo.FactorRh SET descripcion=@descripcion,activo=@activo WHERE id=@id;
   IF @tipo='DESTINO'
   BEGIN
    UPDATE dbo.EstadoUnidad SET descripcion=@descripcion WHERE codigo=@codigo;
    UPDATE dbo.DestinoEntrega SET descripcion=@descripcion,activo=@activo,motivo_obligatorio=@motivo_obligatorio WHERE id=@id;
   END;
  END;
  SET @entidad=CASE @tipo WHEN 'COMPONENTE' THEN 'MaestroComponente' WHEN 'GRUPO' THEN 'MaestroGrupo' WHEN 'RH' THEN 'MaestroRh' ELSE 'MaestroDestino' END;
  IF @tipo='DESTINO' SET @nuevo=(SELECT @id id,@codigo codigo,@descripcion descripcion,@activo activo,@motivo_obligatorio motivo_obligatorio FOR JSON PATH,WITHOUT_ARRAY_WRAPPER);
  ELSE SET @nuevo=(SELECT @id id,@codigo codigo,@descripcion descripcion,@activo activo FOR JSON PATH,WITHOUT_ARRAY_WRAPPER);
  INSERT dbo.Auditoria(entidad,id_entidad,accion,id_usuario_actor,identificador_usuario_actor,estado_anterior_json,estado_nuevo_json,cambios_json)
  VALUES(@entidad,@id,CASE WHEN @anterior IS NULL THEN 'CREATE' ELSE 'UPDATE' END,@id_usuario,
    (SELECT nombre_usuario FROM dbo.Usuario WHERE id_usuario=@id_usuario),@anterior,@nuevo,
    (SELECT 'Registro' Campo_380_jh,@anterior ValorAnterior_380_jh,@nuevo ValorNuevo_380_jh FOR JSON PATH,INCLUDE_NULL_VALUES));
  UPDATE dbo.MaestrosPN1Control SET huella=CONVERT(VARCHAR(64),HASHBYTES('SHA2_256',CONVERT(NVARCHAR(MAX),
   (SELECT id,codigo,descripcion,activo FROM dbo.ComponenteTransfusional ORDER BY id FOR JSON PATH,INCLUDE_NULL_VALUES) + N'|'+
   (SELECT id,codigo,descripcion,activo FROM dbo.GrupoSanguineo ORDER BY id FOR JSON PATH,INCLUDE_NULL_VALUES) + N'|'+
   (SELECT id,codigo,descripcion,activo FROM dbo.FactorRh ORDER BY id FOR JSON PATH,INCLUDE_NULL_VALUES) + N'|'+
   COALESCE((SELECT codigo,descripcion FROM dbo.EstadoUnidad ORDER BY codigo FOR JSON PATH,INCLUDE_NULL_VALUES),N'[]') + N'|'+
    COALESCE((SELECT id,codigo,descripcion,activo,motivo_obligatorio FROM dbo.DestinoEntrega ORDER BY id FOR JSON PATH,INCLUDE_NULL_VALUES),N'[]') + N'|'+
   COALESCE((SELECT id_auditoria,entidad,id_entidad,accion,id_usuario_actor,identificador_usuario_actor,fecha_evento,estado_anterior_json,estado_nuevo_json,cambios_json FROM dbo.Auditoria WHERE entidad IN ('MaestroComponente','MaestroGrupo','MaestroRh','MaestroDestino') ORDER BY id_auditoria FOR JSON PATH,INCLUDE_NULL_VALUES),N'[]'))),2) WHERE id=1;
  COMMIT; SELECT 'OK' codigo_resultado;
 END TRY
 BEGIN CATCH
  IF @@TRANCOUNT>0 ROLLBACK;
  IF ERROR_NUMBER() IN (2601,2627) BEGIN SELECT 'DUPLICADO' codigo_resultado; RETURN; END;
  THROW;
 END CATCH;
END;
GO
