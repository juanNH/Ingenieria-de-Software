using Domain;

namespace Application
{
    public class AutorizacionApplicationService_380_jh
    {
        public bool TienePermiso_380_jh(string codigoPermiso)
        {
            Usuario_380_jh usuario = Sesion_380_jh.ObtenerInstancia_380_jh().ObtenerUsuario_380_jh();
            return usuario != null && usuario.TienePermiso_380_jh(codigoPermiso);
        }

        public bool TienePermiso_380_jh(Usuario_380_jh usuario, string codigoPermiso)
        {
            return usuario != null && usuario.TienePermiso_380_jh(codigoPermiso);
        }
    }
}
