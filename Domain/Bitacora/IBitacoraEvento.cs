using System;
using Abstractions;

namespace Domain
{
    public interface IBitacoraEvento_380_jh
    {
        int Id_380_jh { get; set; }
        int? IdUsuario_380_jh { get; set; }
        string IdentificadorUsuario_380_jh { get; set; }
        BitacoraModulo_380_jh Modulo_380_jh { get; set; }
        BitacoraAccion_380_jh Accion_380_jh { get; set; }
        BitacoraNivel_380_jh Nivel_380_jh { get; set; }
        string Descripcion_380_jh { get; set; }
        string Equipo_380_jh { get; set; }
        DateTime Fecha_380_jh { get; set; }
    }
}
