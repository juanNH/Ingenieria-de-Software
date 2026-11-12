# Diagramas entidad-relación por caso de uso

Los diagramas corresponden a CDUN-01 a CDUN-08 y se encuentran en la carpeta **Database Diagrams** de la base `HemovidaGest` de SQL Server. Cada uno muestra las tablas seleccionadas para ese caso; no es un esquema completo de la base.

Los PNG con sufijo `- SSMS.png` son copias obtenidas del diseñador nativo de SQL Server Management Studio. Los diagramas se restauran y sus nombres se actualizan mediante `Database/StoredProcedures/12_DiagramasSSMS.sql`; sus líneas corresponden a claves foráneas de SQL Server.

| Caso | Procedimientos y funcionamiento relevantes |
| --- | --- |
| CDUN-01 | `sp_Donante_Registrar` y vínculo con el usuario que da de alta al donante |
| CDUN-02 | `sp_Donacion_Registrar` |
| CDUN-03 | `sp_Unidad_Clasificar` y `sp_Unidad_CambiarEstado` |
| CDUN-04 | Movimiento de unidad, disparador `tr_MovimientoUnidad_Comprobante` y procedimientos de comprobantes |
| CDUN-05 | `sp_SolicitudTransfusional_Registrar` y `sp_PN2_ActualizarEstudio`. Se consulta `Reserva` antes de completar o corregir el estudio. |
| CDUN-06 · Estudio | `sp_PN2_ValidarEstudio`. Vista nativa `CDUN-06 - Estudio`. |
| CDUN-06 · Reserva | `sp_PN2_UnidadesCandidatas`, `sp_Reserva_Registrar` y `sp_Reserva_Cancelar`. Vista nativa `CDUN-06 - Reserva`. |
| CDUN-06 · Entrega | `sp_Reserva_Listar` y `sp_Entrega_Registrar`. Vista nativa `CDUN-06 - Entrega`. |
| CDUN-07 | `sp_Entrega_Listar` y `sp_Entrega_RegistrarDestino`. El destino activo se toma de `DestinoEntrega` y el movimiento dispara el comprobante de CDUN-08. |
| CDUN-08 | Disparador `tr_MovimientoUnidad_Comprobante`, consulta `sp_Comprobante_Pendientes` y confirmación `sp_Comprobante_MarcarGenerado`. |

El CDUN-06 —Validar estudio, reservar y entregar unidad— tiene tres vistas DER complementarias del mismo caso: **Estudio**, **Reserva** y **Entrega**. Se conservan las disposiciones nativas y las tablas de cada etapa. Los PNG identifican esas vistas en el nombre del archivo; sus imágenes se mantienen sin alteraciones.

Las líneas representan claves foráneas declaradas en SQL Server; las referencias lógicas sin clave foránea no aparecen como líneas. Las imágenes conservan las columnas técnicas mostradas por SSMS, incluida `dvh`.

Los datos del receptor y de su estudio están en `SolicitudTransfusional`. En CDUN-05 y CDUN-06, SSMS dibuja las relaciones de `tipo_componente`, `grupo_receptor` y `rh_receptor` con los códigos de sus catálogos, y las dos relaciones con `Usuario` (solicitante y validador). El grupo, el factor Rh y el usuario validador pueden quedar vacíos cuando aún no se cuenta con esa información. Las claves se instalan con `Database/StoredProcedures/09a_RelacionesSolicitudPN2.sql`.

En CDUN-04 y CDUN-08, `ComprobantePendiente` se conecta con el resto de las tablas mediante la FK a `Usuario`. Sus campos `proceso` e `id_referencia` identifican la donación o la entrega sin una clave foránea directa; el disparador captura los datos del comprobante en XML.
