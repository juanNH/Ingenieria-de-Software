-- PN2: permisos y traducciones incrementales. Conserva personalizaciones existentes.
USE [HemovidaGest];
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @permisos TABLE(codigo VARCHAR(100),nombre VARCHAR(100),descripcion VARCHAR(255),accion VARCHAR(100));
INSERT @permisos VALUES
(N'INVENTARIO_VER',N'Ver inventario',N'Consultar unidades y ubicaciones de PN2',N'VER_INVENTARIO'),
(N'INVENTARIO_ACTUALIZAR',N'Actualizar ubicación',N'Registrar cambios de ubicación de unidades',N'ACTUALIZAR_INVENTARIO'),
(N'SOLICITUD_VER',N'Ver solicitudes transfusionales',N'Consultar solicitudes según el rol',N'VER_SOLICITUD'),
(N'SOLICITUD_CREAR',N'Registrar solicitud transfusional',N'Registrar la necesidad del paciente receptor',N'CREAR_SOLICITUD'),
(N'RESERVA_VER',N'Ver reservas y entregas',N'Consultar reservas y entregas de PN2',N'VER_RESERVA'),
(N'RESERVA_CREAR',N'Reservar unidad',N'Asignar una unidad disponible a una solicitud',N'CREAR_RESERVA'),
(N'RESERVA_CANCELAR',N'Cancelar reserva',N'Cancelar una reserva activa con motivo',N'CANCELAR_RESERVA'),
(N'ENTREGA_REGISTRAR',N'Registrar entrega',N'Confirmar la salida de una unidad reservada',N'REGISTRAR_ENTREGA'),
(N'DESTINO_REGISTRAR',N'Registrar destino de unidad',N'Registrar transfusión, devolución o descarte',N'REGISTRAR_DESTINO'),
(N'UNIDAD_REINGRESAR',N'Reingresar unidad devuelta',N'Enviar una unidad devuelta a nueva revisión de PN1',N'REINGRESAR_UNIDAD'),
(N'TRAZABILIDAD_VER',N'Consultar trazabilidad',N'Consultar el historial relacionado de una unidad',N'VER_TRAZABILIDAD'),
(N'REPORTE_VER',N'Ver reportes y alertas',N'Consultar resúmenes y alertas operativas de PN2',N'VER_REPORTE');
INSERT dbo.Permiso(codigo,nombre,descripcion,modulo,accion,estado_permiso)
SELECT codigo,nombre,descripcion,'PN2',accion,'ACTIVO' FROM @permisos p
WHERE NOT EXISTS(SELECT 1 FROM dbo.Permiso d WHERE d.codigo=p.codigo);
INSERT dbo.ComponentePermiso(codigo,nombre,descripcion,tipo,estado_componente)
SELECT codigo,nombre,descripcion,'PERMISO','ACTIVO' FROM @permisos p
WHERE NOT EXISTS(SELECT 1 FROM dbo.ComponentePermiso d WHERE d.codigo=p.codigo);
DECLARE @familias TABLE(codigo VARCHAR(100),nombre VARCHAR(100),descripcion VARCHAR(255));
INSERT @familias VALUES
(N'PN2_OPERADOR_BANCO',N'Operador del banco de sangre',N'Inventario, atención de solicitudes y destino de unidades'),
(N'PN2_PROFESIONAL_SOLICITANTE',N'Profesional solicitante',N'Registro y consulta de sus solicitudes transfusionales'),
(N'PN2_AUDITOR',N'Auditor',N'Consulta de trazabilidad de las unidades');
INSERT dbo.ComponentePermiso(codigo,nombre,descripcion,tipo,estado_componente)
SELECT codigo,nombre,descripcion,'FAMILIA','ACTIVO' FROM @familias f
WHERE NOT EXISTS(SELECT 1 FROM dbo.ComponentePermiso d WHERE d.codigo=f.codigo);
DECLARE @relaciones TABLE(padre VARCHAR(100),hijo VARCHAR(100));
INSERT @relaciones VALUES
(N'PN2_OPERADOR_BANCO',N'INVENTARIO_VER'),
(N'PN2_OPERADOR_BANCO',N'INVENTARIO_ACTUALIZAR'),
(N'PN2_OPERADOR_BANCO',N'SOLICITUD_VER'),
(N'PN2_OPERADOR_BANCO',N'RESERVA_VER'),
(N'PN2_OPERADOR_BANCO',N'DESTINO_REGISTRAR'),
(N'PN2_OPERADOR_BANCO',N'REPORTE_VER'),
(N'PN2_PROFESIONAL_SOLICITANTE',N'SOLICITUD_VER'),
(N'PN2_PROFESIONAL_SOLICITANTE',N'SOLICITUD_CREAR'),
(N'PN1_OPERADOR_AUTORIZADO',N'INVENTARIO_VER'),
(N'PN1_OPERADOR_AUTORIZADO',N'SOLICITUD_VER'),
(N'PN1_OPERADOR_AUTORIZADO',N'RESERVA_VER'),
(N'PN1_OPERADOR_AUTORIZADO',N'RESERVA_CREAR'),
(N'PN1_OPERADOR_AUTORIZADO',N'RESERVA_CANCELAR'),
(N'PN1_OPERADOR_AUTORIZADO',N'ENTREGA_REGISTRAR'),
(N'PN2_AUDITOR',N'TRAZABILIDAD_VER'),
(N'PN1_RESPONSABLE_AUTORIZADO',N'INVENTARIO_VER'),
(N'PN1_RESPONSABLE_AUTORIZADO',N'UNIDAD_REINGRESAR'),
(N'PN1_RESPONSABLE_AUTORIZADO',N'REPORTE_VER'),
(N'ADMINISTRADOR',N'INVENTARIO_VER'),
(N'ADMINISTRADOR',N'INVENTARIO_ACTUALIZAR'),
(N'ADMINISTRADOR',N'SOLICITUD_VER'),
(N'ADMINISTRADOR',N'SOLICITUD_CREAR'),
(N'ADMINISTRADOR',N'RESERVA_VER'),
(N'ADMINISTRADOR',N'RESERVA_CREAR'),
(N'ADMINISTRADOR',N'RESERVA_CANCELAR'),
(N'ADMINISTRADOR',N'ENTREGA_REGISTRAR'),
(N'ADMINISTRADOR',N'DESTINO_REGISTRAR'),
(N'ADMINISTRADOR',N'UNIDAD_REINGRESAR'),
(N'ADMINISTRADOR',N'TRAZABILIDAD_VER'),
(N'ADMINISTRADOR',N'REPORTE_VER');
INSERT dbo.ComponentePermisoRelacion(id_padre,id_hijo)
SELECT p.id_componente,h.id_componente FROM @relaciones r
JOIN dbo.ComponentePermiso p ON p.codigo=r.padre
JOIN dbo.ComponentePermiso h ON h.codigo=r.hijo
WHERE NOT EXISTS(SELECT 1 FROM dbo.ComponentePermisoRelacion d
 WHERE d.id_padre=p.id_componente AND d.id_hijo=h.id_componente);
COMMIT TRANSACTION;
GO
-- Las traducciones de las vistas PN2 se agregan abajo sin reemplazar las personalizadas.

-- PN2_TRADUCCIONES_GENERADAS
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @textos TABLE(clave VARCHAR(150),descripcion VARCHAR(255),es NVARCHAR(500),en NVARCHAR(500));
INSERT @textos VALUES
(N'MENU_INVENTORY',N'Inventario',N'Inventario',N'Inventory'),
(N'MENU_TRANSFUSION_REQUESTS',N'Solicitudes transfusionales',N'Solicitudes transfusionales',N'Transfusion requests'),
(N'MENU_RESERVATIONS',N'Reservas y entregas',N'Reservas y entregas',N'Reservations and deliveries'),
(N'MENU_UNIT_DESTINATION',N'Destino de unidades',N'Destino de unidades',N'Unit destinations'),
(N'MENU_TRACEABILITY',N'Trazabilidad',N'Trazabilidad',N'Traceability'),
(N'MENU_REPORTS_ALERTS',N'Reportes y alertas',N'Reportes y alertas',N'Reports and alerts'),
(N'PN2_INVENTORY_TITLE',N'Inventario de unidades',N'Inventario de unidades',N'Unit inventory'),
(N'PN2_INVENTORY_DESCRIPTION',N'Consultá el estado y la ubicación de las unidades del banco de sangre.',N'Consultá el estado y la ubicación de las unidades del banco de sangre.',N'Review the status and location of blood bank units.'),
(N'PN2_INVENTORY_UPDATE',N'Unidad seleccionada',N'Unidad seleccionada',N'Selected unit'),
(N'PN2_INVENTORY_NOTE',N'El reingreso de una unidad devuelta requiere motivo y vuelve a En revisión; luego debe clasificarse y liberarse en Unidades.',N'El reingreso de una unidad devuelta requiere motivo y vuelve a En revisión; luego debe clasificarse y liberarse en Unidades.',N'Reentering a returned unit requires a reason and returns it to Under review; it must then be classified and released in Units.'),
(N'PN2_LOCATION',N'Ubicación',N'Ubicación',N'Location'),
(N'PN2_UPDATE_LOCATION',N'Actualizar ubicación',N'Actualizar ubicación',N'Update location'),
(N'PN2_REENTER_UNIT',N'Reingresar unidad devuelta',N'Reingresar unidad devuelta',N'Reenter returned unit'),
(N'PN2_REQUESTS_TITLE',N'Solicitudes transfusionales',N'Solicitudes transfusionales',N'Transfusion requests'),
(N'PN2_REQUESTS_DESCRIPTION',N'El Profesional solicitante registra el pedido. Puede conservarse pendiente aunque no haya unidades disponibles.',N'El Profesional solicitante registra el pedido. Puede conservarse pendiente aunque no haya unidades disponibles.',N'The requesting professional registers the request. It may remain pending when no units are available.'),
(N'PN2_REQUEST_ID',N'Solicitud',N'Solicitud',N'Request'),
(N'PN2_PATIENT',N'Paciente receptor',N'Paciente receptor',N'Recipient patient'),
(N'PN2_REQUESTING_PROFESSIONAL',N'Profesional solicitante',N'Profesional solicitante',N'Requesting professional'),
(N'PN2_REQUEST_DATE',N'Fecha de solicitud',N'Fecha de solicitud',N'Request date'),
(N'PN2_REQUIRED',N'Requeridas',N'Requeridas',N'Required'),
(N'PN2_RESERVED',N'Reservadas',N'Reservadas',N'Reserved'),
(N'PN2_DELIVERED',N'Entregadas vigentes',N'Entregadas vigentes',N'Valid deliveries'),
(N'PN2_PENDING',N'Saldo pendiente',N'Saldo pendiente',N'Pending balance'),
(N'PN2_NEW_REQUEST',N'Registrar solicitud transfusional',N'Registrar solicitud transfusional',N'Register transfusion request'),
(N'PN2_QUANTITY_NOTE',N'Se permite atención parcial. Las devoluciones y los descartes reabren el saldo; el historial conserva todas las entregas.',N'Se permite atención parcial. Las devoluciones y los descartes reabren el saldo; el historial conserva todas las entregas.',N'Partial fulfillment is allowed. Returns and discards reopen the balance; all deliveries remain in history.'),
(N'PN2_RESERVATIONS_TITLE',N'Reservas y entregas',N'Reservas y entregas',N'Reservations and deliveries'),
(N'PN2_RESERVATIONS_DESCRIPTION',N'Elegí una solicitud y una unidad para reservar. Seleccioná una reserva activa para entregarla o cancelarla con motivo.',N'Elegí una solicitud y una unidad para reservar. Seleccioná una reserva activa para entregarla o cancelarla con motivo.',N'Choose a request and a unit to reserve. Select an active reservation to deliver it or cancel it with a reason.'),
(N'PN2_AVAILABLE_UNITS',N'Unidades liberadas y vigentes del componente solicitado',N'Unidades liberadas y vigentes del componente solicitado',N'Released, unexpired units of the requested component'),
(N'PN2_RESERVATION_ID',N'Reserva',N'Reserva',N'Reservation'),
(N'PN2_RESERVATION_DATE',N'Fecha de reserva',N'Fecha de reserva',N'Reservation date'),
(N'PN2_RESPONSIBLE',N'Responsable',N'Responsable',N'Responsible user'),
(N'PN2_CANCELLATION_REASON',N'Motivo de cancelación',N'Motivo de cancelación',N'Cancellation reason'),
(N'PN2_RESERVATION_ACTIONS',N'Atender solicitud',N'Atender solicitud',N'Fulfill request'),
(N'PN2_REASON_OBSERVATION',N'Motivo / observación',N'Motivo / observación',N'Reason / note'),
(N'PN2_REQUEST_COUNTS',N'Requeridas: {0} · Reservadas: {1} · Entregadas vigentes: {2} · Saldo pendiente: {3}',N'Requeridas: {0} · Reservadas: {1} · Entregadas vigentes: {2} · Saldo pendiente: {3}',N'Required: {0} · Reserved: {1} · Valid deliveries: {2} · Pending balance: {3}'),
(N'PN2_RESERVE',N'Reservar unidad seleccionada',N'Reservar unidad seleccionada',N'Reserve selected unit'),
(N'PN2_DELIVER',N'Entregar reserva seleccionada',N'Entregar reserva seleccionada',N'Deliver selected reservation'),
(N'PN2_CANCEL_RESERVATION',N'Cancelar reserva seleccionada',N'Cancelar reserva seleccionada',N'Cancel selected reservation'),
(N'PN2_DESTINATIONS_TITLE',N'Destino de unidades entregadas',N'Destino de unidades entregadas',N'Delivered unit destinations'),
(N'PN2_DESTINATIONS_DESCRIPTION',N'Registrá la transfusión, devolución o descarte de una entrega. El listado conserva también los destinos ya registrados.',N'Registrá la transfusión, devolución o descarte de una entrega. El listado conserva también los destinos ya registrados.',N'Record a delivery''s transfusion, return or discard. The list also preserves previously recorded destinations.'),
(N'PN2_DELIVERY_ID',N'Entrega',N'Entrega',N'Delivery'),
(N'PN2_DELIVERY_DATE',N'Fecha de entrega',N'Fecha de entrega',N'Delivery date'),
(N'PN2_DESTINATION',N'Destino',N'Destino',N'Destination'),
(N'PN2_DESTINATION_DATE',N'Fecha de destino',N'Fecha de destino',N'Destination date'),
(N'PN2_REASON',N'Motivo',N'Motivo',N'Reason'),
(N'PN2_REGISTER_DESTINATION',N'Registrar destino',N'Registrar destino',N'Record destination'),
(N'PN2_DESTINATION_NOTE',N'Devolución y descarte requieren motivo y reabren el saldo. La devolución requiere reingreso autorizado antes de reutilizar la unidad.',N'Devolución y descarte requieren motivo y reabren el saldo. La devolución requiere reingreso autorizado antes de reutilizar la unidad.',N'Returns and discards require a reason and reopen the balance. Returned units require authorized reentry before reuse.'),
(N'PN2_TRACE_TITLE',N'Trazabilidad de unidades',N'Trazabilidad de unidades',N'Unit traceability'),
(N'PN2_TRACE_DESCRIPTION',N'Consultá movimientos y vínculos con solicitud, reserva y entrega. Las fechas opcionales filtran la fecha del movimiento.',N'Consultá movimientos y vínculos con solicitud, reserva y entrega. Las fechas opcionales filtran la fecha del movimiento.',N'Review movements and their request, reservation and delivery links. Optional dates filter the movement date.'),
(N'PN2_UNIT_FILTER',N'ID unidad (0: todas)',N'ID unidad (0: todas)',N'Unit ID (0: all)'),
(N'PN2_UNIT_ID',N'ID unidad',N'ID unidad',N'Unit ID'),
(N'PN2_MOVEMENT_DATE',N'Fecha del movimiento',N'Fecha del movimiento',N'Movement date'),
(N'PN2_MOVEMENT',N'Movimiento',N'Movimiento',N'Movement'),
(N'PN2_PREVIOUS_STATE',N'Estado anterior',N'Estado anterior',N'Previous status'),
(N'PN2_NEW_STATE',N'Estado nuevo',N'Estado nuevo',N'New status'),
(N'PN2_REPORTS_TITLE',N'Reportes y alertas',N'Reportes y alertas',N'Reports and alerts'),
(N'PN2_REPORTS_DESCRIPTION',N'Stock y alertas muestran la situación actual. Las fechas filtran solicitudes y movimientos; no reconstruyen el inventario pasado.',N'Stock y alertas muestran la situación actual. Las fechas filtran solicitudes y movimientos; no reconstruyen el inventario pasado.',N'Stock and alerts show the current situation. Dates filter requests and movements; they do not reconstruct past inventory.'),
(N'PN2_GENERATE_REPORT',N'Consultar reporte',N'Consultar reporte',N'View report'),
(N'PN2_REPORT_SECTION',N'Sección',N'Sección',N'Section'),
(N'PN2_REPORT_CONCEPT',N'Concepto',N'Concepto',N'Concept'),
(N'PN2_REPORT_QUANTITY',N'Cantidad',N'Cantidad',N'Quantity'),
(N'PN2_REPORT_DETAIL',N'Detalle',N'Detalle',N'Details'),
(N'PN2_REPORT_INVENTARIO',N'Inventario',N'Inventario',N'Inventory'),
(N'PN2_REPORT_SOLICITUDES',N'Solicitudes',N'Solicitudes',N'Requests'),
(N'PN2_REPORT_RESERVAS',N'Reservas',N'Reservas',N'Reservations'),
(N'PN2_REPORT_MOVIMIENTOS',N'Movimientos',N'Movimientos',N'Movements'),
(N'PN2_REPORT_ALERTAS',N'Alertas',N'Alertas',N'Alerts'),
(N'PN2_REPORT_SALDO_PENDIENTE',N'Saldo pendiente',N'Saldo pendiente',N'Pending balance'),
(N'PN2_REPORT_ACTIVAS',N'Activas',N'Activas',N'Active'),
(N'PN2_REPORT_UNIDADES_VENCIDAS',N'Unidades vencidas',N'Unidades vencidas',N'Expired units'),
(N'PN2_REPORT_RESERVAS_INHABILITADAS',N'Reservas inhabilitadas',N'Reservas inhabilitadas',N'Invalid reservations'),
(N'PN2_REPORT_SOLICITUDES_SIN_DISPONIBILIDAD',N'Solicitudes sin disponibilidad',N'Solicitudes sin disponibilidad',N'Requests without available units'),
(N'PN2_UNCLASSIFIED',N'(sin clasificar)',N'(sin clasificar)',N'(unclassified)'),
(N'PN2_REPORT_CURRENT_INVENTORY_NOTE',N'Inventario actual; el período no reconstruye stock histórico.',N'Inventario actual; el período no reconstruye stock histórico.',N'Current inventory; the period does not reconstruct historical stock.'),
(N'PN2_REPORT_REQUESTS_NOTE',N'Solicitudes registradas en el período seleccionado.',N'Solicitudes registradas en el período seleccionado.',N'Requests recorded in the selected period.'),
(N'PN2_REPORT_BALANCE_NOTE',N'Unidades aún no cubiertas por reservas activas o entregas efectivas.',N'Unidades aún no cubiertas por reservas activas o entregas efectivas.',N'Units not yet covered by active reservations or valid deliveries.'),
(N'PN2_REPORT_RESERVATIONS_NOTE',N'Reservas vigentes registradas en el período seleccionado.',N'Reservas vigentes registradas en el período seleccionado.',N'Active reservations recorded in the selected period.'),
(N'PN2_REPORT_MOVEMENTS_NOTE',N'Movimientos registrados en el período; filtro de estado según resultado del movimiento.',N'Movimientos registrados en el período; filtro de estado según resultado del movimiento.',N'Movements recorded in the period; status filter uses the movement''s resulting status.'),
(N'PN2_REPORT_EXPIRED_NOTE',N'Unidades vencidas cuyo recorrido aún no terminó en transfusión o descarte.',N'Unidades vencidas cuyo recorrido aún no terminó en transfusión o descarte.',N'Expired units whose process has not ended in transfusion or discard.'),
(N'PN2_REPORT_INVALID_RESERVATIONS_NOTE',N'Reservas activas con unidad vencida, sin clasificación o con estado/componente incompatible.',N'Reservas activas con unidad vencida, sin clasificación o con estado/componente incompatible.',N'Active reservations with an expired, unclassified unit or an incompatible status/component.'),
(N'PN2_REPORT_UNAVAILABLE_NOTE',N'Solicitudes con saldo y sin unidad liberada, clasificada y vigente del componente solicitado.',N'Solicitudes con saldo y sin unidad liberada, clasificada y vigente del componente solicitado.',N'Requests with pending balance and no released, classified, unexpired unit of the requested component.'),
(N'PN2_SEARCH',N'Buscar',N'Buscar',N'Search'),
(N'PN2_RECORD_COUNT',N'{0} registros',N'{0} registros',N'{0} records'),
(N'PN2_SELECT_UNIT',N'Seleccioná una unidad.',N'Seleccioná una unidad.',N'Select a unit.'),
(N'PN2_SELECT_REQUEST',N'Seleccioná una solicitud.',N'Seleccioná una solicitud.',N'Select a request.'),
(N'PN2_SELECT_REQUEST_UNIT',N'Seleccioná una solicitud y una unidad disponible.',N'Seleccioná una solicitud y una unidad disponible.',N'Select a request and an available unit.'),
(N'PN2_SELECT_RESERVATION',N'Seleccioná una reserva activa.',N'Seleccioná una reserva activa.',N'Select an active reservation.'),
(N'PN2_SELECT_DELIVERY',N'Seleccioná una entrega sin destino registrado.',N'Seleccioná una entrega sin destino registrado.',N'Select a delivery without a recorded destination.'),
(N'PN2_REASON_REQUIRED',N'Ingresá el motivo u observación de la operación.',N'Ingresá el motivo u observación de la operación.',N'Enter a reason or note for the operation.'),
(N'PN2_CONFIRM_DELIVERY',N'¿Confirmás la entrega de la unidad {0} a la solicitud seleccionada?',N'¿Confirmás la entrega de la unidad {0} a la solicitud seleccionada?',N'Confirm delivery of unit {0} for the selected request?'),
(N'PN2_CONFIRM_CANCEL',N'¿Cancelás la reserva de la unidad {0}? La solicitud recuperará su saldo pendiente.',N'¿Cancelás la reserva de la unidad {0}? La solicitud recuperará su saldo pendiente.',N'Cancel the reservation of unit {0}? Its quantity will return to the request''s pending balance.'),
(N'PN2_CONFIRM_DESTINATION',N'¿Confirmás el destino {0}? Se conservará en el historial de la entrega.',N'¿Confirmás el destino {0}? Se conservará en el historial de la entrega.',N'Confirm destination {0}? It will be preserved in the delivery history.'),
(N'PN2_CONFIRM_REENTER',N'¿Reingresás la unidad devuelta {0} a En revisión? Deberá clasificarse y liberarse antes de una nueva reserva.',N'¿Reingresás la unidad devuelta {0} a En revisión? Deberá clasificarse y liberarse antes de una nueva reserva.',N'Reenter returned unit {0} as Under review? It must be classified and released before a new reservation.'),
(N'PN2_SAVED',N'Operación registrada correctamente.',N'Operación registrada correctamente.',N'Operation recorded successfully.'),
(N'PN2_INVALID',N'Revisá los datos obligatorios, la cantidad y el motivo cuando corresponda.',N'Revisá los datos obligatorios, la cantidad y el motivo cuando corresponda.',N'Check required fields, quantity and the reason when applicable.'),
(N'PN2_NOT_FOUND',N'No se encontró el registro. Actualizá el listado antes de reintentar.',N'No se encontró el registro. Actualizá el listado antes de reintentar.',N'The record was not found. Refresh the list before retrying.'),
(N'PN2_CONFLICT',N'El estado o los vínculos cambiaron y no permiten esta operación. Actualizá el listado.',N'El estado o los vínculos cambiaron y no permiten esta operación. Actualizá el listado.',N'The status or links changed and do not allow this operation. Refresh the list.'),
(N'PN2_UNAVAILABLE',N'La unidad no está disponible, vigente o habilitada para esta operación.',N'La unidad no está disponible, vigente o habilitada para esta operación.',N'The unit is unavailable, expired or not enabled for this operation.'),
(N'PN2_QUANTITY_COMPLETE',N'La cantidad requerida ya está cubierta por reservas o entregas vigentes.',N'La cantidad requerida ya está cubierta por reservas o entregas vigentes.',N'The required quantity is already covered by reservations or valid deliveries.'),
(N'PN2_SAVE_ERROR',N'No se confirmó la operación. Actualizá el listado y revisá su estado antes de reintentar.',N'No se confirmó la operación. Actualizá el listado y revisá su estado antes de reintentar.',N'The operation was not confirmed. Refresh the list and check its status before retrying.'),
(N'PN2_QUERY_ERROR',N'No se pudo actualizar la consulta. Los datos visibles pueden corresponder a una consulta anterior. Reintentá la actualización.',N'No se pudo actualizar la consulta. Los datos visibles pueden corresponder a una consulta anterior. Reintentá la actualización.',N'The query could not be refreshed. Visible data may be from a previous query. Try refreshing again.'),
(N'PN2_READ_FAILED',N'No se pudo completar la consulta.',N'No se pudo completar la consulta.',N'The query could not be completed.'),
(N'PN2_INVALID_FILTER',N'La fecha Desde debe ser anterior o igual a la fecha Hasta.',N'La fecha Desde debe ser anterior o igual a la fecha Hasta.',N'The From date must be earlier than or equal to the To date.'),
(N'PN2_STATE_EN_REVISION',N'En revisión',N'En revisión',N'Under review'),
(N'PN2_STATE_LIBERADA',N'Liberada',N'Liberada',N'Released'),
(N'PN2_STATE_BLOQUEADA',N'Bloqueada',N'Bloqueada',N'Blocked'),
(N'PN2_STATE_RESERVADA',N'Reservada',N'Reservada',N'Reserved'),
(N'PN2_STATE_ENTREGADA',N'Entregada',N'Entregada',N'Delivered'),
(N'PN2_STATE_TRANSFUNDIDA',N'Transfundida',N'Transfundida',N'Transfused'),
(N'PN2_STATE_DEVUELTA',N'Devuelta',N'Devuelta',N'Returned'),
(N'PN2_STATE_DESCARTADA',N'Descartada',N'Descartada',N'Discarded'),
(N'PN2_STATE_PENDIENTE',N'Pendiente',N'Pendiente',N'Pending'),
(N'PN2_STATE_PARCIAL',N'Parcial',N'Parcial',N'Partial'),
(N'PN2_STATE_ACTIVA',N'Activa',N'Activa',N'Active'),
(N'PN2_STATE_CANCELADA',N'Cancelada',N'Cancelada',N'Cancelled'),
(N'PN2_STATE_SIN_DESTINO',N'Sin destino registrado',N'Sin destino registrado',N'No destination recorded'),
(N'PN2_STATE_GENERACION',N'Generación',N'Generación',N'Generation'),
(N'PN2_STATE_REGISTRO',N'Registro',N'Registro',N'Registration'),
(N'PN2_STATE_RESTAURACION',N'Restauración',N'Restauración',N'Restoration'),
(N'PN2_STATE_CLASIFICACION',N'Clasificación',N'Clasificación',N'Classification'),
(N'PN2_STATE_LIBERACION',N'Liberación',N'Liberación',N'Release'),
(N'PN2_STATE_BLOQUEO',N'Bloqueo',N'Bloqueo',N'Block'),
(N'PN2_STATE_DESCARTE',N'Descarte',N'Descarte',N'Discard'),
(N'PN2_STATE_UBICACION',N'Cambio de ubicación',N'Cambio de ubicación',N'Location change'),
(N'PN2_STATE_RESERVA',N'Reserva',N'Reserva',N'Reservation'),
(N'PN2_STATE_CANCELACION_RESERVA',N'Cancelación de reserva',N'Cancelación de reserva',N'Reservation cancellation'),
(N'PN2_STATE_CANCELACION',N'Cancelación de reserva',N'Cancelación de reserva',N'Reservation cancellation'),
(N'PN2_STATE_ENTREGA',N'Entrega',N'Entrega',N'Delivery'),
(N'PN2_STATE_TRANSFUSION',N'Transfusión',N'Transfusión',N'Transfusion'),
(N'PN2_STATE_DEVOLUCION',N'Devolución',N'Devolución',N'Return'),
(N'PN2_STATE_REINGRESO',N'Reingreso',N'Reingreso',N'Reentry'),
(N'BITACORA_ACTION_LOCATION_UPDATED',N'Ubicación actualizada',N'Ubicación actualizada',N'Location updated'),
(N'BITACORA_ACTION_TRANSFUSION_REQUEST_REGISTERED',N'Solicitud transfusional registrada',N'Solicitud transfusional registrada',N'Transfusion request registered'),
(N'BITACORA_ACTION_UNIT_RESERVED',N'Unidad reservada',N'Unidad reservada',N'Unit reserved'),
(N'BITACORA_ACTION_RESERVATION_CANCELLED',N'Reserva cancelada',N'Reserva cancelada',N'Reservation cancelled'),
(N'BITACORA_ACTION_UNIT_DELIVERED',N'Unidad entregada',N'Unidad entregada',N'Unit delivered'),
(N'BITACORA_ACTION_UNIT_TRANSFUSED',N'Unidad transfundida',N'Unidad transfundida',N'Unit transfused'),
(N'BITACORA_ACTION_UNIT_RETURNED',N'Unidad devuelta',N'Unidad devuelta',N'Unit returned'),
(N'BITACORA_ACTION_UNIT_REENTERED',N'Unidad reingresada',N'Unidad reingresada',N'Unit reentered');
INSERT dbo.Etiqueta(clave,descripcion)
SELECT clave,descripcion FROM @textos t WHERE NOT EXISTS(SELECT 1 FROM dbo.Etiqueta e WHERE e.clave=t.clave);
INSERT dbo.Traduccion(id_etiqueta,id_idioma,texto)
SELECT e.id_etiqueta,i.id_idioma,CASE i.codigo WHEN 'es-AR' THEN t.es ELSE t.en END
FROM @textos t JOIN dbo.Etiqueta e ON e.clave=t.clave
JOIN dbo.Idioma i ON i.codigo IN ('es-AR','en-US')
WHERE NOT EXISTS(SELECT 1 FROM dbo.Traduccion d WHERE d.id_etiqueta=e.id_etiqueta AND d.id_idioma=i.id_idioma);
COMMIT TRANSACTION;
GO
