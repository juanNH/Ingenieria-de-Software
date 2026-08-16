using System;

namespace Domain
{
    public class BitacoraRegistro_380_jh
    {
        public int Id_380_jh { get; set; }
        public int? IdUsuario_380_jh { get; set; }
        public string IdentificadorUsuario_380_jh { get; set; }
        public string Modulo_380_jh { get; set; }
        public string Accion_380_jh { get; set; }
        public string Nivel_380_jh { get; set; }
        public string Descripcion_380_jh { get; set; }
        public string Equipo_380_jh { get; set; }
        public DateTime Fecha_380_jh { get; set; }

        public AuditoriaMemento_380_jh SaveToMemento_380_jh()
        {
            return new AuditoriaMemento_380_jh("Bitacora", Id_380_jh, new System.Collections.Generic.Dictionary<string, object>
            {
                { "Id", Id_380_jh },
                { "IdUsuario", IdUsuario_380_jh },
                { "IdentificadorUsuario", IdentificadorUsuario_380_jh },
                { "Modulo", Modulo_380_jh },
                { "Accion", Accion_380_jh },
                { "Nivel", Nivel_380_jh },
                { "Descripcion", Descripcion_380_jh },
                { "Equipo", Equipo_380_jh },
                { "Fecha", Fecha_380_jh }
            });
        }
    }
}
