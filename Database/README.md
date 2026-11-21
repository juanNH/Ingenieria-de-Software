# Orden de ejecución de los scripts SQL

Usá este archivo como índice antes de ejecutar algo de `Database`. El orden
depende de si la base es nueva o si ya tiene PN1/PN2.

Los prefijos indican el orden para crear una base nueva desde cero. Para una
base existente, usá el instalador de la sección correspondiente; cada uno aplica
la secuencia de migración adecuada.

## Base nueva con PN1 y PN2

El instalador ejecuta estos **21 scripts**, en este orden, al crear
`HemovidaGest`. Para una ejecución manual, usá el mismo orden y una cuenta con
permisos para crear y modificar la base:

1. `00_tecni_salud_schema.sql` — crea la base, tablas y datos iniciales.
2. `StoredProcedures/01_AuthUsuario.sql` — procedimientos de usuarios.
3. `StoredProcedures/02_Bitacora.sql` — bitácora.
4. `StoredProcedures/03_Auditoria.sql` — auditoría.
5. `StoredProcedures/04_Permisos.sql` — permisos y traducciones de PN1.
6. `StoredProcedures/05_IntegridadPN1.sql` — controles e historial protegido de PN1.
7. `StoredProcedures/06_MaestrosPN1.sql` — catálogos administrables y sus controles.
8. `StoredProcedures/06a_CompatibilidadTransfusional.sql` — matriz configurable, reglas iniciales, auditoría e integridad.
9. `StoredProcedures/06b_TipificacionDonante.sql` — tipificación persistida del donante y recuperación de antecedentes válidos.
10. `StoredProcedures/07_PN1.sql` — procedimientos de donantes, donaciones y unidades.
11. `StoredProcedures/08_PN2_Catalogos.sql` — permisos y traducciones de PN2.
12. `StoredProcedures/09_PN2.sql` — solicitudes, reservas, entregas y trazabilidad.
13. `StoredProcedures/09a_RelacionesSolicitudPN2.sql` — aplica las claves foráneas de la solicitud a los catálogos y al usuario que valida el estudio.
14. `StoredProcedures/10_IntegridadPN2.sql` — extiende la integridad para PN2.
15. `StoredProcedures/11_ComprobantesAutomaticos.sql` — comprobantes automáticos.
16. `StoredProcedures/12_DiagramasSSMS.sql` — crea los diagramas CDUN-01 a CDUN-08 en Database Diagrams de SSMS, con sus diseños guardados; CDUN-06 conserva tres vistas complementarias del mismo caso.
17. `StoredProcedures/13_AjusteCantidadDonacion.sql` — corrección de cantidades e historial de donaciones.
18. `StoredProcedures/14_DiagramasCDUSSMS.sql` — instala los siete DER nativos de CDU-01 a CDU-07, exportados de SSMS. Si ya existen, solo actualiza una definición cuando su versión o contenido difieren; conserva los diagramas de negocio.
19. `StoredProcedures/15_AbastecimientoRiesgo.sql` — procedimiento del reporte de abastecimiento y riesgo de vencimiento, con autorización e integridad de PN2.
20. `StoredProcedures/15a_AbastecimientoUI.sql` — textos del reporte en español e inglés, conservando las traducciones existentes.
21. `StoredProcedures/16_DiagramasFinalesSSMS.sql` — instala el modelo general G07 y sus tres vistas de detalle, guardados en SSMS con todas las tablas en Standard; valida el esquema y actualiza solo las definiciones G07 que difieran.

El script 14 también crea los objetos de soporte nativos que falten, incluida `fn_diagramobjects`, para abrir los diagramas directamente en SSMS sin completar su instalación a mano.

El [instalador de la aplicación](../Installer/InstalarBase.ps1) y su paquete incluyen esta secuencia. La base nueva queda con **21 DER nativos** en **Database Diagrams**: diez vistas de los casos de negocio, siete de los casos de uso del sistema y cuatro G07 del modelo final. Las definiciones son editables en SSMS; la instalación no exporta imágenes PNG. El [modelo final](../Docs/Modelo-datos-final.md) cubre 32 tablas, 221 columnas y 53 FK; el [reporte de abastecimiento](../Docs/ReporteInteligente.md) describe el alcance de los scripts 15 y 15a. Si se eligen datos de demostración, el instalador agrega al final el script opcional `Installer/DatosPrueba.sql`, fuera de los 21 scripts base.

Para instalar solo PN1, detenete después del paso 10. Los pasos 11 a 15 son la
extensión PN2. Los pasos 16 a 21 completan los diagramas, el ajuste de cantidades
y el reporte de la entrega final. `09_PN2.sql` requiere PN1 y `06a_CompatibilidadTransfusional.sql` instalados; `10_IntegridadPN2.sql` requiere
PN2; y los comprobantes requieren ambas integridades.

## Base existente

Preferí los instaladores PowerShell desde la raíz del proyecto. Preparan un
respaldo verificado, aplican los scripts en una transacción y revisan la
integridad al finalizar:

| Situación | Comando |
| --- | --- |
| Incorporar o actualizar Maestros en PN1 o PN1+PN2 | `./Database/InstalarMaestrosPN1.ps1` |
| Incorporar PN2 a una base PN1 válida | `./Database/InstalarPN2.ps1` |
| Configurar compatibilidades en una base con PN2 | `./Database/InstalarCompatibilidad.ps1` |
| Actualizar únicamente los comprobantes de una base PN2 | `./Database/InstalarComprobantes.ps1` |
| Incorporar el reporte de abastecimiento, sus traducciones y los cuatro G07 a una base PN2 actualizada con comprobantes | `./Database/InstalarEntregaFinal.ps1 -Servidor . -Base HemovidaGest` |
| Incorporar grupo sanguíneo y Rh persistidos en el donante (PN1 o PN1+PN2) | `./Database/InstalarMaestrosPN1.ps1` |
| Incorporar la corrección de cantidades en una base con PN2 y comprobantes | `./Database/InstalarMaestrosPN1.ps1` |
| Actualizar PN1 para descartar unidades vencidas sin clasificación, con motivo | `./Database/InstalarMaestrosPN1.ps1` |

`InstalarPN2.ps1` aplica, en orden, `08_PN2_Catalogos.sql`, `06_MaestrosPN1.sql`,
`06a_CompatibilidadTransfusional.sql`, `06b_TipificacionDonante.sql`, `07_PN1.sql`, `09_PN2.sql`, `09a_RelacionesSolicitudPN2.sql`, `10_IntegridadPN2.sql`, `11_ComprobantesAutomaticos.sql`, `12_DiagramasSSMS.sql`, `13_AjusteCantidadDonacion.sql`, `14_DiagramasCDUSSMS.sql`, `15_AbastecimientoRiesgo.sql`, `15a_AbastecimientoUI.sql` y `16_DiagramasFinalesSSMS.sql`. El script 12 crea los diagramas que falten y actualiza la numeración a CDUN-01–08. Conserva la disposición del nombre canónico cuando ya existe y retira las entradas anteriores duplicadas, incluida la decisión operativa unificada en CDUN-03. El script 14 agrega los siete CDU del sistema sin eliminar los diagramas PN1/PN2, y el 16 agrega los cuatro G07 conservando los 17 anteriores. Requiere permisos de propietario de la base. Los diagramas dependen del esquema de tablas instalado.

[InstalarEntregaFinal.ps1](InstalarEntregaFinal.ps1) usa autenticación integrada de Windows y los parámetros explícitos de servidor y base, sin leer `App.local.config`. Exige PN2, comprobantes e integridad válida; crea un respaldo `COPY_ONLY` con checksum en la carpeta de backups de SQL Server y lo comprueba con `RESTORE VERIFYONLY`. Después aplica únicamente 15, 15a y 16 en una transacción, verifica nuevamente la integridad, el reporte y el modelo general y confirma los cambios. Una falla revierte la actualización. Cerrá la aplicación antes de ejecutarlo y reiniciala al terminar para cargar las traducciones. Si todavía faltan migraciones anteriores de PN2, usá primero `InstalarPN2.ps1`.

Para recuperar solo los diagramas después de reconstruir el esquema completo de
PN1/PN2 y comprobantes, ejecutá los scripts 12, 14 y 16 en ese orden desde SSMS,
conectado a la base destino. Si usás `sqlcmd`, estos comandos se ejecutan desde
la raíz del proyecto:

```powershell
sqlcmd -S . -E -d HemovidaGest -b -i Database/StoredProcedures/12_DiagramasSSMS.sql
sqlcmd -S . -E -d HemovidaGest -b -i Database/StoredProcedures/14_DiagramasCDUSSMS.sql
sqlcmd -S . -E -d HemovidaGest -b -i Database/StoredProcedures/16_DiagramasFinalesSSMS.sql
```
Si tu instancia tiene otro nombre, reemplazá `.` por el nombre del servidor.
Después actualizá la carpeta **Database Diagrams** en SSMS. Los scripts restauran los
diseños guardados en el repositorio; no recopilan automáticamente cambios que
hagas después en SSMS ni exportan imágenes.

`InstalarMaestrosPN1.ps1` actualiza maestros y procedimientos relacionados; si
PN2 ya está instalado, también actualiza PN2 y los comprobantes. Incluye las
compatibilidades configurables y conserva las reglas editadas o desactivadas.
Ver [cómo configurar las reglas](../Docs/Compatibilidad-configurable.md). No ejecutes a
mano esos mismos archivos además del instalador.

## Corrección de cantidad declarada

En **PN1 → Donaciones**, elegí una donación registrada y abrí **Cantidad e historial**. Al registrar una donación, la pantalla la selecciona automáticamente. `UNIDAD_VER` habilita la consulta y `DONACION_AJUSTAR_CANTIDAD` habilita los cambios. El responsable autorizado y el administrador reciben ambos permisos. El historial queda en solo lectura desde la primera liberación o al registrarse el comprobante de PN1.

La tabla de **Donaciones** muestra las unidades de la donación seleccionada. Al cambiar de pantalla y volver, Entrega 3 conserva la selección y consulta las unidades guardadas en la base. En **Bitácora**, el módulo **Banco de sangre** y la acción **Cantidad de donación ajustada** muestran las correcciones y restauraciones con el número de donación y las cantidades anterior y nueva.

Marcá las unidades existentes que deben quedar vigentes. Desmarcar una unidad en revisión sin clasificar la anula. Marcar una anulada por un ajuste la devuelve a revisión con el mismo código. **Unidades nuevas** permite sumar unidades en revisión a la misma donación abierta. El total resultante incluye las unidades fijas, las marcadas y las nuevas.

Para probar la actualización, registrá una donación de una unidad en Entrega 2 y dejala en revisión sin clasificar. En **Cantidad e historial** de Entrega 3, ingresá **2** en **Unidades nuevas**, completá el motivo y guardá. Volvé a abrir el historial para ver el cambio de **1 a 3**. Seleccioná **Registro inicial**, elegí **Restaurar cantidad seleccionada**, revisá las unidades marcadas, ingresá el motivo y guardá. La donación vuelve a una unidad vigente y el historial conserva tanto la corrección como la restauración.

`tr_Donacion_AjusteCantidad` valida la versión, el motivo, los identificadores modificados y la cantidad resultante. El ajuste, los movimientos y el historial protegido se confirman juntos.

`./Tests/Comprobantes.Automaticos.Tests.ps1` prueba el circuito en una base temporal, incluidos la reversión ante fallas y el cierre de PN1.

## Scripts auxiliares

Mientras la unidad está en revisión, la clasificación guarda valores provisionales
que se pueden corregir. La primera liberación confirma el grupo y Rh en el donante
y los copia a sus unidades pendientes dentro de la misma transacción, con auditoría
e integridad DVH/DVV. Las unidades que reciben valores nuevos requieren volver a
confirmar su clasificación. Las donaciones futuras guardan esos valores al registrarse.
Una discrepancia con la tipificación confirmada impide liberar la unidad.

`06b_TipificacionDonante.sql` recupera antecedentes completos y coincidentes de
unidades liberadas. Conserva las fichas con evidencia de liberación y vuelve a
dejar pendientes las fichas antiguas que solo tenían clasificaciones provisionales.
Los antecedentes de liberación contradictorios sin ficha requieren revisión.
Los instaladores de Maestros y PN2 lo ejecutan antes de `07_PN1.sql`.
No ejecutes de nuevo `05_IntegridadPN1.sql` sobre una base PN2.

Pruebas en una base temporal: `./Tests/TipificacionDonante.Tests.ps1` (PN1),
agregando `-PN2` para la extensión, `-Legacy` para simular la migración desde
una base sin tipificación del donante, o ambos para migrar PN2.
Para comprobar también las pantallas, compilar `Tests/TipificacionDonante.Ui.Tests.csproj`
en Release y agregar `-UI` al comando de pruebas. El circuito usa la misma base
temporal y comprueba los servicios, mappers y formularios reales.

- `StoredProcedures/99_I18n_Reset_Es_En.sql` es un reinicio de idiomas, no parte de
  una instalación normal. Puede borrar personalizaciones del catálogo de
  idiomas; usalo solo cuando realmente necesites reiniciarlo.
- `INTEGRIDAD_PN1.md` explica la aceptación inicial, la actualización de bases
  PN1 existentes y la recuperación de integridad.

**No ejecutes `00_tecni_salud_schema.sql` sobre una base existente.** Para una base
con datos, elegí el instalador de la tabla anterior según el caso.
