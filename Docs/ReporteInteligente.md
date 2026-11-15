# Abastecimiento y riesgo de vencimiento

El reporte relaciona el stock liberado disponible con las solicitudes transfusionales registradas y el saldo que todavía necesita cobertura. Ayuda a revisar el abastecimiento, detectar unidades próximas a vencer y distinguir la demanda que requiere validar el estudio del receptor.

Se accede desde **Consultas → Abastecimiento y riesgo de vencimiento**, con el permiso `REPORTE_VER`. Application y SQL verifican la autorización; la consulta también exige integridad válida.

## Alcance y filtros

El período **Desde–Hasta** comprende ambos días completos y se aplica a la demanda registrada. Hasta no puede ser posterior a hoy. El valor inicial comprende los últimos treinta días, incluido hoy. El horizonte de vencimiento admite entre 1 y 365 días y comienza en la fecha de corte indicada por SQL Server.

Los filtros de componente, grupo y Rh son exactos y opcionales. Al filtrar un grupo o Rh, las solicitudes sin estudio validado quedan fuera de ese filtro porque todavía no pertenecen a una cohorte validada. **Solo alertas** filtra las filas obtenidas y actualiza las tarjetas con las filas visibles.

El stock y el pendiente representan la situación actual; el período seleccionado no reconstruye inventarios históricos. El pendiente incluye solicitudes anteriores al período que todavía tienen saldo.

## Cohortes y métricas

Una cohorte reúne un **componente, grupo sanguíneo y factor Rh exactos**. Las solicitudes aportan demanda a esa cohorte cuando su estudio tiene usuario y fecha de validación, grupo y Rh. Las demás se agrupan por componente en una fila sin estudio validado, con stock, cobertura y déficit **N/A**.

| Métrica | Definición |
| --- | --- |
| Stock vigente | Cantidad de unidades `LIBERADA`, con componente, grupo y Rh, no vencidas y sin reserva activa. Cada unidad cuenta una vez. |
| Por vencer | Subconjunto de ese stock cuya fecha de vencimiento está entre hoy y hoy + horizonte, ambos incluidos. Una unidad que vence hoy todavía integra el stock vigente. |
| Demanda del período | Suma de las cantidades requeridas de las solicitudes registradas en el período. Cada solicitud suma una vez, aunque un receptor tenga pedidos repetidos. |
| Pendiente actual | Suma del saldo actual de las solicitudes registradas hasta hoy, incluidas las anteriores al período. Reutiliza `v_PN2_SolicitudCantidades`. |
| Unidades/día | Demanda del período ÷ cantidad de días inclusivos del período. |
| Cobertura en días | Stock vigente × días del período ÷ demanda del período. Es N/A si no hay demanda registrada o falta estudio validado. |
| Déficit por cohorte exacta | Máximo entre pendiente actual − stock vigente y cero. Es N/A sin estudio validado. |

El saldo de cada solicitud descuenta las reservas `ACTIVA` y las entregas cuyo destino está pendiente o es `TRANSFUNDIDA`. Las reservas canceladas no cubren saldo; las entregas devueltas o descartadas vuelven a dejar esa necesidad pendiente, según la regla vigente de PN2.

La cobertura es una relación con la demanda histórica registrada, no una predicción de necesidades clínicas. Los cálculos usan los valores sin el redondeo visual de la grilla. El reporte no asigna unidades ni calcula sustituciones compatibles entre grupos o Rh.

## Prioridades e interpretación

| Prioridad | Condición observable |
| --- | --- |
| Reposición y vencimiento | Déficit actual mayor que cero y stock por vencer mayor que cero. |
| Revisar reposición | Déficit actual mayor que cero. |
| Revisar vencimientos | Stock por vencer mayor que cero, sin déficit actual. |
| Validar estudio | Solicitudes que todavía no tienen estudio validado; sus métricas por grupo/Rh son N/A. |
| Sin alerta actual | Sin déficit actual ni stock por vencer dentro del horizonte. |

Las tarjetas suman el stock vigente, el stock por vencer y los déficits de las cohortes visibles; la cuarta muestra unidades pendientes sin estudio validado. El déficit total se obtiene sumando los déficits de cada cohorte exacta, sin compensarlos con excedentes de otro grupo o Rh.

La pantalla presenta una única grilla con todas las métricas por cohorte y cuatro tarjetas de resumen. Para interpretar una fila, seleccionarla y consultar el texto inferior. Ante fechas inválidas o fallas de consulta se vacían las cifras anteriores y se muestra el error; cambiar el idioma conserva ese estado hasta una consulta válida.

## Casos de demostración

La captura de la pantalla usa datos ficticios y lo indica en una franja visible. La evidencia de integración utiliza el servicio, repositorio, DAL y procedimiento reales contra una base temporal creada desde cero.

- **Déficit junto a vencimientos:** en la cohorte de eritrocitos A+, hay 3 unidades vigentes, 2 próximas a vencer y 8 pendientes actuales. El déficit exacto es 5. Con 10 unidades solicitadas en treinta días, la cobertura histórica es 9 días. Corresponde revisar conjuntamente abastecimiento y vencimientos.
- **Falta de estudio validado:** 5 unidades pendientes aparecen en la fila del componente sin grupo/Rh validado. La acción informada es validar el estudio; el reporte conserva stock, cobertura y déficit como N/A.
- **Sin demanda reciente:** una cohorte con stock pero sin solicitudes en el período muestra cobertura N/A, sin inventar una duración ilimitada. Puede igualmente presentar una alerta de vencimiento.
- **Receptor recurrente:** dos solicitudes del mismo documento se cuentan como dos pedidos independientes, sin multiplicar sus cantidades por las reservas o entregas relacionadas.

## Implementación y comprobación

- [DTO de filtros](../Domain/PN2/AbastecimientoFiltro.cs) y [fila del reporte](../Domain/PN2/FilaAbastecimiento.cs).
- [Servicio de aplicación](../Application/PN2/PN2ApplicationService.cs), [repositorio](../Repository/PN2/PN2Repository.cs) y [DataMapper](../DAL/PN2/PN2DataMapper.cs): método `ListarAbastecimiento_380_jh`.
- [Consulta agregada](../Database/StoredProcedures/15_AbastecimientoRiesgo.sql): CTE de stock, solicitudes, cohortes y métricas, con parámetros, autorización e integridad.
- [Textos en español e inglés](../Database/StoredProcedures/15a_AbastecimientoUI.sql): inserción de claves faltantes que conserva personalizaciones existentes.
- [Pantalla de abastecimiento](../UI/Views/PN2/AbastecimientoView.cs).
- [Pruebas SQL](../Tests/Abastecimiento.Tests.ps1) y [runner de interfaz e integración](../Tests/Abastecimiento.UI.Tests.ps1): base temporal aislada, métricas, fechas, permisos, integridad, traducciones, filtros, menú y captura mediante `DrawToBitmap`. La regresión de interfaz abre la vista con handles reales, selecciona las cohortes, cambia el idioma, cierra y vuelve a abrir tres veces; también comprueba que las vistas cerradas dejen de recibir notificaciones de idioma.

Las pruebas construyen explícitamente una conexión de seguridad integrada y no leen la configuración privada de la aplicación. Las bases y carpetas temporales se eliminan después de ejecutarse.
