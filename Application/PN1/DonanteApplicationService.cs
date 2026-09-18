using System.Collections.Generic;
using Abstractions;
using Domain;
using Repository;

namespace Application
{
    public class DonanteApplicationService_380_jh
    {
        private readonly DonanteRepository_380_jh _donanteRepository_380_jh;
        private readonly AutorizacionApplicationService_380_jh _autorizacionService_380_jh;
        private readonly BitacoraApplicationService_380_jh _bitacoraService_380_jh;

        public DonanteApplicationService_380_jh()
            : this(new DonanteRepository_380_jh(), new AutorizacionApplicationService_380_jh(), new BitacoraApplicationService_380_jh())
        {
        }

        public DonanteApplicationService_380_jh(
            DonanteRepository_380_jh donanteRepository,
            AutorizacionApplicationService_380_jh autorizacionService,
            BitacoraApplicationService_380_jh bitacoraService)
        {
            _donanteRepository_380_jh = donanteRepository;
            _autorizacionService_380_jh = autorizacionService;
            _bitacoraService_380_jh = bitacoraService;
        }

        public List<Donante_380_jh> Listar_380_jh(string buscar = null)
        {
            if (!_autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.DonanteVer_380_jh) &&
                !_autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.DonanteCrear_380_jh))
            {
                return new List<Donante_380_jh>();
            }

            return _donanteRepository_380_jh.Listar_380_jh(Normalizar_380_jh(buscar));
        }

        public CodigoOperacionPN1_380_jh Registrar_380_jh(Donante_380_jh donante)
        {
            Usuario_380_jh usuario = Sesion_380_jh.ObtenerInstancia_380_jh().ObtenerUsuario_380_jh();
            if (!_autorizacionService_380_jh.TienePermiso_380_jh(usuario, PermisosSistema_380_jh.DonanteCrear_380_jh))
            {
                RegistrarFalla_380_jh(usuario, "Intento no autorizado de registrar un donante.");
                return CodigoOperacionPN1_380_jh.NoAutorizado_380_jh;
            }

            if (donante == null || string.IsNullOrWhiteSpace(donante.Documento_380_jh) ||
                string.IsNullOrWhiteSpace(donante.Nombre_380_jh) || string.IsNullOrWhiteSpace(donante.Apellido_380_jh))
            {
                RegistrarFalla_380_jh(usuario, "Registro de donante rechazado por datos inválidos.");
                return CodigoOperacionPN1_380_jh.DatosInvalidos_380_jh;
            }

            Normalizar_380_jh(donante);
            if (!new IntegridadApplicationService_380_jh().Verificar_380_jh().EsValida_380_jh)
            {
                RegistrarFalla_380_jh(usuario, "Operacion bloqueada por integridad PN1.");
                return CodigoOperacionPN1_380_jh.IntegridadInvalida_380_jh;
            }

            CodigoOperacionPN1_380_jh resultado = _donanteRepository_380_jh.Registrar_380_jh(donante, usuario.Id_380_jh);
            if (resultado == CodigoOperacionPN1_380_jh.Ok_380_jh)
            {
                _bitacoraService_380_jh.RegistrarEvento_380_jh(
                    BitacoraModulo_380_jh.BancoSangre_380_jh,
                    BitacoraAccion_380_jh.DonanteRegistrado_380_jh,
                    BitacoraNivel_380_jh.Informacion_380_jh,
                    usuario,
                    "Donante registrado: " + donante.Documento_380_jh + ".");
            }
            else
            {
                RegistrarFalla_380_jh(usuario, "No se pudo registrar el donante " + donante.Documento_380_jh + ".");
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

        private static string Normalizar_380_jh(string texto)
        {
            return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
        }

        private static void Normalizar_380_jh(Donante_380_jh donante)
        {
            donante.Documento_380_jh = Normalizar_380_jh(donante.Documento_380_jh);
            donante.Nombre_380_jh = Normalizar_380_jh(donante.Nombre_380_jh);
            donante.Apellido_380_jh = Normalizar_380_jh(donante.Apellido_380_jh);
            donante.Telefono_380_jh = Normalizar_380_jh(donante.Telefono_380_jh);
            donante.Email_380_jh = Normalizar_380_jh(donante.Email_380_jh);
            donante.Domicilio_380_jh = Normalizar_380_jh(donante.Domicilio_380_jh);
        }
    }
}
