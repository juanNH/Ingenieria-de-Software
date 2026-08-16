using System;
using Abstractions;

namespace Domain
{
    public class Bitacora_380_jh : IBitacoraEvento_380_jh
    {
        public int Id_380_jh { get; set; }
        public int? IdUsuario_380_jh { get; set; }
        public string IdentificadorUsuario_380_jh { get; set; }
        public BitacoraModulo_380_jh Modulo_380_jh { get; set; }
        public BitacoraAccion_380_jh Accion_380_jh { get; set; }
        public BitacoraNivel_380_jh Nivel_380_jh { get; set; }
        public string Descripcion_380_jh { get; set; }
        public string Equipo_380_jh { get; set; }
        public DateTime Fecha_380_jh { get; set; }
    }
}
