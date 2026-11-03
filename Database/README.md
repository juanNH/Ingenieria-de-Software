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
8. `StoredProcedures/07_PN1.sql` — procedimientos de donantes, donaciones y unidades.
9. `StoredProcedures/08_PN2_Catalogos.sql` — permisos y traducciones de PN2.
10. `StoredProcedures/09_PN2.sql` — solicitudes, reservas, entregas y trazabilidad.
11. `StoredProcedures/10_IntegridadPN2.sql` — extiende la integridad para PN2.
12. `StoredProcedures/11_ComprobantesAutomaticos.sql` — comprobantes automáticos.

Para instalar solo PN1, detenete después del paso 8. Los pasos 9 a 12 son la
extensión PN2. `09_PN2.sql` requiere PN1 instalado; `10_IntegridadPN2.sql` requiere
PN2; y los comprobantes requieren ambas integridades.

## Base existente

Preferí los instaladores PowerShell desde la raíz del proyecto. Preparan un
respaldo verificado, aplican los scripts en una transacción y revisan la
integridad al finalizar:

| Situación | Comando |
| --- | --- |
| Incorporar o actualizar Maestros en PN1 o PN1+PN2 | `./Database/InstalarMaestrosPN1.ps1` |
| Incorporar PN2 a una base PN1 válida | `./Database/InstalarPN2.ps1` |
| Actualizar únicamente los comprobantes de una base PN2 | `./Database/InstalarComprobantes.ps1` |

`InstalarPN2.ps1` aplica, en orden, `08_PN2_Catalogos.sql`, `06_MaestrosPN1.sql`,
`07_PN1.sql`, `09_PN2.sql`, `10_IntegridadPN2.sql` y `11_ComprobantesAutomaticos.sql`.
`InstalarMaestrosPN1.ps1` actualiza maestros y procedimientos relacionados; si
PN2 ya está instalado, también actualiza PN2 y los comprobantes. No ejecutes a
mano esos mismos archivos además del instalador.

## Scripts auxiliares

- `StoredProcedures/99_I18n_Reset_Es_En.sql` es un reinicio de idiomas, no parte de
  una instalación normal. Puede borrar personalizaciones del catálogo de
  idiomas; usalo solo cuando realmente necesites reiniciarlo.
- `INTEGRIDAD_PN1.md` explica la aceptación inicial, la actualización de bases
  PN1 existentes y la recuperación de integridad.

**No ejecutes `00_tecni_salud_schema.sql` sobre una base existente.** Para una base
con datos, elegí el instalador de la tabla anterior según el caso.
