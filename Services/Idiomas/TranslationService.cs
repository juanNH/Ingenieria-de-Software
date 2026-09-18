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
                { "MENU_DONORS", CrearTraducciones_380_jh("Donantes", "Donors") },
                { "MENU_DONATIONS", CrearTraducciones_380_jh("Donaciones", "Donations") },
                { "MENU_UNITS", CrearTraducciones_380_jh("Unidades", "Units") },
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
