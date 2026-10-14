using System.Collections.Generic;
using System.Web.Script.Serialization;
using Abstractions;
using Domain;
using Repository;
using Services;

namespace Application
{
    public class DonanteApplicationService_380_jh
    {
        private readonly DonanteRepository_380_jh _donanteRepository_380_jh;
        private readonly AutorizacionApplicationService_380_jh _autorizacionService_380_jh;
        private readonly BitacoraApplicationService_380_jh _bitacoraService_380_jh;
        private readonly CifradoEmailService_380_jh _cifradoEmailService_380_jh;

        public DonanteApplicationService_380_jh()
            : this(new DonanteRepository_380_jh(), new AutorizacionApplicationService_380_jh(), new BitacoraApplicationService_380_jh(), new CifradoEmailService_380_jh())
        {
        }

        public DonanteApplicationService_380_jh(
            DonanteRepository_380_jh donanteRepository,
            AutorizacionApplicationService_380_jh autorizacionService,
            BitacoraApplicationService_380_jh bitacoraService)
            : this(donanteRepository, autorizacionService, bitacoraService, new CifradoEmailService_380_jh())
        {
        }

        public DonanteApplicationService_380_jh(
            DonanteRepository_380_jh donanteRepository,
            AutorizacionApplicationService_380_jh autorizacionService,
            BitacoraApplicationService_380_jh bitacoraService,
            CifradoEmailService_380_jh cifradoEmailService)
        {
            _donanteRepository_380_jh = donanteRepository;
            _autorizacionService_380_jh = autorizacionService;
            _bitacoraService_380_jh = bitacoraService;
            _cifradoEmailService_380_jh = cifradoEmailService;
        }

        public List<Donante_380_jh> Listar_380_jh(string buscar = null)
        {
            Usuario_380_jh usuario = Sesion_380_jh.ObtenerInstancia_380_jh().ObtenerUsuario_380_jh();
            if (!_autorizacionService_380_jh.TienePermiso_380_jh(usuario, PermisosSistema_380_jh.DonanteVer_380_jh) &&
                !_autorizacionService_380_jh.TienePermiso_380_jh(usuario, PermisosSistema_380_jh.DonanteCrear_380_jh))
            {
                return new List<Donante_380_jh>();
            }

            MigrarEmailsLegados_380_jh(usuario);
            return _donanteRepository_380_jh.Listar_380_jh(Normalizar_380_jh(buscar));
        }

        private void MigrarEmailsLegados_380_jh(Usuario_380_jh usuario)
        {
            Dictionary<int, string> emailsLegados = _donanteRepository_380_jh.ListarEmailsLegados_380_jh();
            if (emailsLegados.Count == 0)
            {
                return;
            }

            try
            {
                List<Dictionary<string, object>> filas = new List<Dictionary<string, object>>();
                foreach (KeyValuePair<int, string> email in emailsLegados)
                {
                    filas.Add(new Dictionary<string, object>
                    {
                        { "id", email.Key },
                        { "email_original", email.Value },
                        { "email", _cifradoEmailService_380_jh.Cifrar_380_jh(email.Value) }
                    });
                }

                string json = new JavaScriptSerializer().Serialize(filas);
                if (!_donanteRepository_380_jh.MigrarEmailsLegados_380_jh(json, usuario.Id_380_jh))
                {
                    RegistrarFalla_380_jh(usuario, "No se pudieron migrar correos de donantes al formato cifrado.");
                }
            }
            catch
            {
                RegistrarFalla_380_jh(usuario, "No se pudieron proteger correos de donantes existentes.");
            }
        }

        public string ObtenerEmail_380_jh(int idDonante)
        {
            Usuario_380_jh usuario = Sesion_380_jh.ObtenerInstancia_380_jh().ObtenerUsuario_380_jh();
            if (idDonante <= 0 ||
                (!_autorizacionService_380_jh.TienePermiso_380_jh(usuario, PermisosSistema_380_jh.DonanteVer_380_jh) &&
                 !_autorizacionService_380_jh.TienePermiso_380_jh(usuario, PermisosSistema_380_jh.DonanteCrear_380_jh)))
            {
                return null;
            }

            string emailCifrado = _donanteRepository_380_jh.ObtenerEmailCifrado_380_jh(idDonante);
            return _cifradoEmailService_380_jh.Descifrar_380_jh(emailCifrado);
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

            Donante_380_jh donanteProtegido = Clonar_380_jh(donante);
            try
            {
                donanteProtegido.Email_380_jh = _cifradoEmailService_380_jh.Cifrar_380_jh(donante.Email_380_jh);
            }
            catch
            {
                RegistrarFalla_380_jh(usuario, "No se pudo proteger el correo del donante.");
                return CodigoOperacionPN1_380_jh.Error_380_jh;
            }

            CodigoOperacionPN1_380_jh resultado = _donanteRepository_380_jh.Registrar_380_jh(donanteProtegido, usuario.Id_380_jh);
            if (resultado == CodigoOperacionPN1_380_jh.Ok_380_jh)
            {
                donante.Id_380_jh = donanteProtegido.Id_380_jh;
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

        public CodigoOperacionPN1_380_jh Actualizar_380_jh(Donante_380_jh donante, bool actualizarEmail)
        {
            Usuario_380_jh usuario = Sesion_380_jh.ObtenerInstancia_380_jh().ObtenerUsuario_380_jh();
            if (!_autorizacionService_380_jh.TienePermiso_380_jh(usuario, PermisosSistema_380_jh.DonanteCrear_380_jh))
            {
                RegistrarFalla_380_jh(usuario, "Intento no autorizado de actualizar un donante.");
                return CodigoOperacionPN1_380_jh.NoAutorizado_380_jh;
            }

            if (donante == null || donante.Id_380_jh <= 0 || string.IsNullOrWhiteSpace(donante.Documento_380_jh) ||
                string.IsNullOrWhiteSpace(donante.Nombre_380_jh) || string.IsNullOrWhiteSpace(donante.Apellido_380_jh))
            {
                RegistrarFalla_380_jh(usuario, "Actualización de donante rechazada por datos inválidos.");
                return CodigoOperacionPN1_380_jh.DatosInvalidos_380_jh;
            }

            Normalizar_380_jh(donante);
            if (!new IntegridadApplicationService_380_jh().Verificar_380_jh().EsValida_380_jh)
            {
                RegistrarFalla_380_jh(usuario, "Actualización bloqueada por integridad PN1.");
                return CodigoOperacionPN1_380_jh.IntegridadInvalida_380_jh;
            }

            string emailCifrado = null;
            if (actualizarEmail)
            {
                try
                {
                    emailCifrado = _cifradoEmailService_380_jh.Cifrar_380_jh(donante.Email_380_jh);
                }
                catch
                {
                    RegistrarFalla_380_jh(usuario, "No se pudo proteger el correo del donante.");
                    return CodigoOperacionPN1_380_jh.Error_380_jh;
                }
            }

            CodigoOperacionPN1_380_jh resultado = _donanteRepository_380_jh.Actualizar_380_jh(
                donante, emailCifrado, actualizarEmail, usuario.Id_380_jh);
            if (resultado == CodigoOperacionPN1_380_jh.Ok_380_jh)
            {
                _bitacoraService_380_jh.RegistrarEvento_380_jh(
                    BitacoraModulo_380_jh.BancoSangre_380_jh,
                    BitacoraAccion_380_jh.DonanteActualizado_380_jh,
                    BitacoraNivel_380_jh.Informacion_380_jh,
                    usuario,
                    "Donante actualizado: " + donante.Documento_380_jh + ".");
            }
            else
            {
                RegistrarFalla_380_jh(usuario, "No se pudo actualizar el donante " + donante.Documento_380_jh + ".");
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

        private static Donante_380_jh Clonar_380_jh(Donante_380_jh donante)
        {
            return new Donante_380_jh
            {
                Id_380_jh = donante.Id_380_jh,
                Documento_380_jh = donante.Documento_380_jh,
                Nombre_380_jh = donante.Nombre_380_jh,
                Apellido_380_jh = donante.Apellido_380_jh,
                FechaNacimiento_380_jh = donante.FechaNacimiento_380_jh,
                Telefono_380_jh = donante.Telefono_380_jh,
                Email_380_jh = donante.Email_380_jh,
                Domicilio_380_jh = donante.Domicilio_380_jh,
                Estado_380_jh = donante.Estado_380_jh,
                FechaAlta_380_jh = donante.FechaAlta_380_jh,
                IdUsuarioAlta_380_jh = donante.IdUsuarioAlta_380_jh
            };
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
