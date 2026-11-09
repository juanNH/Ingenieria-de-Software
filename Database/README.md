# Orden de ejecución de los scripts SQL

Usá este archivo como índice antes de ejecutar algo de `Database`. El orden
depende de si la base es nueva o si ya tiene PN1/PN2.

Los prefijos indican el orden para crear una base nueva desde cero. Para una
base existente, usá el instalador de la sección correspondiente; cada uno aplica
la secuencia de migración adecuada.

## Base nueva con PN1 y PN2

Ejecutá los archivos uno por uno, en este orden, conectado a SQL Server con
permisos para crear y modificar la base `HemovidaGest`:

1. `00_tecni_salud_schema.sql` — crea la base, tablas y datos iniciales.
2. `StoredProcedures/01_AuthUsuario.sql` — procedimientos de usuarios.
3. `StoredProcedures/02_Bitacora.sql` — bitácora.
4. `StoredProcedures/03_Auditoria.sql` — auditoría.
5. `StoredProcedures/04_Permisos.sql` — permisos y traducciones de PN1.
6. `StoredProcedures/05_IntegridadPN1.sql` — controles e historial protegido de PN1.
7. `StoredProcedures/06_MaestrosPN1.sql` — catálogos administrables y sus controles.
8. `StoredProcedures/06a_CompatibilidadTransfusional.sql` — matriz configurable, reglas iniciales, auditoría e integridad.
9. `StoredProcedures/06b_TipificacionDonante.sql` y luego `StoredProcedures/07_PN1.sql` — procedimientos de donantes, donaciones y unidades.
10. `StoredProcedures/08_PN2_Catalogos.sql` — permisos y traducciones de PN2.
11. `StoredProcedures/09_PN2.sql` — solicitudes, reservas, entregas y trazabilidad.
12. `StoredProcedures/09a_RelacionesSolicitudPN2.sql` — aplica las claves foráneas de la solicitud a los catálogos y al usuario que valida el estudio.
13. `StoredProcedures/10_IntegridadPN2.sql` — extiende la integridad para PN2.
14. `StoredProcedures/11_ComprobantesAutomaticos.sql` — comprobantes automáticos.
15. `StoredProcedures/12_DiagramasSSMS.sql` — crea los diagramas CDUN-01 a CDUN-11 en Database Diagrams de SSMS, con sus diseños guardados.
16. `StoredProcedures/13_AjusteCantidadDonacion.sql` — corrección de cantidades e historial de donaciones.

Para instalar solo PN1, detenete después del paso 9. Los pasos 10 a 14 son la
extensión PN2. `09_PN2.sql` requiere PN1 y `06a_CompatibilidadTransfusional.sql` instalados; `10_IntegridadPN2.sql` requiere
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
| Incorporar grupo sanguíneo y Rh persistidos en el donante (PN1 o PN1+PN2) | `./Database/InstalarMaestrosPN1.ps1` |
| Incorporar la corrección de cantidades en una base con PN2 y comprobantes | `./Database/InstalarMaestrosPN1.ps1` |

`InstalarPN2.ps1` aplica, en orden, `08_PN2_Catalogos.sql`, `06_MaestrosPN1.sql`,
`06a_CompatibilidadTransfusional.sql`, `06b_TipificacionDonante.sql`, `07_PN1.sql`, `09_PN2.sql`, `09a_RelacionesSolicitudPN2.sql`, `10_IntegridadPN2.sql`, `11_ComprobantesAutomaticos.sql`, `12_DiagramasSSMS.sql` y `13_AjusteCantidadDonacion.sql`. El script 12 crea los diagramas que falten y conserva los existentes. Requiere permisos de propietario de la base. Los diagramas dependen del esquema de tablas instalado.

Para recuperar solo los diagramas después de reconstruir el esquema, ejecutá desde
la raíz del proyecto `sqlcmd -S . -E -d HemovidaGest -b -i Database/StoredProcedures/12_DiagramasSSMS.sql`.
Si tu instancia tiene otro nombre, reemplazá `.` por el nombre del servidor.
Después actualizá la carpeta **Database Diagrams** en SSMS. El script restaura los
diseños guardados en el repositorio; no recopila automáticamente cambios que
hagas después en SSMS ni exporta imágenes.

`InstalarMaestrosPN1.ps1` actualiza maestros y procedimientos relacionados; si
PN2 ya está instalado, también actualiza PN2 y los comprobantes. Incluye las
compatibilidades configurables y conserva las reglas editadas o desactivadas.
Ver [cómo configurar las reglas](../Docs/Compatibilidad-configurable.md). No ejecutes a
mano esos mismos archivos además del instalador.

## Corrección de cantidad declarada

En **Unidades**, seleccioná una unidad de la donación y abrí **Cantidad e historial**. `UNIDAD_VER` habilita la consulta y `DONACION_AJUSTAR_CANTIDAD` habilita los cambios. El responsable autorizado y el administrador reciben ambos permisos. El historial queda en solo lectura desde la primera liberación o al registrarse el comprobante de PN1.

`tr_Donacion_AjusteCantidad` recibe la versión que estaba viendo el responsable, la cantidad nueva, el motivo y los códigos seleccionados para la baja. La aplicación envía los identificadores de esas unidades y el trigger registra sus códigos en la auditoría. El ajuste, los movimientos y el historial protegido se confirman juntos. Al aumentar, el trigger genera unidades en revisión. Al reducir, anula las unidades seleccionadas en revisión sin clasificar y conserva sus códigos. Usar una cantidad anterior aplica las mismas reglas y agrega una versión al historial.

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
