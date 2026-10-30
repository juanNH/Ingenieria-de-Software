using System;
using System.Collections.Generic;
using Domain;
using Repository;

namespace Services
{
    public class TranslationService_380_jh
    {
        private readonly TranslationRepository_380_jh _translationRepository_380_jh;
        private static readonly Dictionary<string, Dictionary<string, string>> TextosPredeterminados_380_jh =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "BTN_RECALCULATE_DV", CrearTraducciones_380_jh("Recalcular DV", "Recalculate DV") },
                { "GRID_DV_BLOCK", CrearTraducciones_380_jh("Bloqueo DV", "DV block") },
                { "LANGUAGE_SELECTOR", CrearTraducciones_380_jh("Idioma seleccionado", "Selected language") },
                { "AUDIT_FILTERS", CrearTraducciones_380_jh("Filtros", "Filters") },
                { "AUDIT_FILTER_FROM", CrearTraducciones_380_jh("Desde", "From") },
                { "AUDIT_FILTER_TO", CrearTraducciones_380_jh("Hasta", "To") },
                { "AUDIT_FILTER_USER", CrearTraducciones_380_jh("Usuario", "User") },
                { "AUDIT_FILTER_MODULE", CrearTraducciones_380_jh("Módulo", "Module") },
                { "AUDIT_FILTER_ACTION", CrearTraducciones_380_jh("Acción", "Action") },
                { "AUDIT_FILTER_LEVEL", CrearTraducciones_380_jh("Nivel", "Level") },
                { "AUDIT_FILTER_DESCRIPTION", CrearTraducciones_380_jh("Descripción", "Description") },
                { "BTN_SEARCH", CrearTraducciones_380_jh("Buscar", "Search") },
                { "BTN_CLEAR_FILTERS", CrearTraducciones_380_jh("Limpiar", "Clear") },
                { "FILTER_ALL", CrearTraducciones_380_jh("Todos", "All") },
                { "FILTER_ALL_ACTIONS", CrearTraducciones_380_jh("Todas", "All") },
                { "BITACORA_MODULE_SECURITY", CrearTraducciones_380_jh("Seguridad", "Security") },
                { "BITACORA_ACTION_LOGIN_SUCCESS", CrearTraducciones_380_jh("Login exitoso", "Successful login") },
                { "BITACORA_ACTION_LOGIN_FAILURE", CrearTraducciones_380_jh("Login fallido", "Failed login") },
                { "BITACORA_ACTION_REGISTER_FAILURE", CrearTraducciones_380_jh("Registro fallido", "Failed registration") },
                { "BITACORA_MODULE_BLOOD_BANK", CrearTraducciones_380_jh("Banco de sangre", "Blood bank") },
                { "BITACORA_MODULE_INTEGRITY", CrearTraducciones_380_jh("Integridad", "Integrity") },
                { "BITACORA_ACTION_DONOR_REGISTERED", CrearTraducciones_380_jh("Donante registrado", "Donor registered") },
                { "BITACORA_ACTION_DONATION_REGISTERED", CrearTraducciones_380_jh("Donación registrada", "Donation registered") },
                { "BITACORA_ACTION_UNITS_GENERATED", CrearTraducciones_380_jh("Unidades generadas", "Units generated") },
                { "BITACORA_ACTION_UNIT_CLASSIFIED", CrearTraducciones_380_jh("Unidad clasificada", "Unit classified") },
                { "BITACORA_ACTION_UNIT_RELEASED", CrearTraducciones_380_jh("Unidad liberada", "Unit released") },
                { "BITACORA_ACTION_UNIT_BLOCKED", CrearTraducciones_380_jh("Unidad bloqueada", "Unit blocked") },
                { "BITACORA_ACTION_UNIT_DISCARDED", CrearTraducciones_380_jh("Unidad descartada", "Unit discarded") },
                { "BITACORA_ACTION_OPERATION_FAILURE", CrearTraducciones_380_jh("Operación fallida", "Operation failed") },
                { "MENU_INTEGRITY", CrearTraducciones_380_jh("Integridad", "Integrity") },
                { "INTEGRITY_OK", CrearTraducciones_380_jh("Integridad: última verificación correcta.", "Integrity: last verification passed.") },
                { "INTEGRITY_BLOCKED", CrearTraducciones_380_jh("Modo solo consulta: integridad comprometida. Los datos afectados no son confiables; contactá al administrador.", "Read-only mode: integrity compromised. Affected data is untrusted; contact the administrator.") },
                { "INTEGRITY_UNAVAILABLE", CrearTraducciones_380_jh("Operaciones protegidas bloqueadas: no se pudo verificar la integridad. Revisá la conexión y las migraciones SQL.", "Protected operations are blocked: integrity could not be verified. Check the connection and SQL migrations.") },
                { "INTEGRITY_PENDING", CrearTraducciones_380_jh("Pendiente: revisar los datos y aceptar la base inicial.", "Pending: review the data and accept the initial baseline.") },
                { "INTEGRITY_DESCRIPTION", CrearTraducciones_380_jh("Se verifican filas (DVH), tablas (DVV) e historiales de los procesos protegidos. Restaurar recupera las últimas versiones verificadas; no deshace operaciones legítimas.", "Checks rows (DVH), tables (DVV), and histories for protected processes. Restore recovers the latest verified versions; it does not undo legitimate operations.") },
                { "INTEGRITY_VERIFY", CrearTraducciones_380_jh("Verificar / actualizar", "Verify / refresh") },
                { "INTEGRITY_INITIALIZE", CrearTraducciones_380_jh("Aceptar base inicial", "Accept initial baseline") },
                { "INTEGRITY_RESTORE", CrearTraducciones_380_jh("Restaurar desde historial", "Restore from history") },
                { "INTEGRITY_ENTITY", CrearTraducciones_380_jh("Entidad", "Entity") },
                { "INTEGRITY_ID", CrearTraducciones_380_jh("ID", "ID") },
                { "INTEGRITY_TYPE", CrearTraducciones_380_jh("Incidencia", "Issue") },
                { "INTEGRITY_DATE", CrearTraducciones_380_jh("Detectada", "Detected") },
                { "INTEGRITY_CURRENT", CrearTraducciones_380_jh("Registro actual (JSON; vacío si falta)", "Current record (JSON; empty if missing)") },
                { "INTEGRITY_TRUSTED", CrearTraducciones_380_jh("Última versión del historial (usar solo si es íntegro)", "Latest history version (use only when intact)") },
                { "INTEGRITY_CONFIRM_BASELINE", CrearTraducciones_380_jh("¿Aceptás los datos actuales como base inicial? Confirmá solo después de revisarlos y respaldarlos. Esta acción no corrige datos previos y se realiza una sola vez.", "Accept current data as the initial baseline? Confirm only after reviewing and backing it up. This does not correct existing data and can only be done once.") },
                { "INTEGRITY_CONFIRM_RESTORE", CrearTraducciones_380_jh("¿Restaurar todas las inconsistencias recuperables desde las últimas versiones verificadas? Se conservarán los identificadores y se auditará la operación. No se aceptarán datos alterados como válidos.", "Restore all recoverable inconsistencies from the latest verified versions? IDs will be preserved and the operation audited. Altered data will not be accepted as valid.") },
                { "INTEGRITY_RECOVERED", CrearTraducciones_380_jh("Operación completada e integridad verificada.", "Operation completed and integrity verified.") },
                { "INTEGRITY_RECOVERY_FAILED", CrearTraducciones_380_jh("No se confirmó la recuperación. Verificá nuevamente el estado y revisá la bitácora antes de reintentar.", "Recovery was not confirmed. Verify the state again and review the log before retrying.") },
                { "INTEGRITY_STALE_PREVIEW", CrearTraducciones_380_jh("Los datos cambiaron desde la revisión. Actualizá el informe antes de confirmar.", "Data changed since the preview. Refresh the report before confirming.") },
                { "INTEGRITY_BACKUP_REQUIRED", CrearTraducciones_380_jh("La fuente no permite una recuperación segura. Necesitás un backup confiable; no recalcules para ocultar el error.", "The source cannot support safe recovery. A trusted backup is required; do not recalculate to hide the error.") },
                { "INTEGRITY_TYPE_SIN_INICIALIZAR", CrearTraducciones_380_jh("Base inicial ausente o incompleta", "Missing or incomplete baseline") },
                { "INTEGRITY_TYPE_DVH", CrearTraducciones_380_jh("Dígito de fila incorrecto", "Row checksum mismatch") },
                { "INTEGRITY_TYPE_DVV", CrearTraducciones_380_jh("Dígito de tabla incorrecto", "Table checksum mismatch") },
                { "INTEGRITY_TYPE_HISTORIAL_INVALIDO", CrearTraducciones_380_jh("Historial alterado", "Altered history") },
                { "INTEGRITY_TYPE_FALTANTE", CrearTraducciones_380_jh("Registro eliminado", "Deleted record") },
                { "INTEGRITY_TYPE_SIN_VERSION", CrearTraducciones_380_jh("Registro sin versión confiable", "Record without a trusted version") },
                { "INTEGRITY_TYPE_VERSION_DIFERENTE", CrearTraducciones_380_jh("Datos distintos del historial", "Data differs from history") },
                { "BITACORA_ACTION_INTEGRITY_DETECTED", CrearTraducciones_380_jh("Error de integridad detectado", "Integrity error detected") },
                { "BITACORA_ACTION_INTEGRITY_INITIALIZED", CrearTraducciones_380_jh("Base inicial de integridad aceptada", "Initial integrity baseline accepted") },
                { "BITACORA_ACTION_INTEGRITY_RESTORED", CrearTraducciones_380_jh("Integridad restaurada", "Integrity restored") },
                { "BITACORA_ACTION_RESTORE_FAILED", CrearTraducciones_380_jh("Recuperación fallida", "Recovery failed") },
                { "MENU_BLOOD_BANK", CrearTraducciones_380_jh("Banco de sangre", "Blood bank") },
                { "RECEIPT_PDF", CrearTraducciones_380_jh("Comprobante PDF", "PDF receipt") },
                { "RECEIPT_SAVE", CrearTraducciones_380_jh("Guardar comprobante PDF", "Save PDF receipt") },
                { "RECEIPT_SAVED", CrearTraducciones_380_jh("Comprobante PDF guardado.", "PDF receipt saved.") },
                { "RECEIPT_ERROR", CrearTraducciones_380_jh("No se pudo generar el comprobante. Verificá los permisos, la integridad y el acceso al archivo.", "The receipt could not be generated. Check permissions, integrity and file access.") },
                { "MENU_PN1", CrearTraducciones_380_jh("PN1", "PN1") },
                { "MENU_PN2", CrearTraducciones_380_jh("PN2", "PN2") },
                { "MENU_CONSULTAS", CrearTraducciones_380_jh("Consultas", "Queries") },
                { "MENU_DONORS", CrearTraducciones_380_jh("Donantes", "Donors") },
                { "MENU_DONATIONS", CrearTraducciones_380_jh("Donaciones", "Donations") },
                { "MENU_UNITS", CrearTraducciones_380_jh("Unidades", "Units") },
                { "MENU_INVENTORY", CrearTraducciones_380_jh("Inventario", "Inventory") },
                { "MENU_TRANSFUSION_REQUESTS", CrearTraducciones_380_jh("Solicitudes transfusionales", "Transfusion requests") },
                { "MENU_RESERVATIONS", CrearTraducciones_380_jh("Reservas y entregas", "Reservations and deliveries") },
                { "MENU_UNIT_DESTINATION", CrearTraducciones_380_jh("Destino de unidades", "Unit destinations") },
                { "MENU_TRACEABILITY", CrearTraducciones_380_jh("Trazabilidad", "Traceability") },
                { "MENU_REPORTS_ALERTS", CrearTraducciones_380_jh("Reportes y alertas", "Reports and alerts") },
                { "PN2_INVENTORY_TITLE", CrearTraducciones_380_jh("Inventario de unidades", "Unit inventory") },
                { "PN2_INVENTORY_DESCRIPTION", CrearTraducciones_380_jh("Consultá el estado y la ubicación de las unidades del banco de sangre.", "Review the status and location of blood bank units.") },
                { "PN2_INVENTORY_DESCRIPTION_VIEW", CrearTraducciones_380_jh("Consultá las unidades y filtrá por código, componente, grupo, estado o vencimiento.", "Review units and filter by code, component, blood group, status or expiration date.") },
                { "PN2_INVENTORY_UPDATE", CrearTraducciones_380_jh("Unidad seleccionada", "Selected unit") },
                { "PN2_INVENTORY_NOTE", CrearTraducciones_380_jh("El reingreso de una unidad devuelta requiere motivo y vuelve a En revisión; luego debe clasificarse y liberarse en Unidades.", "Reentering a returned unit requires a reason and returns it to Under review; it must then be classified and released in Units.") },
                { "PN2_LOCATION", CrearTraducciones_380_jh("Ubicación", "Location") },
                { "PN2_UPDATE_LOCATION", CrearTraducciones_380_jh("Actualizar ubicación", "Update location") },
                { "PN2_REENTER_UNIT", CrearTraducciones_380_jh("Reingresar unidad devuelta", "Reenter returned unit") },
                { "PN2_REQUESTS_TITLE", CrearTraducciones_380_jh("Solicitudes transfusionales", "Transfusion requests") },
                { "PN2_REQUESTS_DESCRIPTION", CrearTraducciones_380_jh("El Profesional solicitante registra el pedido. Puede conservarse pendiente aunque no haya unidades disponibles.", "The requesting professional registers the request. It may remain pending when no units are available.") },
                { "PN2_REQUEST_ID", CrearTraducciones_380_jh("Solicitud", "Request") },
                { "PN2_PATIENT", CrearTraducciones_380_jh("Paciente receptor", "Recipient patient") },
                { "PN2_REQUESTING_PROFESSIONAL", CrearTraducciones_380_jh("Profesional solicitante", "Requesting professional") },
                { "PN2_REQUEST_DATE", CrearTraducciones_380_jh("Fecha de solicitud", "Request date") },
                { "PN2_REQUIRED", CrearTraducciones_380_jh("Requeridas", "Required") },
                { "PN2_RESERVED", CrearTraducciones_380_jh("Reservadas", "Reserved") },
                { "PN2_DELIVERED", CrearTraducciones_380_jh("Entregadas vigentes", "Valid deliveries") },
                { "PN2_PENDING", CrearTraducciones_380_jh("Saldo pendiente", "Pending balance") },
                { "PN2_NEW_REQUEST", CrearTraducciones_380_jh("Registrar solicitud transfusional", "Register transfusion request") },
                { "PN2_QUANTITY_NOTE", CrearTraducciones_380_jh("Se permite atención parcial. Las devoluciones y los descartes reabren el saldo; el historial conserva todas las entregas.", "Partial fulfillment is allowed. Returns and discards reopen the balance; all deliveries remain in history.") },
                { "PN2_RESERVATIONS_TITLE", CrearTraducciones_380_jh("Reservas y entregas", "Reservations and deliveries") },
                { "PN2_RESERVATIONS_DESCRIPTION", CrearTraducciones_380_jh("Elegí una solicitud y una unidad para reservar. Seleccioná una reserva activa para entregarla o cancelarla con motivo.", "Choose a request and a unit to reserve. Select an active reservation to deliver it or cancel it with a reason.") },
                { "PN2_AVAILABLE_UNITS", CrearTraducciones_380_jh("Unidades liberadas y vigentes del componente solicitado", "Released, unexpired units of the requested component") },
                { "PN2_RESERVATION_ID", CrearTraducciones_380_jh("Reserva", "Reservation") },
                { "PN2_RESERVATION_DATE", CrearTraducciones_380_jh("Fecha de reserva", "Reservation date") },
                { "PN2_RESPONSIBLE", CrearTraducciones_380_jh("Responsable", "Responsible user") },
                { "PN2_CANCELLATION_REASON", CrearTraducciones_380_jh("Motivo de cancelación", "Cancellation reason") },
                { "PN2_RESERVATION_ACTIONS", CrearTraducciones_380_jh("Atender solicitud", "Fulfill request") },
                { "PN2_REASON_OBSERVATION", CrearTraducciones_380_jh("Motivo / observación", "Reason / note") },
                { "PN2_REQUEST_COUNTS", CrearTraducciones_380_jh("Requeridas: {0} · Reservadas: {1} · Entregadas vigentes: {2} · Saldo pendiente: {3}", "Required: {0} · Reserved: {1} · Valid deliveries: {2} · Pending balance: {3}") },
                { "PN2_RESERVE", CrearTraducciones_380_jh("Reservar unidad seleccionada", "Reserve selected unit") },
                { "PN2_DELIVER", CrearTraducciones_380_jh("Entregar reserva seleccionada", "Deliver selected reservation") },
                { "PN2_CANCEL_RESERVATION", CrearTraducciones_380_jh("Cancelar reserva seleccionada", "Cancel selected reservation") },
                { "PN2_DESTINATIONS_TITLE", CrearTraducciones_380_jh("Destino de unidades entregadas", "Delivered unit destinations") },
                { "PN2_DESTINATIONS_DESCRIPTION", CrearTraducciones_380_jh("Registrá la transfusión, devolución o descarte de una entrega. El listado conserva también los destinos ya registrados.", "Record a delivery's transfusion, return or discard. The list also preserves previously recorded destinations.") },
                { "PN2_DELIVERY_ID", CrearTraducciones_380_jh("Entrega", "Delivery") },
                { "PN2_DELIVERY_DATE", CrearTraducciones_380_jh("Fecha de entrega", "Delivery date") },
                { "PN2_DESTINATION", CrearTraducciones_380_jh("Destino", "Destination") },
                { "PN2_DESTINATION_DATE", CrearTraducciones_380_jh("Fecha de destino", "Destination date") },
                { "PN2_REASON", CrearTraducciones_380_jh("Motivo", "Reason") },
                { "PN2_REGISTER_DESTINATION", CrearTraducciones_380_jh("Registrar destino", "Record destination") },
                { "PN2_DESTINATION_NOTE", CrearTraducciones_380_jh("Devolución y descarte requieren motivo y reabren el saldo. La devolución requiere reingreso autorizado antes de reutilizar la unidad.", "Returns and discards require a reason and reopen the balance. Returned units require authorized reentry before reuse.") },
                { "PN2_TRACE_TITLE", CrearTraducciones_380_jh("Trazabilidad de unidades", "Unit traceability") },
                { "PN2_TRACE_DESCRIPTION", CrearTraducciones_380_jh("Consultá movimientos y vínculos con solicitud, reserva y entrega. Las fechas opcionales filtran la fecha del movimiento.", "Review movements and their request, reservation and delivery links. Optional dates filter the movement date.") },
                { "PN2_UNIT_FILTER", CrearTraducciones_380_jh("ID unidad (0: todas)", "Unit ID (0: all)") },
                { "PN2_UNIT_ID", CrearTraducciones_380_jh("ID unidad", "Unit ID") },
                { "PN2_MOVEMENT_DATE", CrearTraducciones_380_jh("Fecha del movimiento", "Movement date") },
                { "PN2_MOVEMENT", CrearTraducciones_380_jh("Movimiento", "Movement") },
                { "PN2_PREVIOUS_STATE", CrearTraducciones_380_jh("Estado anterior", "Previous status") },
                { "PN2_NEW_STATE", CrearTraducciones_380_jh("Estado nuevo", "New status") },
                { "PN2_REPORTS_TITLE", CrearTraducciones_380_jh("Reportes y alertas", "Reports and alerts") },
                { "PN2_REPORTS_DESCRIPTION", CrearTraducciones_380_jh("Stock y alertas muestran la situación actual. Las fechas filtran solicitudes y movimientos; no reconstruyen el inventario pasado.", "Stock and alerts show the current situation. Dates filter requests and movements; they do not reconstruct past inventory.") },
                { "PN2_GENERATE_REPORT", CrearTraducciones_380_jh("Consultar reporte", "View report") },
                { "PN2_REPORT_SECTION", CrearTraducciones_380_jh("Sección", "Section") },
                { "PN2_REPORT_CONCEPT", CrearTraducciones_380_jh("Concepto", "Concept") },
                { "PN2_REPORT_QUANTITY", CrearTraducciones_380_jh("Cantidad", "Quantity") },
                { "PN2_REPORT_DETAIL", CrearTraducciones_380_jh("Detalle", "Details") },
                { "PN2_REPORT_INVENTARIO", CrearTraducciones_380_jh("Inventario", "Inventory") },
                { "PN2_REPORT_SOLICITUDES", CrearTraducciones_380_jh("Solicitudes", "Requests") },
                { "PN2_REPORT_RESERVAS", CrearTraducciones_380_jh("Reservas", "Reservations") },
                { "PN2_REPORT_MOVIMIENTOS", CrearTraducciones_380_jh("Movimientos", "Movements") },
                { "PN2_REPORT_ALERTAS", CrearTraducciones_380_jh("Alertas", "Alerts") },
                { "PN2_REPORT_SALDO_PENDIENTE", CrearTraducciones_380_jh("Saldo pendiente", "Pending balance") },
                { "PN2_REPORT_ACTIVAS", CrearTraducciones_380_jh("Activas", "Active") },
                { "PN2_REPORT_UNIDADES_VENCIDAS", CrearTraducciones_380_jh("Unidades vencidas", "Expired units") },
                { "PN2_REPORT_RESERVAS_INHABILITADAS", CrearTraducciones_380_jh("Reservas inhabilitadas", "Invalid reservations") },
                { "PN2_REPORT_SOLICITUDES_SIN_DISPONIBILIDAD", CrearTraducciones_380_jh("Solicitudes sin disponibilidad", "Requests without available units") },
                { "PN2_UNCLASSIFIED", CrearTraducciones_380_jh("(sin clasificar)", "(unclassified)") },
                { "PN2_REPORT_CURRENT_INVENTORY_NOTE", CrearTraducciones_380_jh("Inventario actual; el período no reconstruye stock histórico.", "Current inventory; the period does not reconstruct historical stock.") },
                { "PN2_REPORT_REQUESTS_NOTE", CrearTraducciones_380_jh("Solicitudes registradas en el período seleccionado.", "Requests recorded in the selected period.") },
                { "PN2_REPORT_BALANCE_NOTE", CrearTraducciones_380_jh("Unidades aún no cubiertas por reservas activas o entregas efectivas.", "Units not yet covered by active reservations or valid deliveries.") },
                { "PN2_REPORT_RESERVATIONS_NOTE", CrearTraducciones_380_jh("Reservas vigentes registradas en el período seleccionado.", "Active reservations recorded in the selected period.") },
                { "PN2_REPORT_MOVEMENTS_NOTE", CrearTraducciones_380_jh("Movimientos registrados en el período; filtro de estado según resultado del movimiento.", "Movements recorded in the period; status filter uses the movement's resulting status.") },
                { "PN2_REPORT_EXPIRED_NOTE", CrearTraducciones_380_jh("Unidades vencidas cuyo recorrido aún no terminó en transfusión o descarte.", "Expired units whose process has not ended in transfusion or discard.") },
                { "PN2_REPORT_INVALID_RESERVATIONS_NOTE", CrearTraducciones_380_jh("Reservas activas con unidad vencida, sin clasificación o con estado/componente incompatible.", "Active reservations with an expired, unclassified unit or an incompatible status/component.") },
                { "PN2_REPORT_UNAVAILABLE_NOTE", CrearTraducciones_380_jh("Solicitudes con saldo y sin unidad liberada, clasificada y vigente del componente solicitado.", "Requests with pending balance and no released, classified, unexpired unit of the requested component.") },
                { "PN2_SEARCH", CrearTraducciones_380_jh("Buscar", "Search") },
                { "PN2_RECORD_COUNT", CrearTraducciones_380_jh("{0} registros", "{0} records") },
                { "PN2_SELECT_UNIT", CrearTraducciones_380_jh("Seleccioná una unidad.", "Select a unit.") },
                { "PN2_SELECT_REQUEST", CrearTraducciones_380_jh("Seleccioná una solicitud.", "Select a request.") },
                { "PN2_SELECT_REQUEST_UNIT", CrearTraducciones_380_jh("Seleccioná una solicitud y una unidad disponible.", "Select a request and an available unit.") },
                { "PN2_SELECT_RESERVATION", CrearTraducciones_380_jh("Seleccioná una reserva activa.", "Select an active reservation.") },
                { "PN2_SELECT_DELIVERY", CrearTraducciones_380_jh("Seleccioná una entrega sin destino registrado.", "Select a delivery without a recorded destination.") },
                { "PN2_REASON_REQUIRED", CrearTraducciones_380_jh("Ingresá el motivo u observación de la operación.", "Enter a reason or note for the operation.") },
                { "PN2_CONFIRM_DELIVERY", CrearTraducciones_380_jh("¿Confirmás la entrega de la unidad {0} a la solicitud seleccionada?", "Confirm delivery of unit {0} for the selected request?") },
                { "PN2_CONFIRM_CANCEL", CrearTraducciones_380_jh("¿Cancelás la reserva de la unidad {0}? La solicitud recuperará su saldo pendiente.", "Cancel the reservation of unit {0}? Its quantity will return to the request's pending balance.") },
                { "PN2_CONFIRM_DESTINATION", CrearTraducciones_380_jh("¿Confirmás el destino {0}? Se conservará en el historial de la entrega.", "Confirm destination {0}? It will be preserved in the delivery history.") },
                { "PN2_CONFIRM_REENTER", CrearTraducciones_380_jh("¿Reingresás la unidad devuelta {0} a En revisión? Deberá clasificarse y liberarse antes de una nueva reserva.", "Reenter returned unit {0} as Under review? It must be classified and released before a new reservation.") },
                { "PN2_SAVED", CrearTraducciones_380_jh("Operación registrada correctamente.", "Operation recorded successfully.") },
                { "PN2_INVALID", CrearTraducciones_380_jh("Revisá los datos obligatorios, la cantidad y el motivo cuando corresponda.", "Check required fields, quantity and the reason when applicable.") },
                { "PN2_NOT_FOUND", CrearTraducciones_380_jh("No se encontró el registro. Actualizá el listado antes de reintentar.", "The record was not found. Refresh the list before retrying.") },
                { "PN2_CONFLICT", CrearTraducciones_380_jh("El estado o los vínculos cambiaron y no permiten esta operación. Actualizá el listado.", "The status or links changed and do not allow this operation. Refresh the list.") },
                { "PN2_UNAVAILABLE", CrearTraducciones_380_jh("La unidad no está disponible, vigente o habilitada para esta operación.", "The unit is unavailable, expired or not enabled for this operation.") },
                { "PN2_QUANTITY_COMPLETE", CrearTraducciones_380_jh("La cantidad requerida ya está cubierta por reservas o entregas vigentes.", "The required quantity is already covered by reservations or valid deliveries.") },
                { "PN2_SAVE_ERROR", CrearTraducciones_380_jh("No se confirmó la operación. Actualizá el listado y revisá su estado antes de reintentar.", "The operation was not confirmed. Refresh the list and check its status before retrying.") },
                { "PN2_QUERY_ERROR", CrearTraducciones_380_jh("No se pudo actualizar la consulta. Los datos visibles pueden corresponder a una consulta anterior. Reintentá la actualización.", "The query could not be refreshed. Visible data may be from a previous query. Try refreshing again.") },
                { "PN2_READ_FAILED", CrearTraducciones_380_jh("No se pudo completar la consulta.", "The query could not be completed.") },
                { "PN2_INVALID_FILTER", CrearTraducciones_380_jh("La fecha Desde debe ser anterior o igual a la fecha Hasta.", "The From date must be earlier than or equal to the To date.") },
                { "PN2_STATE_EN_REVISION", CrearTraducciones_380_jh("En revisión", "Under review") },
                { "PN2_STATE_LIBERADA", CrearTraducciones_380_jh("Liberada", "Released") },
                { "PN2_STATE_BLOQUEADA", CrearTraducciones_380_jh("Bloqueada", "Blocked") },
                { "PN2_STATE_RESERVADA", CrearTraducciones_380_jh("Reservada", "Reserved") },
                { "PN2_STATE_ENTREGADA", CrearTraducciones_380_jh("Entregada", "Delivered") },
                { "PN2_STATE_TRANSFUNDIDA", CrearTraducciones_380_jh("Transfundida", "Transfused") },
                { "PN2_STATE_DEVUELTA", CrearTraducciones_380_jh("Devuelta", "Returned") },
                { "PN2_STATE_DESCARTADA", CrearTraducciones_380_jh("Descartada", "Discarded") },
                { "PN2_STATE_PENDIENTE", CrearTraducciones_380_jh("Pendiente", "Pending") },
                { "PN2_STATE_PARCIAL", CrearTraducciones_380_jh("Parcial", "Partial") },
                { "PN2_STATE_ACTIVA", CrearTraducciones_380_jh("Activa", "Active") },
                { "PN2_STATE_CANCELADA", CrearTraducciones_380_jh("Cancelada", "Cancelled") },
                { "PN2_STATE_SIN_DESTINO", CrearTraducciones_380_jh("Sin destino registrado", "No destination recorded") },
                { "PN2_STATE_GENERACION", CrearTraducciones_380_jh("Generación", "Generation") },
                { "PN2_STATE_REGISTRO", CrearTraducciones_380_jh("Registro", "Registration") },
                { "PN2_STATE_RESTAURACION", CrearTraducciones_380_jh("Restauración", "Restoration") },
                { "PN2_STATE_CLASIFICACION", CrearTraducciones_380_jh("Clasificación", "Classification") },
                { "PN2_STATE_LIBERACION", CrearTraducciones_380_jh("Liberación", "Release") },
                { "PN2_STATE_BLOQUEO", CrearTraducciones_380_jh("Bloqueo", "Block") },
                { "PN2_STATE_DESCARTE", CrearTraducciones_380_jh("Descarte", "Discard") },
                { "PN2_STATE_UBICACION", CrearTraducciones_380_jh("Cambio de ubicación", "Location change") },
                { "PN2_STATE_RESERVA", CrearTraducciones_380_jh("Reserva", "Reservation") },
                { "PN2_STATE_CANCELACION_RESERVA", CrearTraducciones_380_jh("Cancelación de reserva", "Reservation cancellation") },
                { "PN2_STATE_CANCELACION", CrearTraducciones_380_jh("Cancelación de reserva", "Reservation cancellation") },
                { "PN2_STATE_ENTREGA", CrearTraducciones_380_jh("Entrega", "Delivery") },
                { "PN2_STATE_TRANSFUSION", CrearTraducciones_380_jh("Transfusión", "Transfusion") },
                { "PN2_STATE_DEVOLUCION", CrearTraducciones_380_jh("Devolución", "Return") },
                { "PN2_STATE_REINGRESO", CrearTraducciones_380_jh("Reingreso", "Reentry") },
                { "BITACORA_ACTION_LOCATION_UPDATED", CrearTraducciones_380_jh("Ubicación actualizada", "Location updated") },
                { "BITACORA_ACTION_TRANSFUSION_REQUEST_REGISTERED", CrearTraducciones_380_jh("Solicitud transfusional registrada", "Transfusion request registered") },
                { "BITACORA_ACTION_UNIT_RESERVED", CrearTraducciones_380_jh("Unidad reservada", "Unit reserved") },
                { "BITACORA_ACTION_RESERVATION_CANCELLED", CrearTraducciones_380_jh("Reserva cancelada", "Reservation cancelled") },
                { "BITACORA_ACTION_UNIT_DELIVERED", CrearTraducciones_380_jh("Unidad entregada", "Unit delivered") },
                { "BITACORA_ACTION_UNIT_TRANSFUSED", CrearTraducciones_380_jh("Unidad transfundida", "Unit transfused") },
                { "BITACORA_ACTION_UNIT_RETURNED", CrearTraducciones_380_jh("Unidad devuelta", "Unit returned") },
                { "BITACORA_ACTION_UNIT_REENTERED", CrearTraducciones_380_jh("Unidad reingresada", "Unit reentered") },
                { "DONOR_REGISTERED", CrearTraducciones_380_jh("Donante registrado correctamente.", "Donor registered successfully.") },
                { "DONOR_DUPLICATE", CrearTraducciones_380_jh("Ya existe un donante con ese documento.", "A donor with that document already exists.") },
                { "DONOR_INVALID", CrearTraducciones_380_jh("Completá documento, nombre y apellido.", "Document, first name and last name are required.") },
                { "DONOR_ERROR", CrearTraducciones_380_jh("No se pudo registrar el donante.", "Could not register the donor.") },
                { "DONATION_REGISTERED", CrearTraducciones_380_jh("Donación registrada y unidades generadas.", "Donation registered and units generated.") },
                { "DONATION_INVALID", CrearTraducciones_380_jh("Completá los datos de la donación y verificá el vencimiento.", "Complete the donation data and verify the expiration date.") },
                { "DONATION_ERROR", CrearTraducciones_380_jh("No se pudo registrar la donación.", "Could not register the donation.") },
                { "UNIT_CLASSIFIED", CrearTraducciones_380_jh("Unidad clasificada correctamente.", "Unit classified successfully.") },
                { "UNIT_RELEASED", CrearTraducciones_380_jh("Unidad liberada correctamente.", "Unit released successfully.") },
                { "UNIT_BLOCKED", CrearTraducciones_380_jh("Unidad bloqueada correctamente.", "Unit blocked successfully.") },
                { "UNIT_DISCARDED", CrearTraducciones_380_jh("Unidad descartada correctamente.", "Unit discarded successfully.") },
                { "UNIT_INVALID", CrearTraducciones_380_jh("Completá los datos requeridos de la unidad.", "Complete the required unit data.") },
                { "UNIT_CONFLICT", CrearTraducciones_380_jh("La unidad no está en un estado válido para esta operación.", "The unit is not in a valid state for this operation.") },
                { "UNIT_ERROR", CrearTraducciones_380_jh("No se pudo actualizar la unidad.", "Could not update the unit.") },
                { "UNIT_EXPORT_XML", CrearTraducciones_380_jh("Exportar proceso a XML", "Export process to XML") },
                { "UNIT_EXPORT_XML_SELECT", CrearTraducciones_380_jh("Seleccioná una unidad para exportar.", "Select a unit to export.") },
                { "UNIT_EXPORT_XML_SUCCESS", CrearTraducciones_380_jh("Proceso PN1 exportado a XML correctamente.", "PN1 process exported to XML successfully.") },
                { "UNIT_EXPORT_XML_ERROR", CrearTraducciones_380_jh("No se pudo exportar el proceso PN1 a XML.", "Could not export the PN1 process to XML.") },
                { "UNIT_IMPORT_XML", CrearTraducciones_380_jh("Deserializar XML", "Deserialize XML") },
                { "UNIT_CLEAR_XML", CrearTraducciones_380_jh("Limpiar XML", "Clear XML") },
                { "UNIT_TAB_DATABASE", CrearTraducciones_380_jh("Unidades", "Units") },
                { "UNIT_TAB_XML", CrearTraducciones_380_jh("Vista previa XML", "XML preview") },
                { "UNIT_XML_PREVIEW", CrearTraducciones_380_jh("Vista previa: {0} unidades. Datos no verificados contra la base.", "Preview: {0} units. Data not verified against the database.") },
                { "UNIT_XML_PREVIEW_COUNT", CrearTraducciones_380_jh("Vista previa: {0} unidades; el archivo declara {1}. Datos no verificados contra la base.", "Preview: {0} units; file declares {1}. Data not verified against the database.") },
                { "UNIT_IMPORT_XML_ERROR", CrearTraducciones_380_jh("No se pudo importar el XML. Revisá el formato y los campos, y que los identificadores y códigos no estén repetidos.", "Could not import the XML. Check the format and fields and ensure identifiers and codes are unique.") },
                { "UNIT_SERIALIZE_TABLE", CrearTraducciones_380_jh("Serializar tabla", "Serialize table") },
                { "UNIT_DESERIALIZE_TABLE", CrearTraducciones_380_jh("Deserializar XML", "Deserialize XML") },
                { "UNIT_CLEAR_TABLE", CrearTraducciones_380_jh("Limpiar tabla", "Clear table") },
                { "UNIT_TABLE_PREVIEW", CrearTraducciones_380_jh("Vista temporal: {0} unidades. Datos XML sin verificar; Actualizar recupera la base.", "Temporary view: {0} units. Unverified XML data; Refresh reloads the database.") },
                { "UNIT_TABLE_EMPTY", CrearTraducciones_380_jh("La tabla no tiene filas para serializar.", "There are no table rows to serialize.") },
                { "UNIT_TABLE_EXPORT_SUCCESS", CrearTraducciones_380_jh("Tabla serializada correctamente.", "Table serialized successfully.") },
                { "UNIT_TABLE_EXPORT_ERROR", CrearTraducciones_380_jh("No se pudo serializar la tabla. Revisá los datos visibles y la ubicación del archivo.", "Could not serialize the table. Check the displayed data and file location.") },
                { "UNIT_TABLE_IMPORT_ERROR", CrearTraducciones_380_jh("No se pudo deserializar el XML. Revisá su formato, sus campos y que los códigos sean únicos.", "Could not deserialize the XML. Check its format and fields and ensure codes are unique.") },
                { "OPERATION_NOT_AUTHORIZED", CrearTraducciones_380_jh("No tenés permisos para realizar esta operación.", "You are not authorized to perform this operation.") },
                { "UNIT_STATE_EN_REVISION", CrearTraducciones_380_jh("En revisión", "Under review") },
                { "UNIT_STATE_LIBERADA", CrearTraducciones_380_jh("Liberada", "Released") },
                { "UNIT_STATE_BLOQUEADA", CrearTraducciones_380_jh("Bloqueada", "Blocked") },
                { "UNIT_STATE_DESCARTADA", CrearTraducciones_380_jh("Descartada", "Discarded") },
                { "STATUS_ACTIVE", CrearTraducciones_380_jh("Activo", "Active") },
                { "STATUS_INACTIVE", CrearTraducciones_380_jh("Inactivo", "Inactive") },
                { "BTN_REGISTER", CrearTraducciones_380_jh("Registrar", "Register") },
                { "BTN_CLEAR", CrearTraducciones_380_jh("Limpiar", "Clear") },
                { "BITACORA_LEVEL_INFORMATION", CrearTraducciones_380_jh("Información", "Information") },
                { "BITACORA_LEVEL_WARNING", CrearTraducciones_380_jh("Advertencia", "Warning") },
                { "BITACORA_LEVEL_ERROR", CrearTraducciones_380_jh("Error", "Error") }
            };

        public TranslationService_380_jh()
            : this(new TranslationRepository_380_jh())
        {
        }

        public TranslationService_380_jh(TranslationRepository_380_jh translationRepository)
        {
            _translationRepository_380_jh = translationRepository;
        }

        public string Translate_380_jh(string key, Idioma_380_jh idioma)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            if (idioma == null || idioma.Id_380_jh == 0)
            {
                return key;
            }

            string texto = _translationRepository_380_jh.ObtenerTexto_380_jh(key, idioma.Id_380_jh);
            if (!string.IsNullOrWhiteSpace(texto))
            {
                return texto;
            }

            return ObtenerTextoPredeterminado_380_jh(key, idioma.Codigo_380_jh);
        }

        public Dictionary<string, string> ListarPorIdioma_380_jh(Idioma_380_jh idioma)
        {
            if (idioma == null || idioma.Id_380_jh == 0)
            {
                return new Dictionary<string, string>();
            }

            return _translationRepository_380_jh.ListarPorIdioma_380_jh(idioma.Id_380_jh);
        }

        private static Dictionary<string, string> CrearTraducciones_380_jh(string espanol, string ingles)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "es-AR", espanol },
                { "en-US", ingles }
            };
        }

        private static string ObtenerTextoPredeterminado_380_jh(string key, string codigoIdioma)
        {
            Dictionary<string, string> traducciones;
            if (TextosPredeterminados_380_jh.TryGetValue(key, out traducciones))
            {
                string texto;
                if (!string.IsNullOrWhiteSpace(codigoIdioma) &&
                    traducciones.TryGetValue(codigoIdioma, out texto))
                {
                    return texto;
                }

                string codigoBase = !string.IsNullOrWhiteSpace(codigoIdioma) &&
                    codigoIdioma.StartsWith("en", StringComparison.OrdinalIgnoreCase)
                    ? "en-US"
                    : "es-AR";
                if (traducciones.TryGetValue(codigoBase, out texto))
                {
                    return texto;
                }
            }

            return key;
        }
    }
}
