using System;

namespace Domain
{
    public class Unidad_380_jh
    {
        public int Id_380_jh { get; set; }
        public string CodigoIdentificacion_380_jh { get; set; }
        public int IdDonacion_380_jh { get; set; }
        public int? IdDonante_380_jh { get; set; }
        public string DocumentoDonante_380_jh { get; set; }
        public string Donante_380_jh { get; set; }
        public string TipoComponente_380_jh { get; set; }
        public DateTime FechaVencimiento_380_jh { get; set; }
        public string GrupoSanguineo_380_jh { get; set; }
        public string FactorRh_380_jh { get; set; }
        public string Observaciones_380_jh { get; set; }
        public string EstadoOperativo_380_jh { get; set; }
        public DateTime FechaAlta_380_jh { get; set; }
        public DateTime FechaUltimaModificacion_380_jh { get; set; }
        public int IdUsuarioUltimaModificacion_380_jh { get; set; }

        public bool EstaClasificada_380_jh
        {
            get
            {
                return !string.IsNullOrWhiteSpace(TipoComponente_380_jh) &&
                       !string.IsNullOrWhiteSpace(GrupoSanguineo_380_jh) &&
                       !string.IsNullOrWhiteSpace(FactorRh_380_jh);
            }
        }
    }
}
