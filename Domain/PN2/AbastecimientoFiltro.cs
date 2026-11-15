using System;

namespace Domain
{
    public class AbastecimientoFiltro_380_jh
    {
        public AbastecimientoFiltro_380_jh()
        {
            Desde_380_jh = DateTime.Today.AddDays(-29);
            Hasta_380_jh = DateTime.Today;
            HorizonteVencimientoDias_380_jh = 7;
        }

        public DateTime Desde_380_jh { get; set; }
        public DateTime Hasta_380_jh { get; set; }
        public int HorizonteVencimientoDias_380_jh { get; set; }
        public string TipoComponente_380_jh { get; set; }
        public string GrupoSanguineo_380_jh { get; set; }
        public string FactorRh_380_jh { get; set; }
    }
}
