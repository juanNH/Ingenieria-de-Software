using System.Collections.Generic;
using Abstractions;
using Domain;
using Repository;

namespace Application
{
    public class UnidadApplicationService_380_jh
    {
        private readonly UnidadRepository_380_jh _unidadRepository_380_jh;
        private readonly AutorizacionApplicationService_380_jh _autorizacionService_380_jh;
        private readonly BitacoraApplicationService_380_jh _bitacoraService_380_jh;

        public UnidadApplicationService_380_jh()
            : this(new UnidadRepository_380_jh(), new AutorizacionApplicationService_380_jh(), new BitacoraApplicationService_380_jh())
        {
        }

        public UnidadApplicationService_380_jh(
            UnidadRepository_380_jh unidadRepository,
            AutorizacionApplicationService_380_jh autorizacionService,
            BitacoraApplicationService_380_jh bitacoraService)
        {
            _unidadRepository_380_jh = unidadRepository;
            _autorizacionService_380_jh = autorizacionService;
            _bitacoraService_380_jh = bitacoraService;
        }

        public List<Unidad_380_jh> Listar_380_jh(string buscar = null)
        {
            if (!_autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UnidadVer_380_jh) &&
                !_autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UnidadClasificar_380_jh) &&
                !_autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UnidadLiberar_380_jh) &&
                !_autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UnidadBloquear_380_jh) &&
                !_autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UnidadDescartar_380_jh))
            {
                return new List<Unidad_380_jh>();
            }

            return _unidadRepository_380_jh.Listar_380_jh(string.IsNullOrWhiteSpace(buscar) ? null : buscar.Trim());
        }

        public CodigoOperacionPN1_380_jh Clasificar_380_jh(Unidad_380_jh unidad)
        {
            Usuario_380_jh usuario = Sesion_380_jh.ObtenerInstancia_380_jh().ObtenerUsuario_380_jh();
            if (!_autorizacionService_380_jh.TienePermiso_380_jh(usuario, PermisosSistema_380_jh.UnidadClasificar_380_jh))
            {
                RegistrarFalla_380_jh(usuario, "Intento no autorizado de clasificar una unidad.");
                return CodigoOperacionPN1_380_jh.NoAutorizado_380_jh;
            }

            if (unidad == null || string.IsNullOrWhiteSpace(unidad.TipoComponente_380_jh) ||
                string.IsNullOrWhiteSpace(unidad.GrupoSanguineo_380_jh) || string.IsNullOrWhiteSpace(unidad.FactorRh_380_jh) ||
                unidad.FechaVencimiento_380_jh.Date < System.DateTime.Today)
            {
                RegistrarFalla_380_jh(usuario, "Clasificación de unidad rechazada por datos inválidos.");
                return CodigoOperacionPN1_380_jh.DatosInvalidos_380_jh;
            }

            unidad.TipoComponente_380_jh = unidad.TipoComponente_380_jh.Trim();
            unidad.GrupoSanguineo_380_jh = unidad.GrupoSanguineo_380_jh.Trim().ToUpperInvariant();
            unidad.FactorRh_380_jh = unidad.FactorRh_380_jh.Trim();
            if (!new IntegridadApplicationService_380_jh().Verificar_380_jh().EsValida_380_jh)
            {
                RegistrarFalla_380_jh(usuario, "Operacion bloqueada por integridad PN1.");
                return CodigoOperacionPN1_380_jh.IntegridadInvalida_380_jh;
            }

            CodigoOperacionPN1_380_jh resultado = _unidadRepository_380_jh.Clasificar_380_jh(unidad, usuario.Id_380_jh);
            if (resultado == CodigoOperacionPN1_380_jh.Ok_380_jh)
            {
                _bitacoraService_380_jh.RegistrarEvento_380_jh(
                    BitacoraModulo_380_jh.BancoSangre_380_jh,
                    BitacoraAccion_380_jh.UnidadClasificada_380_jh,
                    BitacoraNivel_380_jh.Informacion_380_jh,
                    usuario,
                    "Unidad clasificada: " + unidad.CodigoIdentificacion_380_jh + ".");
            }
            else
            {
                RegistrarFalla_380_jh(usuario, "No se pudo clasificar la unidad " + unidad.Id_380_jh + ".");
            }

            if (resultado == CodigoOperacionPN1_380_jh.IntegridadInvalida_380_jh)
                new IntegridadApplicationService_380_jh().Verificar_380_jh();
            return resultado;
        }

        public CodigoOperacionPN1_380_jh CambiarEstado_380_jh(Unidad_380_jh unidad, string estadoNuevo, string observacion)
        {
            Usuario_380_jh usuario = Sesion_380_jh.ObtenerInstancia_380_jh().ObtenerUsuario_380_jh();
            string permiso = ObtenerPermisoEstado_380_jh(estadoNuevo);
            if (string.IsNullOrWhiteSpace(permiso) || !_autorizacionService_380_jh.TienePermiso_380_jh(usuario, permiso))
            {
                RegistrarFalla_380_jh(usuario, "Intento no autorizado de cambiar el estado de una unidad.");
                return CodigoOperacionPN1_380_jh.NoAutorizado_380_jh;
            }

            if (unidad == null || string.IsNullOrWhiteSpace(observacion) &&
                (estadoNuevo == EstadoUnidadPN1_380_jh.Bloqueada_380_jh || estadoNuevo == EstadoUnidadPN1_380_jh.Descartada_380_jh))
            {
                RegistrarFalla_380_jh(usuario, "Cambio de estado rechazado por datos inválidos.");
                return CodigoOperacionPN1_380_jh.DatosInvalidos_380_jh;
            }

            if (!new IntegridadApplicationService_380_jh().Verificar_380_jh().EsValida_380_jh)
            {
                RegistrarFalla_380_jh(usuario, "Operacion bloqueada por integridad PN1.");
                return CodigoOperacionPN1_380_jh.IntegridadInvalida_380_jh;
            }

            CodigoOperacionPN1_380_jh resultado = _unidadRepository_380_jh.CambiarEstado_380_jh(unidad, estadoNuevo, observacion, usuario.Id_380_jh);
            if (resultado == CodigoOperacionPN1_380_jh.Ok_380_jh)
            {
                _bitacoraService_380_jh.RegistrarEvento_380_jh(
                    BitacoraModulo_380_jh.BancoSangre_380_jh,
                    ObtenerAccionEstado_380_jh(estadoNuevo),
                    BitacoraNivel_380_jh.Informacion_380_jh,
                    usuario,
                    "Unidad " + estadoNuevo + ": " + unidad.CodigoIdentificacion_380_jh + ".");
            }
            else
            {
                RegistrarFalla_380_jh(usuario, "No se pudo cambiar el estado de la unidad " + unidad.Id_380_jh + ".");
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

        private static string ObtenerPermisoEstado_380_jh(string estado)
        {
            switch (estado)
            {
                case EstadoUnidadPN1_380_jh.Liberada_380_jh: return PermisosSistema_380_jh.UnidadLiberar_380_jh;
                case EstadoUnidadPN1_380_jh.Bloqueada_380_jh: return PermisosSistema_380_jh.UnidadBloquear_380_jh;
                case EstadoUnidadPN1_380_jh.Descartada_380_jh: return PermisosSistema_380_jh.UnidadDescartar_380_jh;
                default: return null;
            }
        }

        private static BitacoraAccion_380_jh ObtenerAccionEstado_380_jh(string estado)
        {
            switch (estado)
            {
                case EstadoUnidadPN1_380_jh.Liberada_380_jh: return BitacoraAccion_380_jh.UnidadLiberada_380_jh;
                case EstadoUnidadPN1_380_jh.Bloqueada_380_jh: return BitacoraAccion_380_jh.UnidadBloqueada_380_jh;
                default: return BitacoraAccion_380_jh.UnidadDescartada_380_jh;
            }
        }
    }
}
