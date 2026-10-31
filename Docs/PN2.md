# PN2 Solicitud, reserva, entrega y trazabilidad

Estado: implementado para validación funcional. Actualizado: 17 de octubre de 2026.

Actualización en curso (octubre de 2026): el pedido incorpora datos de identificación y estudios del receptor, validación por un operador y búsqueda preliminar por componente y grupo. Véanse las reglas, límites y pasos de instalación de [Compatibilidad en PN2](PN2-compatibilidad.md).

PN2 continúa el trabajo de PN1: utiliza las unidades registradas, clasificadas y liberadas allí para atender solicitudes transfusionales. Incluye código C#, persistencia SQL Server, permisos, tres pantallas operativas disponibles desde PN2 y diagramas de negocio editables en draw.io. PN1 y PN2 aparecen como opciones separadas del menú principal. El tab Consultas está oculto por ahora; las vistas Trazabilidad y Reportes y alertas, junto con sus permisos, se conservan en el código. La pantalla independiente de Inventario también se conserva en el código, pero no aparece en el menú.

## Referencias y alcance

La fuente es [Trabajo de Diploma, versión 1.1.0](https://docs.google.com/document/d/1EUrkuuLmCq6YnOGA_lSNvpeQSwCAK-Xd34jLIo4du58/edit?tab=t.0): RFN2, glosario G03, roles G04 y reglas G05. La nomenclatura y la presentación toman como referencia PN1 y los CDUN-12 a CDUN-17 existentes.

El profesional presenta el pedido y los estudios. Un usuario habilitado para registrar solicitudes transcribe los datos; el operador autorizado valida el estudio y atiende la reserva. El solicitante identificado en el pedido y el usuario que lo carga son datos distintos. El documento fuente no fue modificado.

También se confirmaron entregas parciales, cancelación manual con motivo, devolución con reingreso explícito autorizado y reapertura del saldo cuando una entrega se devuelve o descarta. Estas decisiones están implementadas.

Los diagramas de clases y la secuencia de clases quedan pendientes de la validación funcional. La secuencia de roles forma parte de los diagramas de negocio y está preparada junto con los demás diagramas de esta etapa. La futura bitácora de cambios será una entrega documental con DER, tabla copia, casos de uso y explicación de triggers. Esta implementación reutiliza los registros operativos y la protección de integridad existentes.

## Diagramas de negocio

| Artefacto | Archivo editable | Contenido |
| --- | --- | --- |
| Roles | [Diagrama de roles](../Diagramas/Secuencia%20de%20Roles/PN2_DiagramaRoles.drawio) | Responsabilidades e intercambios, cancelación y reingreso. |
| Actividad | [Actividad de negocio](../Diagramas/Secuencia%20de%20Roles/PN2_DiagramaActividadNegocio.drawio) | Flujo principal, decisiones, destinos y alternativas. |
| Modelo conceptual | [Modelo conceptual](../Diagramas/Secuencia%20de%20Roles/PN2_DiagramaModeloConceptual.drawio) | Conceptos, datos y multiplicidades del negocio. |
| Secuencia de roles | [Secuencia de roles](../Diagramas/Secuencia%20de%20Roles/PN2_DiagramaSecuenciaRoles.drawio) | Interacciones entre las personas y el sistema durante la atención. |
| Entrada, Comportamiento y Salida | [E/C/S](../Diagramas/Secuencia%20de%20Roles/PN2_DiagramaEntradaComportamientoSalida.drawio) | Flujo operativo y E9/C11/S10 del comprobante automático al registrar destino. |
| Casos de uso | [CDUN-12 a CDUN-17](../Diagramas/Casos%20de%20Uso/PN2%20-%20CDU-12%20a%20CDU-17%20-%20Casos%20de%20uso.drawio) | Actores y casos existentes; comprobante, cancelación y reingreso. |

Los CDUN conservan sus identificadores. No se agregaron diagramas de clases ni secuencias de clases.

## Roles y permisos

| Rol existente | Operaciones de PN2 | Familia de permisos |
| --- | --- | --- |
| Profesional solicitante | Presentar el pedido y sus estudios; si dispone de acceso, registrar y consultar sus solicitudes. | PN2_PROFESIONAL_SOLICITANTE |
| Operador del banco de sangre | Registrar y consultar solicitudes, consultar reservas, registrar destino y consultar reportes. | PN2_OPERADOR_BANCO |
| Operador autorizado | Registrar solicitudes, validar estudios, consultar unidades candidatas; reservar, cancelar y entregar. | PN1_OPERADOR_AUTORIZADO, ampliada |
| Responsable autorizado | Consultar reportes. La lógica de reingreso de una unidad Devuelta se conserva en el servicio, pero no tiene una acción disponible en la interfaz actual. | PN1_RESPONSABLE_AUTORIZADO, ampliada |
| Auditor | Consultar trazabilidad. | PN2_AUDITOR |
| Administrador del banco de sangre | Acceso a las operaciones de PN2 mediante la familia administrativa existente. | ADMINISTRADOR |

Los permisos se comprueban en Application y SQL. La instalación agrega las familias y sus relaciones sin crear usuarios de demostración. Para revisar el circuito con distintos actores, asignar las familias mediante la gestión de permisos existente y volver a iniciar sesión. El profesional sin permisos operativos adicionales ve solamente sus solicitudes.

## Flujo en lenguaje coloquial

1. **Preparar las unidades en PN1.** Registrar donante y donación, generar unidades, clasificarlas y liberarlas. La unidad debe estar vigente y su componente debe corresponder al solicitado.
2. **Registrar el pedido.** El profesional presenta la orden y el informe. El personal carga solicitante, nombre y documento del receptor, componente, cantidad, grupo ABO, RhD, resultado de anticuerpos, laboratorio, referencia y fecha del estudio. La solicitud puede guardarse sin stock o sin estudio; este se completa después y permanece pendiente de validación.
3. **Validar el estudio y buscar.** Un operador autorizado revisa los datos, la identidad y la vigencia del informe y registra su validación. En **Reservas y entregas**, el sistema muestra unidades candidatas liberadas, vigentes y del componente solicitado, filtradas según el grupo registrado. El filtro es preliminar y solo contempla los nombres de componentes documentados.
4. **Reservar por unidad.** El operador verifica la compatibilidad para el receptor según el protocolo del servicio, ingresa la referencia de esa comprobación y confirma. El sistema vuelve a controlar el estudio, el filtro, el saldo y la disponibilidad; la unidad pasa a Reservada. La atención puede ser parcial.
5. **Entregar la reserva.** Se selecciona una reserva activa y se confirma la entrega. Se vuelven a controlar vigencia, estado y relación con la solicitud. La unidad pasa a Entregada; una reserva sólo puede producir una entrega.
6. **Registrar qué ocurrió.** En Destino de unidades se selecciona la entrega sin destino y se registra Transfundida, Devuelta o Descartada. Devolución y descarte requieren motivo y recuperan una unidad de saldo para reemplazo. La entrega original permanece en el historial.
7. **Cancelar si no se entregará.** Una reserva activa puede cancelarse con motivo. Se conserva el registro y la unidad vuelve a Liberada. La vigencia se vuelve a comprobar antes de cualquier nueva reserva; no hay vencimiento automático de reservas.
8. **Reingresar una devolución.** La lógica de reingreso queda preservada en el servicio: un Responsable autorizado puede llevar una unidad Devuelta a En revisión. La interfaz actual no expone esa acción; antes de volver a reservarla, la unidad debe clasificarse otra vez y liberarse desde PN1.
9. **Consultar el recorrido.** Trazabilidad muestra movimientos con usuario, fecha, estado anterior y nuevo, observación y vínculos con solicitud, reserva y entrega. Reportes y alertas permite revisar stock, pedidos y problemas operativos. Ambas son consultas independientes del circuito de entrega.
10. **Emitir el comprobante.** Al registrar un destino, el sistema genera el PDF de esa entrega. Si el archivo no puede guardarse, queda pendiente para reintento. En Solicitudes transfusionales se puede exportar aparte un reporte manual del estado actual de la solicitud.

El sistema filtra por componente y una regla preliminar de ABO/RhD; la evaluación clínica y las pruebas aplicables corresponden al personal autorizado. No determina por sí solo la aptitud definitiva. No se implementa gestión automatizada de anticuerpos, prioridades ni FEFO.

## Cantidades y estados

Cada reserva corresponde a una unidad y una solicitud; cada reserva tiene cero o una entrega. Una unidad conserva distintas reservas a lo largo de su historia, pero nunca dos activas simultáneas.

| Cantidad visible | Cálculo |
| --- | --- |
| Requeridas | Cantidad registrada en el pedido. |
| Reservadas | Reservas activas todavía sin entregar. |
| Entregadas | Entregas sin destino o con destino Transfundida. No incluye devueltas ni descartadas. |
| Pendientes | Requeridas menos Reservadas menos Entregadas; nunca menor que cero. |

La columna Entregadas representa atención efectiva. El historial conserva también las entregas posteriormente devueltas o descartadas.

| Estado de la solicitud | Cuándo se muestra |
| --- | --- |
| PENDIENTE | Ninguna unidad cubierta por reservas activas o entregas efectivas. |
| PARCIAL | Una parte cubierta y todavía hay saldo. |
| RESERVADA | Todo cubierto entre reservas activas y entregas efectivas, con alguna reserva todavía activa. |
| ENTREGADA | Toda la cantidad cubierta por entregas efectivas. |

Ejemplo: para un pedido de 3, reservar 2 muestra **3 / 2 / 0 / 1**. Entregar una muestra **3 / 1 / 1 / 1**. Devolver esa entrega muestra **3 / 1 / 0 / 2**. Cancelar la otra reserva deja **3 / 0 / 0 / 3**. El orden es Requeridas / Reservadas / Entregadas / Pendientes.

Estados de reserva: ACTIVA, ENTREGADA y CANCELADA. Estados nuevos de unidad: RESERVADA, ENTREGADA, TRANSFUNDIDA y DEVUELTA; se conservan EN_REVISION, LIBERADA, BLOQUEADA y DESCARTADA de PN1. El descarte es terminal; el reingreso se admite únicamente desde DEVUELTA.

## Entradas, comportamientos y salidas

| Entrada | Comportamiento | Salida |
| --- | --- | --- |
| E1. Solicitud, datos del receptor y estudio de laboratorio. | C1. Registrar el pedido y transcribir el estudio. | S1. Solicitud registrada con estudio pendiente de validación. |
| E2. Solicitud e informe original. | C2. Validar estudio y consultar candidatos por componente, estado, vigencia y grupo. | S2. Estudio validado y unidades candidatas. |
| E1, E2 y E10. Solicitud, inventario y resultado de pruebas. | C3. Verificar saldo y confirmar compatibilidad según el protocolo. | S1 si falta disponibilidad o validación; de lo contrario continúa la atención. |
| E10. Unidad candidata, resultado de compatibilidad y su referencia. | C4. Confirmar la evaluación y reservar la unidad. | S3. Reserva registrada con referencia. |
| E3. Reserva y datos de entrega. | C5. Confirmar habilitación y registrar entrega. | S4. Entrega registrada. |
| E4. Entrega, destino, responsable, fecha y motivo cuando corresponde. | C6. Registrar transfusión, devolución o descarte. | S5. Destino registrado y saldo actualizado. |
| E5. Unidad, período, estado, grupo o componente. | C7. Consultar trazabilidad. | S6. Historial consultado. |
| E6. Consulta de reportes y alertas. | C8. Generar reportes y alertas. | S7. Resultados disponibles. |
| E7. Reserva activa y motivo. | C9. Cancelar reserva. | S8. Reserva cancelada, unidad liberada y saldo actualizado. |
| E8. Unidad devuelta, responsable autorizado y motivo. | C10. Reingresar a En revisión. | S9. Unidad pendiente de nueva clasificación y liberación PN1. |
| E9. Destino guardado de una entrega. | C11. Generar automáticamente el comprobante de esa entrega. | S10. PDF con el destino o comprobante pendiente de reintento. |

## Pantallas y consultas

Desde el menú principal aparecen las opciones habilitadas por los permisos de la sesión:

- **Solicitudes transfusionales:** registro del pedido y cantidades de atención.
- **Reservas y entregas:** selección de solicitud y stock, reserva, cancelación y entrega; historial de reservas.
- **Destino de unidades:** entregas y registro de transfusión, devolución o descarte.
- **Consultas (oculto en la navegación actual):** las vistas Trazabilidad y Reportes y alertas permanecen implementadas. Trazabilidad permite filtrar por unidad, período, estado resultante del movimiento, grupo y componente. Reportes y alertas muestra inventario actual, solicitudes, saldos, reservas, movimientos y alertas operativas.

Las alertas incluyen unidades vencidas, reservas activas cuya unidad ya no reúne las condiciones y solicitudes con saldo sin stock apto del componente. Son resultados de consulta, sin envío automático ni umbrales de días agregados. Los filtros de fecha delimitan registros y movimientos; no reconstruyen el stock de una fecha pasada. El período incluye el día completo seleccionado como Hasta.

Las tres pantallas PN2 disponibles en la navegación tienen textos en español e inglés y utilizan el mecanismo de integridad de PN1. Las vistas de consultas también se conservan implementadas, pero su tab superior no se agrega al menú. La lista de unidades disponibles está dentro de Reservas y entregas. Un error de consulta se informa y bloquea la selección de resultados anteriores para evitar operar sobre datos desactualizados.

**Copiar comprobante PDF** permite recuperar el original de una entrega; **Exportar reporte PDF** genera un documento con datos actuales. Para ambas consultas PN2 se requieren `SOLICITUD_VER` y `RESERVA_VER`. El estado Entregada no es un cierre irreversible: una devolución o descarte posterior puede reabrir el saldo. Véase [Comprobantes y balanceo](Comprobantes-PN1-PN2.md).

## Código y base de datos

Se conservan las capas y la nomenclatura `_380_jh` del proyecto:

| Capa o artefacto | Ubicación |
| --- | --- |
| Modelos y estados | [Domain/PN2](../Domain/PN2/) |
| Acceso SQL | [DAL/PN2](../DAL/PN2/) |
| Repositorio | [Repository/PN2](../Repository/PN2/) |
| Operaciones y permisos | [PN2ApplicationService](../Application/PN2/PN2ApplicationService.cs) |
| Pantallas | [UI/Views/PN2](../UI/Views/PN2/) |
| Tablas y procedimientos | [PN2.sql](../Database/StoredProcedures/PN2.sql) |
| Permisos y traducciones | [PN2_Catalogos.sql](../Database/StoredProcedures/PN2_Catalogos.sql) |
| Integridad y recuperación común | [IntegridadPN2.sql](../Database/StoredProcedures/IntegridadPN2.sql) |
| Instalación | [InstalarPN2.ps1](../Database/InstalarPN2.ps1) |

Se agregan SolicitudTransfusional, Reserva y Entrega. Unidad incorpora ubicación y MovimientoUnidad conserva referencias opcionales a solicitud, reserva y entrega. Los índices y las transacciones evitan reservas simultáneas de la misma unidad, duplicación de entrega y sobreasignación de la última cantidad pendiente.

La protección DVH/DVV y la recuperación existentes incluyen las nuevas tablas y columnas. Se preservan los hashes anteriores cuando las nuevas columnas no tienen datos. La migración rechaza una base con integridad inválida; después de instalar PN2 no se debe ejecutar una versión antigua de IntegridadPN1.sql que omita estas tablas. El script actual contiene ese control.

### Instalación sobre una base PN1 existente

1. Configurar [DAL/App.local.example.config](../DAL/App.local.example.config) como `DAL/App.local.config`, siguiendo el README. La base debe tener PN1 e integridad instalados y válidos.
2. Cerrar la aplicación antes de actualizar.
3. Desde la raíz del proyecto ejecutar:

```powershell
./Database/InstalarPN2.ps1
```

Opcionalmente se pueden indicar `-Servidor` y `-Base`. El instalador usa la conexión local sin mostrar sus credenciales, comprueba la integridad y crea un backup COPY_ONLY con checksum en la carpeta de backups de SQL Server; verifica ese archivo antes de migrar. La cuenta SQL necesita permisos de backup y de modificación de esquema.

Luego ejecuta PN2_Catalogos.sql, la actualización de PN1.sql, PN2.sql, IntegridadPN2.sql y ComprobantesAutomaticos.sql en una transacción y verifica nuevamente la integridad antes de confirmar. Un error revierte la actualización. No usar el esquema inicial completo para actualizar una base con datos. Una base que ya tiene PN2 puede actualizar solo comprobantes con `./Database/InstalarComprobantes.ps1`.

4. Compilar `IngDeSoftware.sln` y abrir la UI. Volver a iniciar sesión para cargar los permisos. El administrador existente recibe las operaciones de PN2; los demás usuarios usan las familias asignadas.

### Estado de la instalación local

La versión anterior de PN2 se aplicó el 17 de octubre de 2026 a `HemovidaGest` en el servidor local `.`. Los cambios de estudios y compatibilidad descritos en [PN2-compatibilidad](PN2-compatibilidad.md) requieren una nueva migración; no se aplican automáticamente.

El respaldo anterior a la actualización se conserva en la carpeta de backups de SQL Server con el nombre `HemovidaGest_AntesPN2_20261017_184644_18913565e0f147428488c69e24604568.bak`; se verificó con `RESTORE VERIFYONLY` y checksum. Para comenzar la revisión local alcanza con abrir la aplicación e iniciar sesión nuevamente.

## Recorrido para la validación funcional

1. Preparar en PN1 al menos tres unidades vigentes, clasificadas y liberadas con un componente reconocido y grupos candidatos para el estudio del receptor.
2. Crear una solicitud por tres unidades, cargar el estudio y comprobar **3 / 0 / 0 / 3**. Un operador autorizado valida el informe.
3. Confirmar la compatibilidad de dos candidatas, ingresar la referencia y reservarlas; comprobar **3 / 2 / 0 / 1**. Intentar reservar una de ellas nuevamente: debe rechazarse.
4. Entregar una reserva y comprobar **3 / 1 / 1 / 1**. No debe poder entregarse otra vez la misma reserva.
5. Devolverla con motivo: comprobar **3 / 1 / 0 / 2** y conservar la entrega en el historial.
6. Cancelar la reserva restante con motivo: comprobar **3 / 0 / 0 / 3**. La unidad queda Liberada.
7. Intentar reservar directamente la Devuelta: debe rechazarse. La interfaz no ofrece el reingreso; la operación de servicio se conserva y, al volver a habilitarse desde una pantalla, debe dejar la unidad En revisión para que se clasifique otra vez en PN1.
8. Clasificar y liberar; reservarla nuevamente, entregarla y registrar Transfundida. Revisar que aparezcan ambos ciclos en Trazabilidad.
9. En otra entrega registrar Descartada: debe reabrir saldo y dejar la unidad terminal, sin permitir reingreso.
10. Completar el pedido con reemplazos y comprobar ENTREGADA, saldo cero y rechazo de reservas adicionales.
11. Probar sin stock y con una unidad vencida o bloqueada: la solicitud se conserva pendiente y no se confirma una reserva inválida.
12. Revisar reportes con y sin período, y probar cuentas con permisos de profesional, operador, responsable y auditor.

### Comprobaciones realizadas

La implementación anterior completó comprobaciones SQL, controles de concurrencia, vencimiento y migración, además de integración Application/UI y revisión de las pantallas PN2. La navegación actual expone las pantallas operativas de PN2; oculta el tab Consultas y la pantalla independiente de Inventario. La validación funcional del usuario queda pendiente.

## Continuación después de validar

Con las observaciones funcionales resueltas se podrán preparar los diagramas de clases y la secuencia de clases siguiendo PN1 y los nombres reales del código.

Para retomar la bitácora, el pedido puede ser:

> Tomá como base Docs/PN2.md, el código y los diagramas de negocio PN2 validados. Documentá la bitácora de cambios con su DER, tabla copia, casos de uso y funcionamiento previsto de los triggers. Conservá la nomenclatura del proyecto y distinguí lo implementado de lo propuesto. La entrega de bitácora es únicamente documental.

Ese diseño deberá precisar operaciones auditadas, datos anteriores y nuevos, identificación del actor, comportamiento si falla el registro y consulta del historial. La interpretación de tabla copia se acordará con lo exigido por la cátedra.
