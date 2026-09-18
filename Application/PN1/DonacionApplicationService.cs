using Abstractions;
using Domain;
using Repository;

namespace Application
{
    public class DonacionApplicationService_380_jh
    {
        private readonly DonacionRepository_380_jh _donacionRepository_380_jh;
        private readonly AutorizacionApplicationService_380_jh _autorizacionService_380_jh;
        private readonly BitacoraApplicationService_380_jh _bitacoraService_380_jh;

        public DonacionApplicationService_380_jh()
            : this(new DonacionRepository_380_jh(), new AutorizacionApplicationService_380_jh(), new BitacoraApplicationService_380_jh())
        {
        }

        public DonacionApplicationService_380_jh(
            DonacionRepository_380_jh donacionRepository,
            AutorizacionApplicationService_380_jh autorizacionService,
            BitacoraApplicationService_380_jh bitacoraService)
        {
            _donacionRepository_380_jh = donacionRepository;
            _autorizacionService_380_jh = autorizacionService;
            _bitacoraService_380_jh = bitacoraService;
        }

        public CodigoOperacionPN1_380_jh Registrar_380_jh(Donacion_380_jh donacion)
        {
            Usuario_380_jh usuario = Sesion_380_jh.ObtenerInstancia_380_jh().ObtenerUsuario_380_jh();
            if (!_autorizacionService_380_jh.TienePermiso_380_jh(usuario, PermisosSistema_380_jh.DonacionCrear_380_jh))
            {
                RegistrarFalla_380_jh(usuario, "Intento no autorizado de registrar una donación.");
                return CodigoOperacionPN1_380_jh.NoAutorizado_380_jh;
            }

            if (donacion == null || donacion.IdDonante_380_jh <= 0 || donacion.CantidadUnidades_380_jh <= 0 ||
                string.IsNullOrWhiteSpace(donacion.TipoComponente_380_jh) || donacion.FechaDonacion_380_jh == default(System.DateTime) ||
                donacion.FechaVencimiento_380_jh.Date < donacion.FechaDonacion_380_jh.Date)
            {
                RegistrarFalla_380_jh(usuario, "Registro de donación rechazado por datos inválidos.");
                return CodigoOperacionPN1_380_jh.DatosInvalidos_380_jh;
            }

            donacion.TipoComponente_380_jh = donacion.TipoComponente_380_jh.Trim();
            donacion.Observaciones_380_jh = string.IsNullOrWhiteSpace(donacion.Observaciones_380_jh)
                ? null
                : donacion.Observaciones_380_jh.Trim();
            donacion.IdUsuarioResponsable_380_jh = usuario.Id_380_jh;

            if (!new IntegridadApplicationService_380_jh().Verificar_380_jh().EsValida_380_jh)
            {
                RegistrarFalla_380_jh(usuario, "Operacion bloqueada por integridad PN1.");
                return CodigoOperacionPN1_380_jh.IntegridadInvalida_380_jh;
            }

            CodigoOperacionPN1_380_jh resultado = _donacionRepository_380_jh.Registrar_380_jh(donacion, usuario.Id_380_jh);
            if (resultado == CodigoOperacionPN1_380_jh.Ok_380_jh)
            {
                _bitacoraService_380_jh.RegistrarEvento_380_jh(
                    BitacoraModulo_380_jh.BancoSangre_380_jh,
                    BitacoraAccion_380_jh.DonacionRegistrada_380_jh,
                    BitacoraNivel_380_jh.Informacion_380_jh,
                    usuario,
                    "Donación registrada con " + donacion.Unidades_380_jh.Count + " unidad(es).");
                _bitacoraService_380_jh.RegistrarEvento_380_jh(
                    BitacoraModulo_380_jh.BancoSangre_380_jh,
                    BitacoraAccion_380_jh.UnidadesGeneradas_380_jh,
                    BitacoraNivel_380_jh.Informacion_380_jh,
                    usuario,
                    "Se generaron " + donacion.Unidades_380_jh.Count + " unidad(es) en revisión.");
            }
            else
            {
                RegistrarFalla_380_jh(usuario, "No se pudo registrar la donación.");
            }

            if (resultado == CodigoOperacionPN1_380_jh.IntegridadInvalida_380_jh)
                new IntegridadApplicationService_380_jh().Verificar_380_jh();
            return resultado;
        }

        private void RegistrarFalla_380_jh(Usuario_380_jh usuario, string descripcion)
        {
            _bitacoraService_380_jh.RegistrarEvento_380_jh(
                BitacoraModulo_380_jh.BancoSangre_380_jh,
                BitacoraAccion_380_jh.OperacionFallida_380_jh,
                BitacoraNivel_380_jh.Advertencia_380_jh,
                usuario,
                descripcion);
        }
    }
}
