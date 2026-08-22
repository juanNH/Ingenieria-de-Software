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
