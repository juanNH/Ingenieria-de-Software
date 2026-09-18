using System;
using System.Collections.Generic;

namespace Domain
{
    public class Donacion_380_jh
    {
        public int Id_380_jh { get; set; }
        public int IdDonante_380_jh { get; set; }
        public Donante_380_jh Donante_380_jh { get; set; }
        public DateTime FechaDonacion_380_jh { get; set; }
        public int CantidadUnidades_380_jh { get; set; }
        public string TipoComponente_380_jh { get; set; }
        public DateTime FechaVencimiento_380_jh { get; set; }
        public string Observaciones_380_jh { get; set; }
        public int IdUsuarioResponsable_380_jh { get; set; }
        public DateTime FechaAlta_380_jh { get; set; }
        public List<Unidad_380_jh> Unidades_380_jh { get; set; } = new List<Unidad_380_jh>();
    }
}
