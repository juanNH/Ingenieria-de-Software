-- Después de 06_MaestrosPN1.sql y antes de 09_PN2.sql.
-- Solo la primera instalación convierte la política anterior en filas.
USE [HemovidaGest];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO
BEGIN TRY
 BEGIN TRANSACTION;
 EXEC dbo.sp_IntegridadPN1_Exigir;
 EXEC dbo.sp_MaestroPN1_Exigir;
 DECLARE @nueva BIT=0,@huella VARCHAR(64);
 IF OBJECT_ID('dbo.CompatibilidadTransfusional','U') IS NULL
 BEGIN
  IF OBJECT_ID('dbo.CompatibilidadTransfusionalControl','U') IS NOT NULL
   THROW 51015,'COMPATIBILIDAD_INTEGRIDAD_INVALIDA',1;
  CREATE TABLE dbo.CompatibilidadTransfusional
  (
   id_compatibilidad INT IDENTITY PRIMARY KEY,
   tipo_componente VARCHAR(100) NOT NULL,
   grupo_unidad VARCHAR(5) NOT NULL,
   rh_unidad VARCHAR(5) NOT NULL,
   grupo_receptor VARCHAR(5) NOT NULL,
   rh_receptor VARCHAR(5) NOT NULL,
   activo BIT NOT NULL CONSTRAINT DF_Compatibilidad_Activo DEFAULT 1,
   CONSTRAINT UQ_Compatibilidad_Combinacion UNIQUE(tipo_componente,grupo_unidad,rh_unidad,grupo_receptor,rh_receptor),
   CONSTRAINT FK_Compatibilidad_Componente FOREIGN KEY(tipo_componente) REFERENCES dbo.ComponenteTransfusional(codigo),
   CONSTRAINT FK_Compatibilidad_GrupoUnidad FOREIGN KEY(grupo_unidad) REFERENCES dbo.GrupoSanguineo(codigo),
   CONSTRAINT FK_Compatibilidad_RhUnidad FOREIGN KEY(rh_unidad) REFERENCES dbo.FactorRh(codigo),
   CONSTRAINT FK_Compatibilidad_GrupoReceptor FOREIGN KEY(grupo_receptor) REFERENCES dbo.GrupoSanguineo(codigo),
   CONSTRAINT FK_Compatibilidad_RhReceptor FOREIGN KEY(rh_receptor) REFERENCES dbo.FactorRh(codigo)
  );
  CREATE TABLE dbo.CompatibilidadTransfusionalControl(id INT NOT NULL PRIMARY KEY CHECK(id=1),huella VARCHAR(64) NOT NULL);
  SET @nueva=1;
  -- Estas condiciones son exclusivamente la migración de las reglas existentes.
  -- No se aplican al alta de nuevos maestros ni al evaluar candidatos.
  INSERT dbo.CompatibilidadTransfusional(tipo_componente,grupo_unidad,rh_unidad,grupo_receptor,rh_receptor)
  SELECT c.codigo,gu.codigo,ru.codigo,gr.codigo,rr.codigo
  FROM dbo.ComponenteTransfusional c
  CROSS JOIN dbo.GrupoSanguineo gu CROSS JOIN dbo.FactorRh ru
  CROSS JOIN dbo.GrupoSanguineo gr CROSS JOIN dbo.FactorRh rr
  WHERE gu.codigo IN ('O','A','B','AB') AND gr.codigo IN ('O','A','B','AB')
   AND ru.codigo IN ('+','-') AND rr.codigo IN ('+','-')
   AND (
    (UPPER(LTRIM(RTRIM(c.codigo))) COLLATE Latin1_General_CI_AI IN
      ('GLOBULOS ROJOS','CONCENTRADO DE GLOBULOS ROJOS','ERITROCITOS','CONCENTRADO DE ERITROCITOS')
     AND (rr.codigo='+' OR ru.codigo='-')
     AND (gu.codigo='O' OR gu.codigo=gr.codigo OR (gr.codigo='AB' AND gu.codigo IN ('A','B'))))
    OR (UPPER(LTRIM(RTRIM(c.codigo))) COLLATE Latin1_General_CI_AI IN ('PLASMA','PLASMA FRESCO CONGELADO')
     AND (gu.codigo='AB' OR gu.codigo=gr.codigo OR (gr.codigo='O' AND gu.codigo IN ('A','B'))))
    OR (UPPER(LTRIM(RTRIM(c.codigo))) COLLATE Latin1_General_CI_AI IN ('PLAQUETAS','CONCENTRADO DE PLAQUETAS')
     AND gu.codigo=gr.codigo AND (rr.codigo='+' OR ru.codigo='-'))
   );
 END;
 IF OBJECT_ID('dbo.CompatibilidadTransfusionalControl','U') IS NULL
  THROW 51015,'COMPATIBILIDAD_INTEGRIDAD_INVALIDA',1;
 SELECT @huella=CONVERT(VARCHAR(64),HASHBYTES('SHA2_256',CONVERT(NVARCHAR(MAX),
  COALESCE((SELECT id_compatibilidad,tipo_componente,grupo_unidad,rh_unidad,grupo_receptor,rh_receptor,activo
   FROM dbo.CompatibilidadTransfusional ORDER BY id_compatibilidad FOR JSON PATH,INCLUDE_NULL_VALUES),N'[]')+N'|'+
  COALESCE((SELECT id_auditoria,entidad,id_entidad,accion,id_usuario_actor,identificador_usuario_actor,fecha_evento,estado_anterior_json,estado_nuevo_json,cambios_json
   FROM dbo.Auditoria WHERE entidad='CompatibilidadTransfusional' ORDER BY id_auditoria FOR JSON PATH,INCLUDE_NULL_VALUES),N'[]'))),2);
 IF @nueva=1 INSERT dbo.CompatibilidadTransfusionalControl(id,huella) VALUES(1,@huella);
 ELSE IF NOT EXISTS(SELECT 1 FROM dbo.CompatibilidadTransfusionalControl WHERE id=1 AND huella=@huella)
  THROW 51015,'COMPATIBILIDAD_INTEGRIDAD_INVALIDA',1;
 COMMIT;
END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH;
GO
CREATE OR ALTER VIEW dbo.v_CompatibilidadTransfusional_Huella AS
 SELECT CONVERT(VARCHAR(64),HASHBYTES('SHA2_256',CONVERT(NVARCHAR(MAX),
  COALESCE((SELECT id_compatibilidad,tipo_componente,grupo_unidad,rh_unidad,grupo_receptor,rh_receptor,activo
   FROM dbo.CompatibilidadTransfusional ORDER BY id_compatibilidad FOR JSON PATH,INCLUDE_NULL_VALUES),N'[]')+N'|'+
  COALESCE((SELECT id_auditoria,entidad,id_entidad,accion,id_usuario_actor,identificador_usuario_actor,fecha_evento,estado_anterior_json,estado_nuevo_json,cambios_json
   FROM dbo.Auditoria WHERE entidad='CompatibilidadTransfusional' ORDER BY id_auditoria FOR JSON PATH,INCLUDE_NULL_VALUES),N'[]'))),2) huella;
GO
CREATE OR ALTER PROCEDURE dbo.sp_CompatibilidadTransfusional_Exigir AS
BEGIN
 SET NOCOUNT ON;
 IF @@TRANCOUNT=0 THROW 51002,'INTEGRITY_TRANSACTION_REQUIRED',1;
 DECLARE @n BIGINT;
 SELECT @n=COUNT_BIG(*) FROM dbo.CompatibilidadTransfusional WITH(TABLOCK,HOLDLOCK);
 SELECT @n=COUNT_BIG(*) FROM dbo.Auditoria WITH(TABLOCK,HOLDLOCK) WHERE entidad='CompatibilidadTransfusional';
 IF NOT EXISTS(SELECT 1 FROM dbo.CompatibilidadTransfusionalControl WITH(HOLDLOCK) WHERE id=1 AND huella=(SELECT huella FROM dbo.v_CompatibilidadTransfusional_Huella))
  THROW 51015,'COMPATIBILIDAD_INTEGRIDAD_INVALIDA',1;
END;
GO
CREATE OR ALTER PROCEDURE dbo.sp_CompatibilidadTransfusional_Listar @id_usuario INT AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 BEGIN TRY
  BEGIN TRANSACTION;
  EXEC dbo.sp_IntegridadPN1_Autorizar @id_usuario,'MAESTROS_PN1_ADMINISTRAR';
  EXEC dbo.sp_IntegridadPN1_Exigir;
  EXEC dbo.sp_MaestroPN1_Exigir;
  EXEC dbo.sp_CompatibilidadTransfusional_Exigir;
  SELECT id_compatibilidad,tipo_componente,grupo_unidad,rh_unidad,grupo_receptor,rh_receptor,activo
  FROM dbo.CompatibilidadTransfusional ORDER BY tipo_componente,grupo_unidad,rh_unidad,grupo_receptor,rh_receptor;
  COMMIT;
 END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH;
END;
GO
CREATE OR ALTER PROCEDURE dbo.sp_CompatibilidadTransfusional_Guardar
 @id INT,@tipo_componente VARCHAR(100),@grupo_unidad VARCHAR(5),@rh_unidad VARCHAR(5),
 @grupo_receptor VARCHAR(5),@rh_receptor VARCHAR(5),@activo BIT,@id_usuario INT
AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 BEGIN TRY
  BEGIN TRANSACTION;
  EXEC dbo.sp_IntegridadPN1_Autorizar @id_usuario,'MAESTROS_PN1_ADMINISTRAR';
  EXEC dbo.sp_IntegridadPN1_Exigir;
  EXEC dbo.sp_MaestroPN1_Exigir;
  EXEC dbo.sp_CompatibilidadTransfusional_Exigir;
  IF @id IS NULL OR @id<0 OR @activo IS NULL
   OR NOT EXISTS(SELECT 1 FROM dbo.ComponenteTransfusional WHERE codigo=@tipo_componente AND (@activo=0 OR activo=1))
   OR NOT EXISTS(SELECT 1 FROM dbo.GrupoSanguineo WHERE codigo=@grupo_unidad AND (@activo=0 OR activo=1))
   OR NOT EXISTS(SELECT 1 FROM dbo.GrupoSanguineo WHERE codigo=@grupo_receptor AND (@activo=0 OR activo=1))
   OR NOT EXISTS(SELECT 1 FROM dbo.FactorRh WHERE codigo=@rh_unidad AND (@activo=0 OR activo=1))
   OR NOT EXISTS(SELECT 1 FROM dbo.FactorRh WHERE codigo=@rh_receptor AND (@activo=0 OR activo=1))
  BEGIN ROLLBACK; SELECT 'DATOS_INVALIDOS' codigo_resultado; RETURN; END;
  DECLARE @anterior NVARCHAR(MAX),@nuevo NVARCHAR(MAX);
  IF @id>0
  BEGIN
   SELECT @anterior=(SELECT id_compatibilidad,tipo_componente,grupo_unidad,rh_unidad,grupo_receptor,rh_receptor,activo FOR JSON PATH,WITHOUT_ARRAY_WRAPPER)
   FROM dbo.CompatibilidadTransfusional WHERE id_compatibilidad=@id AND tipo_componente=@tipo_componente
    AND grupo_unidad=@grupo_unidad AND rh_unidad=@rh_unidad AND grupo_receptor=@grupo_receptor AND rh_receptor=@rh_receptor;
   IF @anterior IS NULL BEGIN ROLLBACK; SELECT 'NO_ENCONTRADO' codigo_resultado; RETURN; END;
   UPDATE dbo.CompatibilidadTransfusional SET activo=@activo WHERE id_compatibilidad=@id;
  END
  ELSE
  BEGIN
   INSERT dbo.CompatibilidadTransfusional(tipo_componente,grupo_unidad,rh_unidad,grupo_receptor,rh_receptor,activo)
   VALUES(@tipo_componente,@grupo_unidad,@rh_unidad,@grupo_receptor,@rh_receptor,@activo);
   SET @id=CONVERT(INT,SCOPE_IDENTITY());
  END;
  SELECT @nuevo=(SELECT id_compatibilidad,tipo_componente,grupo_unidad,rh_unidad,grupo_receptor,rh_receptor,activo FOR JSON PATH,WITHOUT_ARRAY_WRAPPER)
   FROM dbo.CompatibilidadTransfusional WHERE id_compatibilidad=@id;
  INSERT dbo.Auditoria(entidad,id_entidad,accion,id_usuario_actor,identificador_usuario_actor,estado_anterior_json,estado_nuevo_json,cambios_json)
  VALUES('CompatibilidadTransfusional',@id,CASE WHEN @anterior IS NULL THEN 'CREATE' ELSE 'UPDATE' END,@id_usuario,
   (SELECT nombre_usuario FROM dbo.Usuario WHERE id_usuario=@id_usuario),@anterior,@nuevo,
   (SELECT 'Registro' Campo_380_jh,@anterior ValorAnterior_380_jh,@nuevo ValorNuevo_380_jh FOR JSON PATH,INCLUDE_NULL_VALUES));
  UPDATE dbo.CompatibilidadTransfusionalControl SET huella=(SELECT huella FROM dbo.v_CompatibilidadTransfusional_Huella) WHERE id=1;
  COMMIT; SELECT 'OK' codigo_resultado;
 END TRY
 BEGIN CATCH
  IF @@TRANCOUNT>0 ROLLBACK;
  IF ERROR_NUMBER() IN (2601,2627) BEGIN SELECT 'DUPLICADO' codigo_resultado; RETURN; END;
  THROW;
 END CATCH;
END;
GO

-- Traducciones: conservar las personalizaciones existentes.
DECLARE @textos TABLE(clave VARCHAR(150),es NVARCHAR(500),en NVARCHAR(500));
INSERT @textos VALUES
('COMPAT_TITLE',N'Compatibilidades',N'Compatibility'),
('COMPAT_GROUP_UNIT',N'Grupo de la unidad',N'Unit blood group'),
('COMPAT_RH_UNIT',N'Rh de la unidad',N'Unit Rh'),
('COMPAT_GROUP_RECIPIENT',N'Grupo del receptor',N'Recipient blood group'),
('COMPAT_RH_RECIPIENT',N'Rh del receptor',N'Recipient Rh'),
('COMPAT_HELP',N'Cada fila activa permite una combinación de unidad → receptor para el componente elegido. Sin regla activa no hay candidatas. Para cambiar una combinación, desactivala y creá otra. La confirmación del personal sigue siendo necesaria.',N'Each active row allows one unit → recipient combination for the selected component. Without an active rule there are no candidates. To change a combination, deactivate it and create another. Staff confirmation is still required.'),
('COMPAT_FILTER',N'Filtrar reglas',N'Filter rules'),
('COMPAT_LOAD_ERROR',N'No se pudieron cargar las reglas. Revisá la conexión, la integridad y la instalación de compatibilidades.',N'Rules could not be loaded. Check the connection, integrity and compatibility installation.'),
('COMPAT_RESULT_DATOS_INVALIDOS',N'Seleccioná componente, grupo y Rh de unidad y receptor. Para activar una regla, sus opciones deben estar activas en Maestros.',N'Select the component and unit/recipient blood group and Rh. To activate a rule, its options must be active in Master Data.'),
('COMPAT_RESULT_DUPLICADO',N'La combinación ya existe. Seleccioná su fila para activarla o desactivarla.',N'This combination already exists. Select its row to activate or deactivate it.');
INSERT dbo.Etiqueta(clave,descripcion)
SELECT clave,clave FROM @textos t WHERE NOT EXISTS(SELECT 1 FROM dbo.Etiqueta e WHERE e.clave=t.clave);
INSERT dbo.Traduccion(id_etiqueta,id_idioma,texto)
SELECT e.id_etiqueta,i.id_idioma,CASE WHEN i.codigo='en-US' THEN t.en ELSE t.es END
FROM @textos t JOIN dbo.Etiqueta e ON e.clave=t.clave JOIN dbo.Idioma i ON i.codigo IN ('es-AR','en-US')
WHERE NOT EXISTS(SELECT 1 FROM dbo.Traduccion d WHERE d.id_etiqueta=e.id_etiqueta AND d.id_idioma=i.id_idioma);
UPDATE d SET texto=CASE WHEN i.codigo='en-US' THEN N'Configure allowed combinations in the Compatibility tab. Without an active rule no candidate units are produced in PN2.'
 ELSE N'Configurá las combinaciones permitidas en la pestaña Compatibilidades. Sin una regla activa no se generan candidatas en PN2.' END
FROM dbo.Traduccion d JOIN dbo.Etiqueta e ON e.id_etiqueta=d.id_etiqueta JOIN dbo.Idioma i ON i.id_idioma=d.id_idioma
WHERE e.clave='MASTERS_COMPATIBILITY_NOTE' AND
 ((i.codigo='en-US' AND d.texto=N'Options without a known compatibility rule do not produce candidate units in PN2.')
 OR (i.codigo='es-AR' AND d.texto=N'Las opciones sin una regla de compatibilidad conocida no generan unidades candidatas en PN2.'));
GO
