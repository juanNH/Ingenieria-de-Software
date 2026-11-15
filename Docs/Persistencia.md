# Esquema de persistencia

La persistencia principal utiliza SQL Server y la base `HemovidaGest`, con las tablas de aplicación en el esquema `dbo`. El [diagrama editable](../Diagramas/Arquitectura/Esquema%20de%20persistencia.drawio) muestra el recorrido del acceso a datos y los grupos de información almacenados.

Los servicios de aplicación consumen repositorios. Estos delegan en los DataMappers de DAL, que transforman resultados tabulares en objetos del dominio. `DatabaseContext_380_jh` administra `SqlConnection`, crea comandos ADO.NET con parámetros y permite iniciar, confirmar o revertir una transacción. El acceso combina procedimientos almacenados y comandos SQL de texto parametrizados.

La configuración de conexión se resuelve desde `App.local.config`. No se incluyen credenciales ni valores de una instalación particular en el diagrama. El esquema y la configuración de ejemplo utilizan el catálogo `HemovidaGest`.

| Grupo | Información persistida |
| --- | --- |
| PN1 | Donantes, donaciones, unidades, clasificación y movimientos. |
| PN2 | Solicitudes con datos y estudios del receptor, reservas, entregas y destinos de unidades. |
| Maestros compartidos | Grupos sanguíneos, factores Rh, componentes transfusionales, estados, destinos y compatibilidades. |
| Usuarios y permisos | Cuentas, componentes de permiso y asignaciones. |
| Idiomas y traducciones | Idiomas, claves y textos traducidos. |
| Bitácora y auditoría | Eventos de bitácora e instantáneas de cambios de usuarios, operaciones de PN1/PN2, maestros y compatibilidades. |
| Integridad | Dígitos verificadores horizontales y verticales. |
| Comprobantes | Instantáneas XML, estado de generación y ruta del PDF en `ComprobantePendiente`. |

Los archivos de intercambio XML de unidades se cargan en una vista temporal; no insertan filas en SQL. Los comprobantes PDF se generan como archivos después de confirmar la operación. SQL conserva la instantánea y la ruta para permitir reintentos. Véanse [serialización XML](PN1-serializacion-xml.md) y [comprobantes PN1 y PN2](Comprobantes-PN1-PN2.md).

El detalle de tablas, columnas y relaciones corresponde a los DER nativos de SSMS. El instalador incorpora los diagramas por caso mediante `12_DiagramasSSMS.sql` y `14_DiagramasCDUSSMS.sql`, y las cuatro vistas del [modelo final G07](Modelo-datos-final.md) mediante `16_DiagramasFinalesSSMS.sql`. En total quedan 21 DER nativos editables, según el orden de [Database/README.md](../Database/README.md).

## Referencias de implementación

- [DatabaseContext](../DAL/Infrastructure/DatabaseContext.cs): conexión, comandos, parámetros y transacciones.
- [DonacionRepository](../Repository/PN1/DonacionRepository.cs) y [PN2Repository](../Repository/PN2/PN2Repository.cs): delegación a DAL.
- [DonacionDataMapper](../DAL/PN1/DonacionDataMapper.cs) y [PN2DataMapper](../DAL/PN2/PN2DataMapper.cs): acceso de PN1 y PN2.
- [Esquema base](../Database/00_tecni_salud_schema.sql) y [procedimientos y migraciones](../Database/StoredProcedures/): estructura relacional.
- [Comprobantes automáticos](../Database/StoredProcedures/11_ComprobantesAutomaticos.sql): instantáneas y registro de generación.
