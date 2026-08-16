using System;

namespace Domain
{
    public abstract class BitacoraFactory_380_jh
    {
        public IBitacoraEvento_380_jh Crear_380_jh(string identificadorUsuario, string descripcion)
        {
            IBitacoraEvento_380_jh evento = CrearEvento_380_jh();
            evento.IdentificadorUsuario_380_jh = identificadorUsuario;
            evento.Descripcion_380_jh = descripcion;
            evento.Equipo_380_jh = Environment.MachineName;
            evento.Fecha_380_jh = DateTime.Now;
            return evento;
        }

        protected abstract IBitacoraEvento_380_jh CrearEvento_380_jh();
    }
}
