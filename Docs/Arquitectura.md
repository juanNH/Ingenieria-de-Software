# Arquitectura del sistema

## Objetivo

Organizar HemovidaGest mediante componentes con responsabilidades definidas para sostener el registro y la clasificación de unidades de PN1 y la solicitud, reserva, entrega y destino de PN2. La arquitectura busca facilitar el mantenimiento, la trazabilidad de las operaciones y la protección de los datos.

## Descripción detallada de la arquitectura

HemovidaGest es una aplicación de escritorio en C# para .NET Framework 4.8, con interfaz Windows Forms. La solución contiene siete proyectos: UI, Application, Repository, DAL, Domain, Services y Abstractions.

UI reúne formularios y vistas. Application coordina los casos de uso y las validaciones, autorizaciones y verificaciones de integridad correspondientes. Repository ofrece operaciones de acceso a datos y delega en los DataMappers de DAL. Estos utilizan DatabaseContext_380_jh para ejecutar procedimientos almacenados y SQL parametrizado mediante ADO.NET.

Domain reúne las entidades y estados compartidos, incluidos usuarios, permisos y datos de PN1 y PN2. Abstractions define contratos comunes. Services implementa capacidades de cálculo de hashes de contraseñas, cifrado de correo, idiomas, XML y PDF. Las dependencias entre proyectos incluyen el acceso directo de los servicios de idiomas a Repository.

La persistencia relacional reside en SQL Server, en la base HemovidaGest. Las transacciones implementadas en SQL o ADO.NET confirman o revierten sus cambios como una unidad. Las claves, restricciones, movimientos, auditoría y dígitos verificadores aportan controles y trazabilidad según el recorrido de cada operación.

Los XML de intercambio se validan y pueden presentarse en una vista temporal. Los comprobantes PDF se guardan como archivos; SQL conserva sus instantáneas y metadatos para permitir su generación y reintento.

## Diagrama de componentes

El diagrama de componentes presenta los siete proyectos de la solución, sus 14 dependencias directas y el acceso de DAL a SQL Server.

[Fuente editable del diagrama de componentes](../Diagramas/Arquitectura/Diagrama%20de%20componentes.drawio).

[Editable publicado en Drive](https://drive.google.com/file/d/1mmhaw5ysaS4_KSSk1z_h-kGB5gkWkwnk/view).

## Mapa de navegación

El mapa de navegación relaciona las pantallas de autenticación y el formulario principal con las vistas de PN1, PN2 y administración disponibles según los permisos del usuario.

[Fuente editable del mapa de navegación](../Diagramas/Arquitectura/Mapa%20de%20navegaci%C3%B3n.drawio).

[Editable publicado en Drive](https://drive.google.com/file/d/18TrQLSn9kP8ACrTR0zQ_XEG4oI9ghJbc/view).

El mapa representa las rutas de los menús disponibles y los diálogos que abren. Al ingresar, el formulario principal elige automáticamente la primera vista autorizada. El menú Consultas contiene únicamente Abastecimiento y riesgo de vencimiento. Su vista presenta filtros, indicadores, una grilla de detalle y la información de la fila seleccionada. El permiso `REPORTE_VER` controla la visibilidad y el acceso a esta consulta. La correspondencia se puede revisar en [MainForm](../UI/Forms/Main/MainForm.cs), métodos `ConfigurarMenuNegocio_380_jh`, `ConfigurarMenuPorPermisos_380_jh` y `MostrarPantallaInicial_380_jh`, y en [AbastecimientoView](../UI/Views/PN2/AbastecimientoView.cs).

## Esquema de persistencia

El acceso a SQL Server, los grupos de datos persistidos y la relación con los archivos de intercambio y comprobantes se describen en [Persistencia.md](Persistencia.md).

## Modelos finales

El [modelo de clases integrado G06](Modelo-clases-final.md) reúne los tipos de las siete capas, sus contratos y relaciones, con trazabilidad al código y balance con los diagramas parciales. El [modelo de datos físico G07](Modelo-datos-final.md) presenta las tablas, columnas y claves foráneas reales de SQL Server mediante diagramas nativos de SSMS.

## Referencias de implementación

- [Solución](../IngDeSoftware.sln) y [proyecto UI](../UI/UI.csproj): proyectos, referencias, Windows Forms y .NET Framework 4.8.
- [Inicio de la aplicación](../UI/Program.cs): autenticación y formulario principal.
- [Servicio de PN2](../Application/PN2/PN2ApplicationService.cs): coordinación de operaciones, repositorio, bitácora e integridad.
- [Repositorio de PN2](../Repository/PN2/PN2Repository.cs) y [DataMapper de PN2](../DAL/PN2/PN2DataMapper.cs): recorrido del acceso a datos.
- [DatabaseContext](../DAL/Infrastructure/DatabaseContext.cs): conexión ADO.NET, comandos parametrizados y transacciones.
- [Usuario](../Domain/Usuarios/Usuario.cs) y [contrato XML](../Abstractions/Serialization/IXmlSerializer.cs): modelos y contratos compartidos.
- [LanguageManager](../Services/Idiomas/LanguageManager.cs): dependencia directa de Services hacia Repository.
- [Hash de contraseñas](../Services/Seguridad/PlainTextPasswordService.cs) y [cifrado de correo](../Services/Seguridad/CifradoEmailService.cs): servicios de seguridad actuales.
- [Intercambio XML](../Application/PN1/TablaUnidadesXmlService.cs) y [comprobantes automáticos](../Application/PN1/ComprobanteAutomaticoApplicationService.cs): tratamiento de archivos y colaboración con la persistencia.
- [Operaciones de PN1](../Database/StoredProcedures/07_PN1.sql) y [comprobantes SQL](../Database/StoredProcedures/11_ComprobantesAutomaticos.sql): transacciones, instantáneas y metadatos.
