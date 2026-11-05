# Diagramas entidad-relación por caso de uso

Los archivos editables `.drawio` corresponden a CDUN-01 a CDUN-11. Se construyeron a partir de los metadatos de la base `HemovidaGest` en SQL Server y de los procedimientos utilizados por la aplicación. Cada diagrama muestra las tablas de negocio que participan en ese caso; no es un esquema completo de la base.

| Caso | Procedimientos y funcionamiento relevantes |
| --- | --- |
| CDUN-01 | `sp_Donante_Registrar` |
| CDUN-02 | `sp_Donacion_Registrar` |
| CDUN-03 | `sp_Unidad_Clasificar` |
| CDUN-04 | `sp_Unidad_CambiarEstado` |
| CDUN-05 | Movimiento de unidad, disparador `tr_MovimientoUnidad_Comprobante` y procedimientos de comprobantes |
| CDUN-06 | `sp_SolicitudTransfusional_Registrar` |
| CDUN-07 | `sp_PN2_ValidarEstudio` |
| CDUN-08 | `sp_PN2_UnidadesCandidatas` y `sp_Reserva_Registrar` |
| CDUN-09 | `sp_Entrega_Registrar` |
| CDUN-10 | `sp_Entrega_RegistrarDestino` |
| CDUN-11 | Movimiento de unidad, disparador `tr_MovimientoUnidad_Comprobante` y procedimientos de comprobantes |

Las líneas continuas representan claves foráneas declaradas en SQL Server. Las discontinuas representan validaciones por procedimiento o referencias lógicas sin clave foránea. El texto junto a una FK indica la cantidad de registros de la tabla de origen por cada destino y, a continuación, la cantidad de destinos por cada origen. `FK*` señala una clave foránea a una tabla que quedó fuera del alcance del caso; las columnas técnicas `dvh` se omitieron.

Los datos del receptor y de su estudio están en `SolicitudTransfusional`: en el esquema consultado no existe una tabla independiente `Receptor`. Los campos de componente, grupo y factor del receptor se validan mediante procedimientos, pero no tienen FK declarada. En los comprobantes, `proceso` e `id_referencia` identifican la donación o la entrega sin una FK declarada; el disparador captura los datos en XML.
