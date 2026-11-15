-- Traducciones incrementales del reporte de abastecimiento. Conserva textos personalizados.
USE [HemovidaGest];
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;
INSERT dbo.Idioma(codigo,nombre)
SELECT codigo,nombre FROM (VALUES('es-AR',N'Español'),('en-US',N'English')) i(codigo,nombre)
WHERE NOT EXISTS(SELECT 1 FROM dbo.Idioma d WHERE d.codigo=i.codigo);
DECLARE @textos TABLE(clave VARCHAR(150),es NVARCHAR(500),en NVARCHAR(500));
INSERT @textos VALUES
(N'SUPPLY_TITLE',N'Abastecimiento y riesgo de vencimiento',N'Supply and expiry risk'),
(N'SUPPLY_DESCRIPTION',N'Stock actual y demanda registrada por componente, grupo y Rh exactos. La cobertura usa el promedio histórico; no asigna unidades.',N'Current stock and recorded demand by exact component, blood group and Rh. Coverage uses historical averages; it does not allocate units.'),
(N'SUPPLY_FROM',N'Desde',N'From'),
(N'SUPPLY_TO',N'Hasta',N'To'),
(N'SUPPLY_HORIZON',N'Días a vencer',N'Expiry days'),
(N'SUPPLY_COMPONENT',N'Componente',N'Component'),
(N'SUPPLY_GROUP',N'Grupo',N'Group'),
(N'SUPPLY_RH',N'Rh',N'Rh'),
(N'SUPPLY_GENERATE',N'Consultar',N'Refresh'),
(N'SUPPLY_ALERTS_ONLY',N'Solo alertas',N'Alerts only'),
(N'SUPPLY_KPI_STOCK',N'Stock liberado vigente',N'Current released stock'),
(N'SUPPLY_KPI_EXPIRY',N'Stock por vencer',N'Stock nearing expiry'),
(N'SUPPLY_KPI_DEFICIT',N'Déficit por cohorte exacta',N'Exact-cohort shortage'),
(N'SUPPLY_KPI_STUDY',N'Pendiente sin estudio validado',N'Pending unvalidated study'),
(N'SUPPLY_TAB_DETAIL',N'Detalle por cohorte',N'Cohort detail'),
(N'SUPPLY_TAB_CHART',N'Comparación por componente',N'Component comparison'),
(N'SUPPLY_COL_STOCK',N'Stock vigente',N'Current stock'),
(N'SUPPLY_COL_EXPIRY',N'Por vencer',N'Expiring'),
(N'SUPPLY_COL_DEMAND',N'Demanda período',N'Period demand'),
(N'SUPPLY_COL_RATE',N'Unidades / día',N'Units / day'),
(N'SUPPLY_COL_PENDING',N'Pendiente actual',N'Current pending'),
(N'SUPPLY_COL_COVERAGE',N'Cobertura (días)',N'Coverage (days)'),
(N'SUPPLY_COL_DEFICIT',N'Déficit',N'Shortage'),
(N'SUPPLY_COL_PRIORITY',N'Prioridad',N'Priority'),
(N'SUPPLY_NA',N'N/A',N'N/A'),
(N'SUPPLY_INVALID_PERIOD',N'La fecha desde debe ser anterior o igual a la fecha hasta.',N'The from date must be earlier than or equal to the to date.'),
(N'SUPPLY_QUERY_ERROR',N'No se pudo consultar el reporte. Se borraron los resultados anteriores; revisá permisos, integridad y conexión.',N'The report could not be loaded. Previous results were cleared; check permissions, integrity and connection.'),
(N'SUPPLY_STATUS',N'{0} cohortes · Corte: {1:d} · Solicitudes: {2:d}–{3:d} · Vencimientos: hoy a +{4} días',N'{0} cohorts · As of: {1:d} · Requests: {2:d}–{3:d} · Expiry: today through +{4} days'),
(N'SUPPLY_COVERAGE_HELP',N'Stock actual dividido por el promedio de unidades solicitadas por día en el período. Es una relación histórica de la cohorte exacta; no predice necesidades ni considera sustituciones compatibles.',N'Current stock divided by average units requested per day in the period. This is a historical exact-cohort ratio, not a forecast or an assessment of compatible substitutions.'),
(N'SUPPLY_STUDY_HELP',N'Sin estudio validado, no se calculan stock por grupo y Rh, cobertura ni déficit.',N'Without a validated study, stock by group and Rh, coverage and shortage are not calculated.'),
(N'SUPPLY_EMPTY',N'No hay cohortes para los filtros aplicados.',N'No cohorts match the selected filters.'),
(N'SUPPLY_SELECT_ROW',N'Seleccioná una cohorte para ver su interpretación.',N'Select a cohort to view its interpretation.'),
(N'SUPPLY_STUDY_DETAIL',N'{0:N0} unidades pendientes de solicitudes sin estudio validado. Completar y validar el estudio antes de comparar por grupo y Rh.',N'{0:N0} pending units belong to requests without a validated study. Complete and validate the study before comparing by group and Rh.'),
(N'SUPPLY_REPLENISH_DETAIL',N'{0:N0} unidades de déficit en esta cohorte exacta. Revisar el abastecimiento.',N'This exact cohort has a shortage of {0:N0} units. Review supply.'),
(N'SUPPLY_BALANCED_DETAIL',N'El stock de esta cohorte alcanza para su pendiente actual.',N'This cohort has enough stock for its current pending demand.'),
(N'SUPPLY_EXPIRY_DETAIL',N'{0:N0} unidades disponibles vencen dentro del horizonte; revisar su utilización y vencimiento.',N'{0:N0} available units expire within the horizon; review their use and expiry.'),
(N'SUPPLY_EXACT_COHORT_HELP',N'No incluye sustituciones entre grupos o Rh.',N'Compatible substitutions between groups or Rh are not included.'),
(N'SUPPLY_CHART_STOCK',N'Stock vigente',N'Current stock'),
(N'SUPPLY_CHART_PENDING',N'Pendiente validado',N'Validated pending'),
(N'SUPPLY_CHART_HELP',N'Hasta 6 componentes · Solo cohortes validadas · Sin asignar compatibilidad.',N'Up to 6 components · Validated cohorts only · No compatibility allocation.'),
(N'SUPPLY_PRIORITY_VALIDAR_ESTUDIO',N'Validar estudio',N'Validate study'),
(N'SUPPLY_PRIORITY_REPOSICION_Y_VENCIMIENTO',N'Reposición y vencimiento',N'Replenishment and expiry'),
(N'SUPPLY_PRIORITY_REPOSICION',N'Revisar reposición',N'Review replenishment'),
(N'SUPPLY_PRIORITY_VENCIMIENTO',N'Revisar vencimientos',N'Review expiry'),
(N'SUPPLY_PRIORITY_SIN_ALERTA',N'Sin alerta actual',N'No current alert');
INSERT dbo.Etiqueta(clave,descripcion)
SELECT clave,CONVERT(VARCHAR(255),es) FROM @textos t
WHERE NOT EXISTS(SELECT 1 FROM dbo.Etiqueta e WHERE e.clave=t.clave);
INSERT dbo.Traduccion(id_etiqueta,id_idioma,texto)
SELECT e.id_etiqueta,i.id_idioma,CASE i.codigo WHEN 'es-AR' THEN t.es ELSE t.en END
FROM @textos t JOIN dbo.Etiqueta e ON e.clave=t.clave
JOIN dbo.Idioma i ON i.codigo IN ('es-AR','en-US')
WHERE NOT EXISTS(SELECT 1 FROM dbo.Traduccion d WHERE d.id_etiqueta=e.id_etiqueta AND d.id_idioma=i.id_idioma);
COMMIT TRANSACTION;
GO
