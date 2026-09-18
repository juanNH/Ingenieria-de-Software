using System;
using System.Collections.Generic;

namespace Domain
{
    public sealed class IncidenciaIntegridad_380_jh
    {
        public string Entidad_380_jh { get; set; }
        public int IdEntidad_380_jh { get; set; }
        public string Tipo_380_jh { get; set; }
        public DateTime Fecha_380_jh { get; set; }
        public string EstadoActual_380_jh { get; set; }
        public string EstadoConfiable_380_jh { get; set; }
    }

    public sealed class InformeIntegridad_380_jh
    {
        public bool Disponible_380_jh { get; set; }
        public bool EsValida_380_jh { get; set; }
        public bool PuedeInicializar_380_jh { get; set; }
        public string Revision_380_jh { get; set; }
        public List<IncidenciaIntegridad_380_jh> Incidencias_380_jh { get; } = new List<IncidenciaIntegridad_380_jh>();
        public bool PuedeRestaurar_380_jh
        {
            get
            {
                if (!Disponible_380_jh || EsValida_380_jh || PuedeInicializar_380_jh) return false;
                return Incidencias_380_jh.Count > 0 && !Incidencias_380_jh.Exists(i =>
                    i.Entidad_380_jh == "AuditoriaPN1" || i.Tipo_380_jh == "SIN_INICIALIZAR" ||
                    i.Tipo_380_jh == "SIN_VERSION" || i.Tipo_380_jh == "HISTORIAL_INVALIDO");
            }
        }
    }
}
