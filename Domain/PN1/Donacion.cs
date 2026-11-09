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

    public sealed class CantidadDonacion_380_jh
    {
        public int Cantidad_380_jh { get; set; }
        public int Version_380_jh { get; set; }
        public bool Editable_380_jh { get; set; }
        public List<CambioCantidadDonacion_380_jh> Historial_380_jh { get; } = new List<CambioCantidadDonacion_380_jh>();
        public List<Unidad_380_jh> Anulables_380_jh { get; } = new List<Unidad_380_jh>();
    }

    public sealed class CambioCantidadDonacion_380_jh
    {
        public int Version_380_jh { get; set; }
        public string Accion_380_jh { get; set; }
        public DateTime Fecha_380_jh { get; set; }
        public string Responsable_380_jh { get; set; }
        public int? Anterior_380_jh { get; set; }
        public int Nueva_380_jh { get; set; }
        public string Motivo_380_jh { get; set; }
        public string Codigos_380_jh { get; set; }
    }
}
