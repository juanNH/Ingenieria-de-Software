# PN2 · Solicitud, reserva, entrega y destino de unidades

Alcance: CDUN-05 a CDUN-08 y su consulta de abastecimiento complementaria. Documentación actualizada: 15 de noviembre de 2026.

La solicitud incorpora la identificación y el estudio del receptor, su validación por un operador y la búsqueda preliminar de unidades por componente y grupo. Las reglas y los pasos de instalación están en [Compatibilidad en PN2](PN2-compatibilidad.md).

PN2 continúa el trabajo de PN1: utiliza las unidades registradas, clasificadas y liberadas allí para atender solicitudes transfusionales. Incluye código C#, persistencia SQL Server, permisos, tres pantallas operativas disponibles desde PN2 y diagramas de negocio editables en draw.io. PN1 y PN2 aparecen como opciones separadas del menú principal. **Consultas** contiene únicamente **Abastecimiento y riesgo de vencimiento**, visible con el permiso `REPORTE_VER`. Las vistas Trazabilidad, Reportes y alertas e Inventario se conservan en el código, pero no aparecen en la navegación actual.

## Referencias y alcance

La descomposición funcional y las fichas de CDUN-05 a CDUN-08 están en RFN2 del [Trabajo de Diploma](https://docs.google.com/document/d/1EUrkuuLmCq6YnOGA_lSNvpeQSwCAK-Xd34jLIo4du58/edit?tab=t.0).

El receptor presenta el pedido del profesional y el estudio al personal de registro. Un usuario habilitado transcribe los datos y el operador autorizado valida el estudio y atiende la reserva. El profesional identificado en el pedido y el usuario que lo carga son datos distintos.

El recorte de los diagramas comprende tres actores funcionales: **Personal de registro** (CDUN-05), **Operador autorizado** (CDUN-06) y **Operador del banco de sangre** (CDUN-07 y su comprobante automático CDUN-08). El flujo comienza en Personal de registro con E3, el pedido, los datos del receptor y el estudio disponible. Esta distribución representa responsabilidades del proceso; los perfiles y permisos existentes se conservan en el código.

Desde **Receptor registrado** se puede recuperar un receptor para otra solicitud; también se busca automáticamente al salir del campo documento. Cada documento aparece una sola vez, con el nombre de su última solicitud y el último par grupo sanguíneo/Rh conocido (prioriza la fecha del estudio). **Nuevo receptor** limpia el formulario. El registro se obtiene de las solicitudes persistidas y protegidas por integridad: incluye datos históricos y continúa disponible al reiniciar. Una solicitud posterior sin grupo no borra el grupo conocido.

La selección completa nombre, documento, grupo y Rh, y limpia los datos del estudio. Se permite registrar el grupo y factor sin un estudio; el pedido queda pendiente hasta cargar y validar el informe de esa solicitud. No se reutilizan laboratorios, resultados ni validaciones anteriores. Los profesionales sólo ven receptores de sus solicitudes; los operadores con acceso general conservan ese alcance. Para actualizar una base existente, ejecutar `Database/InstalarPN2.ps1` con la aplicación cerrada.

Las pruebas de reutilización están incluidas en `Tests/PN2.Compatibilidad.Tests.ps1`, sobre una copia descartable de la base configurada. Para incluir Application, DAL y WinForms, compilar `Tests/PN2.Receptores.Tests.csproj` con MSBuild de Visual Studio (`/restore`) y ejecutar el script con `-ReceptoresUiRunner (Resolve-Path Tests/bin/Receptores/PN2.Receptores.Tests.exe)`. No se modifica la base de trabajo ni `App.local.config`.

El circuito operativo admite entregas parciales, cancelación de reservas con motivo y reapertura del saldo cuando una entrega se devuelve o descarta. La lógica de reingreso se conserva en el servicio, pero no es una acción disponible en la interfaz actual.

La secuencia de roles describe el flujo de negocio. Las secuencias de clases muestran los pasos y alternativas de cada CDUN desde la UI hasta SQL Server. Los DER muestran las tablas y claves foráneas que intervienen en cada caso. Los [CDU del sistema](CDU.md) también cuentan con clases y secuencias normalizadas. El [modelo final G06](Modelo-clases-final.md) integra los tipos y relaciones de las siete capas, y el [modelo final G07](Modelo-datos-final.md) reúne las 32 tablas y 53 claves foráneas del esquema físico completo.

## Diagramas de negocio

| Artefacto | Archivo editable | Contenido |
| --- | --- | --- |
| Roles | [Diagrama de roles](../Diagramas/Roles/PN2_DiagramaRoles.drawio) | Responsabilidades e intercambios de los tres actores del recorte operativo. |
| Actividad | [Actividad de negocio](../Diagramas/Diagramas%20de%20actividad/PN2_DiagramaActividadNegocio.drawio) | Flujo principal, decisiones, destinos y alternativas. |
| Modelo conceptual | [Modelo conceptual](../Diagramas/Modelos%20conceptuales/PN2_DiagramaModeloConceptual.drawio) | Conceptos, datos y multiplicidades del negocio. |
| Secuencia de roles | [Secuencia de roles](../Diagramas/Secuencia%20de%20Roles/PN2_DiagramaSecuenciaRoles.drawio) | Interacciones entre las personas y el sistema durante la atención. |
| Entrada, Comportamiento y Salida | [E/C/S](../Diagramas/Entrada%20Comportamiento%20Salida/PN2_DiagramaEntradaComportamientoSalida.drawio) | Flujo operativo y C12/S7 del comprobante automático al registrar destino. |
| Casos de uso | [CDUN-05 a CDUN-08](../Diagramas/Casos%20de%20Uso/PN2%20-%20CDUN-05%20a%20CDUN-08%20-%20Casos%20de%20uso.drawio) | Registro, validación, reserva, entrega, destino y comprobante. |
| Clases | [CDUN-05 a CDUN-08](../Diagramas/Diagramas%20de%20clases/PN2%20-%20CDUN-05%20a%20CDUN-08%20-%20Clases.drawio) | Una página por caso. |
| Secuencia de clases | [CDUN-05 a CDUN-08](../Diagramas/Diagramas%20de%20Secuencia/PN2%20-%20CDUN-05%20a%20CDUN-08%20-%20Secuencia.drawio) | Una página por caso con el flujo principal y las alternativas. |
| DER | [Diagramas y exportaciones de SSMS](../Diagramas/Diagramas%20Entidad%20Relacion/LEEME.md) | Tablas y claves foráneas de cada caso. |

La carga posterior del estudio es una alternativa del CDUN-05. El CDUN-06 —Validar estudio, reservar y entregar unidad— reúne las acciones de la pantalla Reservas y entregas e incluye la cancelación de una reserva activa como alternativa. El CDUN-07 incluye al CDUN-08: el comprobante se emite automáticamente cuando se registra el destino correctamente; si falla la emisión queda pendiente de reintento.

## Roles y permisos

| Rol existente | Operaciones de PN2 | Familia de permisos |
| --- | --- | --- |
| Profesional solicitante | Presentar el pedido y sus estudios; si dispone de acceso, registrar y consultar sus solicitudes. | PN2_PROFESIONAL_SOLICITANTE |
| Operador del banco de sangre | Registrar y consultar solicitudes, consultar reservas, registrar destino y consultar reportes. | PN2_OPERADOR_BANCO |
| Operador autorizado | Registrar solicitudes, validar estudios, consultar unidades candidatas; reservar, cancelar y entregar. | PN1_OPERADOR_AUTORIZADO, ampliada |
| Responsable autorizado | Consultar reportes. La lógica de reingreso de una unidad Devuelta se conserva en el servicio, pero no tiene una acción disponible en la interfaz actual. | PN1_RESPONSABLE_AUTORIZADO, ampliada |
| Auditor | Permiso de trazabilidad conservado para la vista de soporte, fuera de la navegación actual. | PN2_AUDITOR |
| Administrador del banco de sangre | Acceso a las operaciones de PN2 mediante la familia administrativa existente. | ADMINISTRADOR |

La UI habilita las pantallas y acciones según los permisos del usuario. Las operaciones de registro no repiten esas comprobaciones en Application; las consultas SQL conservan los controles de acceso y de alcance de los datos. La instalación agrega las familias y sus relaciones sin crear usuarios de demostración. Para revisar el circuito con distintos actores, asignar las familias mediante la gestión de permisos existente y volver a iniciar sesión. El profesional sin permisos operativos adicionales ve solamente sus solicitudes.

El reporte de abastecimiento requiere `REPORTE_VER` en la navegación, el servicio de aplicación y SQL, además de integridad válida. Su presencia en Consultas no incorpora las vistas de soporte conservadas en el código.

## Flujo operativo

1. **Preparar las unidades en PN1.** Registrar donante y donación, generar unidades, clasificarlas y liberarlas. La unidad debe estar vigente y su componente debe corresponder al solicitado.
2. **Registrar el pedido.** El profesional presenta la orden y el informe. El personal carga solicitante, nombre y documento del receptor, componente, cantidad, grupo ABO, RhD, resultado de anticuerpos, laboratorio, referencia y fecha del estudio. La solicitud puede guardarse sin stock o sin estudio; este se completa después y permanece pendiente de validación.
3. **Validar el estudio y buscar.** Un operador autorizado revisa los datos, la identidad y la vigencia del informe y registra su validación. En **Reservas y entregas**, el sistema muestra unidades candidatas liberadas, vigentes y del componente solicitado, filtradas según el grupo registrado. El filtro es preliminar y utiliza las combinaciones activas configuradas en Maestros → Compatibilidades.
4. **Reservar por unidad.** El operador verifica la compatibilidad para el receptor según el protocolo del servicio, ingresa la referencia de esa comprobación y confirma. El sistema vuelve a controlar el estudio, el filtro, el saldo y la disponibilidad; la unidad pasa a Reservada. La atención puede ser parcial.
5. **Entregar la reserva.** Se selecciona una reserva activa y se confirma la entrega. Se vuelven a controlar vigencia, estado y relación con la solicitud. La unidad pasa a Entregada; una reserva sólo puede producir una entrega.
6. **Registrar el destino.** En Destino de unidades se elige un estado configurado en Maestros. Transfundida cubre el pedido; cualquier otro estado reabre el saldo para reemplazo y queda en el historial. Devolución y descarte requieren motivo.
7. **Cancelar si no se entregará.** Una reserva activa puede cancelarse con motivo. Se conserva el registro y la unidad vuelve a Liberada. La vigencia se vuelve a comprobar antes de cualquier nueva reserva; no hay vencimiento automático de reservas.
8. **Emitir el comprobante.** Al registrar un destino, el sistema genera el PDF de esa entrega. Si el archivo no puede guardarse, queda pendiente para reintento. En Solicitudes transfusionales se puede exportar aparte un reporte manual del estado actual de la solicitud.

### Funciones conservadas fuera de la navegación actual

El servicio conserva el reingreso de una unidad Devuelta a En revisión por un Responsable autorizado. Antes de reservarla otra vez, debe clasificarse y liberarse desde PN1. La interfaz actual no ofrece esa acción.

Las vistas Trazabilidad y Reportes y alertas permanecen en el código sin entradas de menú. Trazabilidad consulta movimientos por unidad, período, estado resultante, grupo y componente. Reportes y alertas reúne inventario, solicitudes, saldos, reservas, movimientos y alertas operativas. Sus filtros de fecha delimitan registros y movimientos; no reconstruyen el stock de una fecha pasada. La vista independiente de Inventario tampoco tiene una entrada de menú: la lista de unidades disponibles está dentro de Reservas y entregas.

El sistema filtra por componente y una regla preliminar de ABO/RhD; la evaluación clínica y las pruebas aplicables corresponden al personal autorizado. No determina por sí solo la aptitud definitiva. No se implementa gestión automatizada de anticuerpos, prioridades clínicas ni FEFO.

## Cantidades y estados

Cada reserva corresponde a una unidad y una solicitud; cada reserva tiene cero o una entrega. Una unidad conserva distintas reservas a lo largo de su historia, pero nunca dos activas simultáneas.

| Cantidad visible | Cálculo |
| --- | --- |
| Requeridas | Cantidad registrada en el pedido. |
| Reservadas | Reservas activas todavía sin entregar. |
| Entregadas | Entregas sin destino o con estado Transfundida. Un destino distinto de Transfundida reabre el saldo. |
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

Las referencias E3, C7–C12 y S5–S7 corresponden al flujo principal del recorte en los diagramas de roles, actividad, secuencia de roles y E/C/S. Las consultas y operaciones complementarias se describen aparte.

| Entrada | Comportamiento | Salida |
| --- | --- | --- |
| E3. Pedido, datos del receptor y estudio disponible. | C7. Registrar la solicitud y transcribir el estudio informado. | S5. Solicitud registrada con estudio pendiente de validación. |
| Solicitud e informe original. | C8. Validar estudio. | Estudio validado para continuar la reserva. |
| Unidad candidata, resultado de compatibilidad y su referencia. | C9. Reservar unidad compatible. | Reserva registrada con referencia. |
| Reserva y datos de entrega. | C10. Confirmar habilitación y registrar entrega. | S6. Entrega registrada. |
| Entrega, estado de destino, responsable, fecha y motivo cuando corresponde. | C11. Registrar el estado de destino configurado. | Estado registrado y saldo actualizado. |
| Destino guardado de una entrega. | C12. Generar automáticamente el comprobante de esa entrega. | S7. PDF con el destino o comprobante pendiente de reintento. |

La consulta de candidatas forma parte de la reserva de CDUN-06, y la cancelación de una reserva activa es una alternativa de ese mismo caso. El reporte de abastecimiento complementa el análisis del proceso sin agregar una operación al flujo principal de CDUN-05 a CDUN-08. Las vistas de soporte y el reingreso conservados en el código mantienen el alcance indicado anteriormente.

## Pantallas y consultas

Desde el menú principal aparecen las opciones habilitadas por los permisos de la sesión:

- **PN2 → Solicitudes transfusionales:** registro del pedido y cantidades de atención.
- **PN2 → Reservas y entregas:** selección de solicitud y stock, reserva, cancelación y entrega; historial de reservas.
- **PN2 → Destino de unidades:** entregas y registro del estado de destino configurado en Maestros.
- **Consultas → Abastecimiento y riesgo de vencimiento:** única pantalla de reporte inteligente, habilitada por `REPORTE_VER`. Presenta filtros, cuatro indicadores, una grilla por cohorte exacta e interpretación de la fila seleccionada, sin pestañas adicionales.

El reporte relaciona stock liberado vigente y no reservado, unidades próximas a vencer, demanda del período y saldo actual. Calcula cobertura histórica y déficit por componente, grupo y Rh exactos; las solicitudes sin estudio validado aparecen separadas con las métricas por cohorte como N/A. El período incluye ambos días completos y el horizonte de vencimiento admite entre 1 y 365 días. **Solo alertas** filtra las filas visibles y actualiza los indicadores. Los cálculos y prioridades se detallan en [ReporteInteligente.md](ReporteInteligente.md).

Las tres pantallas operativas y el reporte de abastecimiento tienen textos en español e inglés y utilizan el mecanismo de integridad. Un error de consulta se informa e impide operar con resultados anteriores; en el reporte también se vacían las cifras anteriores hasta una consulta válida. Las métricas no asignan unidades ni sustituyen la validación del estudio y de la compatibilidad.

**Copiar comprobante PDF** permite recuperar el original de una entrega; **Exportar reporte PDF** genera un documento con datos actuales. Para ambas consultas PN2 se requieren `SOLICITUD_VER` y `RESERVA_VER`. El estado Entregada no es un cierre irreversible: una devolución o descarte posterior puede reabrir el saldo. Véase [Comprobantes y balanceo](Comprobantes-PN1-PN2.md).

## Código y base de datos

Se conservan las capas y la nomenclatura `_380_jh` del proyecto:

| Capa o artefacto | Ubicación |
| --- | --- |
| Modelos y estados | [Domain/PN2](../Domain/PN2/) |
| Acceso SQL | [DAL/PN2](../DAL/PN2/) |
| Repositorio | [Repository/PN2](../Repository/PN2/) |
| Operaciones de PN2 | [PN2ApplicationService](../Application/PN2/PN2ApplicationService.cs) |
| Pantallas | [UI/Views/PN2](../UI/Views/PN2/) |
| Tablas y procedimientos | [09_PN2.sql](../Database/StoredProcedures/09_PN2.sql) |
| Permisos y traducciones | [08_PN2_Catalogos.sql](../Database/StoredProcedures/08_PN2_Catalogos.sql) |
| Integridad y recuperación común | [10_IntegridadPN2.sql](../Database/StoredProcedures/10_IntegridadPN2.sql) |
| Instalación | [InstalarPN2.ps1](../Database/InstalarPN2.ps1) |

Se agregan SolicitudTransfusional, Reserva y Entrega. Unidad incorpora ubicación y MovimientoUnidad conserva referencias opcionales a solicitud, reserva y entrega. Los índices y las transacciones evitan reservas simultáneas de la misma unidad, duplicación de entrega y sobreasignación de la última cantidad pendiente.

La protección DVH/DVV y la recuperación existentes incluyen las nuevas tablas y columnas. Se preservan los hashes anteriores cuando las nuevas columnas no tienen datos. La migración rechaza una base con integridad inválida; después de instalar PN2 no se debe ejecutar una versión antigua de 05_IntegridadPN1.sql que omita estas tablas. El script actual contiene ese control.

### Instalación sobre una base PN1 existente

1. Configurar [DAL/App.local.example.config](../DAL/App.local.example.config) como `DAL/App.local.config`, siguiendo el README. La base debe tener PN1 e integridad instalados y válidos.
2. Cerrar la aplicación antes de actualizar.
3. Desde la raíz del proyecto ejecutar:

```powershell
./Database/InstalarPN2.ps1
```

Opcionalmente se pueden indicar `-Servidor` y `-Base`. El instalador usa la conexión local sin mostrar sus credenciales, comprueba la integridad y crea un backup COPY_ONLY con checksum en la carpeta de backups de SQL Server; verifica ese archivo antes de migrar. La cuenta SQL necesita permisos de backup y de modificación de esquema.

Luego ejecuta `08_PN2_Catalogos.sql`, `06_MaestrosPN1.sql`, `06a_CompatibilidadTransfusional.sql`, `06b_TipificacionDonante.sql`, `07_PN1.sql`, `09_PN2.sql`, `09a_RelacionesSolicitudPN2.sql`, `10_IntegridadPN2.sql`, `11_ComprobantesAutomaticos.sql`, `12_DiagramasSSMS.sql`, `13_AjusteCantidadDonacion.sql`, `14_DiagramasCDUSSMS.sql`, `15_AbastecimientoRiesgo.sql`, `15a_AbastecimientoUI.sql` y `16_DiagramasFinalesSSMS.sql` en una transacción y verifica nuevamente la integridad y la presencia de los siete CDU nativos antes de confirmar. Los scripts 15 y 15a incorporan el [reporte de abastecimiento](ReporteInteligente.md) y sus traducciones. Los scripts 12, 14 y 16 dejan **21 DER nativos** en **Database Diagrams** de SSMS: diez vistas de los casos de negocio, siete de los casos de uso del sistema y cuatro del [modelo final G07](Modelo-datos-final.md). La creación de una base nueva mediante el instalador incluye los mismos reportes y diagramas. Un error revierte la actualización. No usar el esquema inicial completo para actualizar una base con datos.

Una base PN2 actualizada que ya tiene comprobantes puede incorporar solo los scripts 15, 15a y 16 con:

```powershell
./Database/InstalarEntregaFinal.ps1 -Servidor . -Base HemovidaGest
```

Este actualizador usa autenticación integrada, crea y verifica un respaldo antes de aplicar los tres scripts en una transacción y comprueba la integridad al finalizar. Para actualizar únicamente comprobantes, continúa disponible `./Database/InstalarComprobantes.ps1`. El [índice de Database](../Database/README.md) detalla el orden completo y las opciones para cada estado de la base.

Los componentes, grupos sanguíneos y factores Rh se administran en **Maestros** y se comparten con PN1. El filtro de candidatos mantiene las reglas conocidas de PN2: un código nuevo necesita sus reglas en **Maestros → Compatibilidades** para generar candidatos. Ver [Compatibilidad configurable](Compatibilidad-configurable.md).

Los destinos de entrega también se administran en **Maestros**. El código de cada destino es el estado propio de la unidad; no hay un selector de resultado separado. Los estados estándar **TRANSFUNDIDA**, **DEVUELTA** y **DESCARTADA** mantienen sus reglas existentes. Un estado nuevo no cuenta automáticamente como transfusión ni habilita reingreso; puede exigir motivo según su configuración. Una baja oculta la opción para nuevas entregas y conserva su significado en el historial.

4. Compilar `IngDeSoftware.sln` y abrir la UI. Volver a iniciar sesión para cargar los permisos. El administrador existente recibe las operaciones de PN2; los demás usuarios usan las familias asignadas.

### Antecedente de instalación local

La versión anterior de PN2 se aplicó el 17 de octubre de 2026 a `HemovidaGest` en el servidor local `.`. Los cambios de estudios y compatibilidad descritos en [PN2-compatibilidad](PN2-compatibilidad.md) requieren una nueva migración; no se aplican automáticamente.

En esa instalación se generó el respaldo `HemovidaGest_AntesPN2_20261017_184644_18913565e0f147428488c69e24604568.bak` en la carpeta de backups de SQL Server y se verificó con `RESTORE VERIFYONLY` y checksum. Si la base conserva esa versión anterior, completar la actualización indicada antes de revisar el alcance actual, abrir la aplicación y volver a iniciar sesión.

## Recorrido de las pantallas operativas

1. Preparar en PN1 al menos tres unidades vigentes, clasificadas y liberadas con un componente reconocido y grupos candidatos para el estudio del receptor.
2. Crear una solicitud por tres unidades, cargar el estudio y comprobar **3 / 0 / 0 / 3**. Un operador autorizado valida el informe.
3. Confirmar la compatibilidad de dos candidatas, ingresar la referencia y reservarlas; comprobar **3 / 2 / 0 / 1**. Intentar reservar una de ellas nuevamente: debe rechazarse.
4. Entregar una reserva y comprobar **3 / 1 / 1 / 1**. No debe poder entregarse otra vez la misma reserva.
5. Devolverla con motivo: comprobar **3 / 1 / 0 / 2** y conservar la entrega en el historial.
6. Cancelar la reserva restante con motivo: comprobar **3 / 0 / 0 / 3**. La unidad queda Liberada.
7. Intentar reservar directamente la unidad Devuelta: debe rechazarse. La unidad se conserva en ese estado; este recorrido no requiere la operación de reingreso del servicio.
8. Seleccionar otra unidad liberada y vigente, reservarla, entregarla y registrar Transfundida. Comprobar el saldo desde la solicitud y conservar el comprobante de esa entrega.
9. En otra entrega registrar Descartada: debe reabrir saldo y dejar la unidad terminal, sin permitir reingreso.
10. Completar el pedido con reemplazos y comprobar ENTREGADA, saldo cero y rechazo de reservas adicionales.
11. Probar sin stock y con una unidad vencida o bloqueada: la solicitud se conserva pendiente y no se confirma una reserva inválida.
12. Exportar el reporte de la solicitud y comprobar el acceso a cada pantalla operativa con cuentas que tengan los permisos correspondientes.

### Comprobaciones realizadas

Las comprobaciones técnicas cubren SQL, concurrencia, vencimiento, migración, integración Application/UI y las pantallas operativas de PN2. El reporte único completó 22 comprobaciones de interfaz, 13 de integración SQL/UI y 22 de SQL, con salida `0`. También se verificó desde Visual Studio el ingreso real, la apertura del reporte y cinco cierres de sesión consecutivos; tras el sexto ingreso se cerró la aplicación mediante la X con el reporte abierto y salida `0`. La causa y la regresión se documentan en [Cierre de sesión y accesibilidad](Cierre-sesion-accesibilidad.md).

Los modelos finales G06 y G07 y las figuras del documento tienen sus verificaciones descritas en [Modelo de clases final](Modelo-clases-final.md) y [Modelo de datos final](Modelo-datos-final.md). Estas evidencias técnicas no constituyen aceptación formal del usuario ni aprobación docente.

## Referencias técnicas

Los diagramas de clases y secuencia de CDUN-05 a CDUN-08 representan el mismo recorte que las fichas funcionales. Las firmas y los procedimientos se detallan en esos diagramas; las fichas describen las acciones de las personas y las respuestas observables del sistema.

Cada caso mantiene su especificación, clases, secuencia, DER y pantalla de referencia. CDUN-06 tiene tres vistas DER complementarias: Estudio, Reserva y Entrega. La emisión automática y sus alternativas se describen en [Comprobantes de PN1 y PN2](Comprobantes-PN1-PN2.md).
