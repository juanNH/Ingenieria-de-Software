using System;

namespace Domain
{
    public sealed class Sesion_380_jh
    {
        private static readonly Sesion_380_jh _instance_380_jh = new Sesion_380_jh();
        private static readonly object _lock_380_jh = new object();

        private Usuario_380_jh _usuario_380_jh;
        private DateTime _fechaInicio_380_jh;

        private Sesion_380_jh()
        {
        }

        public static Sesion_380_jh ObtenerInstancia_380_jh()
        {
            return _instance_380_jh;
        }

        public void IniciarSesion_380_jh(Usuario_380_jh usuario)
        {
            lock (_lock_380_jh)
            {
                _usuario_380_jh = usuario;
                _fechaInicio_380_jh = DateTime.Now;
            }
        }

        public Usuario_380_jh ObtenerUsuario_380_jh()
        {
            lock (_lock_380_jh)
            {
                return _usuario_380_jh;
            }
        }

        public DateTime ObtenerFechaInicio_380_jh()
        {
            lock (_lock_380_jh)
            {
                return _fechaInicio_380_jh;
            }
        }

        public bool HaySesionActiva_380_jh()
        {
            lock (_lock_380_jh)
            {
                return _usuario_380_jh != null;
            }
        }

        public void Logout_380_jh()
        {
            lock (_lock_380_jh)
            {
                _usuario_380_jh = null;
                _fechaInicio_380_jh = DateTime.MinValue;
            }
        }
    }
}
