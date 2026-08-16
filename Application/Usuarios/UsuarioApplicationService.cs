using System.Collections.Generic;
using System.Net.Mail;
using System.Web.Script.Serialization;
using Domain;
using Repository;
using Services;

namespace Application
{
    public class UsuarioApplicationService_380_jh
    {
        private readonly UsuarioRepository_380_jh _usuarioRepository_380_jh;
        private readonly PermisoRepository_380_jh _permisoRepository_380_jh;
        private readonly BitacoraRepository_380_jh _bitacoraRepository_380_jh;
        private readonly AuditoriaApplicationService_380_jh _auditoriaService_380_jh;
        private readonly DigitoVerificadorApplicationService_380_jh _digitoVerificadorService_380_jh;
        private readonly PlainTextPasswordService_380_jh _passwordService_380_jh;
        private readonly JavaScriptSerializer _serializer_380_jh;
        private BitacoraFactory_380_jh _bitacoraFactory_380_jh;

        public UsuarioApplicationService_380_jh()
            : this(new UsuarioRepository_380_jh(), new PermisoRepository_380_jh(), new BitacoraRepository_380_jh(), new AuditoriaApplicationService_380_jh(), new DigitoVerificadorApplicationService_380_jh(), new PlainTextPasswordService_380_jh())
        {
        }

        public UsuarioApplicationService_380_jh(
            UsuarioRepository_380_jh usuarioRepository,
            PermisoRepository_380_jh permisoRepository,
            BitacoraRepository_380_jh bitacoraRepository,
            PlainTextPasswordService_380_jh passwordService)
            : this(usuarioRepository, permisoRepository, bitacoraRepository, new AuditoriaApplicationService_380_jh(), new DigitoVerificadorApplicationService_380_jh(), passwordService)
        {
        }

        public UsuarioApplicationService_380_jh(
            UsuarioRepository_380_jh usuarioRepository,
            PermisoRepository_380_jh permisoRepository,
            BitacoraRepository_380_jh bitacoraRepository,
            AuditoriaApplicationService_380_jh auditoriaService,
            PlainTextPasswordService_380_jh passwordService)
            : this(usuarioRepository, permisoRepository, bitacoraRepository, auditoriaService, new DigitoVerificadorApplicationService_380_jh(), passwordService)
        {
        }

        public UsuarioApplicationService_380_jh(
            UsuarioRepository_380_jh usuarioRepository,
            PermisoRepository_380_jh permisoRepository,
            BitacoraRepository_380_jh bitacoraRepository,
            AuditoriaApplicationService_380_jh auditoriaService,
            DigitoVerificadorApplicationService_380_jh digitoVerificadorService,
            PlainTextPasswordService_380_jh passwordService)
        {
            _usuarioRepository_380_jh = usuarioRepository;
            _permisoRepository_380_jh = permisoRepository;
            _bitacoraRepository_380_jh = bitacoraRepository;
            _auditoriaService_380_jh = auditoriaService;
            _digitoVerificadorService_380_jh = digitoVerificadorService;
            _passwordService_380_jh = passwordService;
            _serializer_380_jh = new JavaScriptSerializer();
        }

        //Registrar el usuario
        public CodigoRegistroUsuario_380_jh CrearUsuario_380_jh(Usuario_380_jh nuevoUsuario)
        {
            if (nuevoUsuario == null ||
                string.IsNullOrWhiteSpace(nuevoUsuario.Username_380_jh) ||
                string.IsNullOrWhiteSpace(nuevoUsuario.Email_380_jh) ||
                string.IsNullOrWhiteSpace(nuevoUsuario.Password_380_jh))
            {
                return CodigoRegistroUsuario_380_jh.DatosInvalidos_380_jh;
            }

            if (!EsFormatoEmailValido_380_jh(nuevoUsuario.Email_380_jh))
            {
                return CodigoRegistroUsuario_380_jh.EmailInvalido_380_jh;
            }

            //Aplicacion del hash
            Usuario_380_jh usuarioProtegido = new Usuario_380_jh
            {
                Id_380_jh = nuevoUsuario.Id_380_jh,
                Username_380_jh = nuevoUsuario.Username_380_jh.Trim(),
                Email_380_jh = nuevoUsuario.Email_380_jh.Trim(),
                Password_380_jh = _passwordService_380_jh.Hash_380_jh(nuevoUsuario.Password_380_jh),
                Nombre_380_jh = string.IsNullOrWhiteSpace(nuevoUsuario.Nombre_380_jh) ? null : nuevoUsuario.Nombre_380_jh.Trim(),
                Apellido_380_jh = string.IsNullOrWhiteSpace(nuevoUsuario.Apellido_380_jh) ? null : nuevoUsuario.Apellido_380_jh.Trim(),
                Idioma_380_jh = nuevoUsuario.Idioma_380_jh,
                IdiomaPreferidoId_380_jh = nuevoUsuario.IdiomaPreferidoId_380_jh,
                Estado_380_jh = "ACTIVO"
            };

            //Registro de la falla al ingresar REGISTRO
            CodigoRegistroUsuario_380_jh resultado = _usuarioRepository_380_jh.Crear_380_jh(usuarioProtegido);
            this._bitacoraFactory_380_jh = new RegistroFallidoBitacoraFactory_380_jh();

            if (resultado == CodigoRegistroUsuario_380_jh.Creado_380_jh)
            {
                _digitoVerificadorService_380_jh.RecalcularUsuarios_380_jh();
                _auditoriaService_380_jh.RegistrarAlta_380_jh(usuarioProtegido.CrearMemento_380_jh());
            }

            if (resultado == CodigoRegistroUsuario_380_jh.UsuarioExistente_380_jh)
            {
                RegistrarRegistroFallido_380_jh(usuarioProtegido.Username_380_jh, "Intento de registro con usuario existente.");
            }
            else if (resultado == CodigoRegistroUsuario_380_jh.EmailExistente_380_jh)
            {
                RegistrarRegistroFallido_380_jh(usuarioProtegido.Email_380_jh, "Intento de registro con email existente.");
            }

            return resultado;
        }

        //LOGIN registro de falla
        public Usuario_380_jh Login_380_jh(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            string identificador = username.Trim();
            string passwordProtegida = _passwordService_380_jh.Hash_380_jh(password);
            bool integridadUsuariosValida = _digitoVerificadorService_380_jh.VerificarUsuarios_380_jh();
            Usuario_380_jh usuario = _usuarioRepository_380_jh.ObtenerPorCredenciales_380_jh(identificador, passwordProtegida);

            if (usuario == null)
            {
                if (integridadUsuariosValida)
                {
                    Usuario_380_jh usuarioExistente = _usuarioRepository_380_jh.ObtenerActivoPorIdentificador_380_jh(identificador);

                    if (usuarioExistente != null)
                    {
                        int intentosFallidos = _usuarioRepository_380_jh.RegistrarLoginFallidoPorIdentificador_380_jh(identificador);
                        _digitoVerificadorService_380_jh.RecalcularUsuarioYDvv_380_jh(usuarioExistente.Id_380_jh);
                        this._bitacoraFactory_380_jh = new LoginFallidoBitacoraFactory_380_jh();
                        string descripcion = intentosFallidos >= 3
                            ? "Intento de login con contrasena incorrecta. Usuario deshabilitado por alcanzar 3 intentos fallidos."
                            : "Intento de login con contrasena incorrecta.";

                        RegistrarLoginFallido_380_jh(usuarioExistente, descripcion);
                    }
                }

                return null;
            }

            usuario.ComponentesPermiso_380_jh = _permisoRepository_380_jh.ListarAsignadosPorUsuario_380_jh(usuario.Id_380_jh);

            if (!integridadUsuariosValida && !EsAdministrador_380_jh(usuario))
            {
                return null;
            }

            _usuarioRepository_380_jh.ReiniciarIntentosLoginFallidos_380_jh(usuario.Id_380_jh);
            _digitoVerificadorService_380_jh.RecalcularUsuarioYDvv_380_jh(usuario.Id_380_jh);
            usuario.IntentosLoginFallidos_380_jh = 0;

            this._bitacoraFactory_380_jh = new LoginExitosoBitacoraFactory_380_jh();
            RegistrarLoginExitoso_380_jh(usuario, "Login exitoso.");
            return usuario;
        }

        public bool ExisteUsuario_380_jh(string username)
        {
            return _usuarioRepository_380_jh.Existe_380_jh(username);
        }

        public bool EstaBloqueado_380_jh(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            return _usuarioRepository_380_jh.EstaBloqueadoPorIdentificador_380_jh(username.Trim());
        }

        public void Grabar_380_jh(Usuario_380_jh usuario)
        {
            _usuarioRepository_380_jh.Guardar_380_jh(usuario);
            _digitoVerificadorService_380_jh.RecalcularUsuarios_380_jh();
        }

        public bool ModificarUsuario_380_jh(Usuario_380_jh usuario)
        {
            if (usuario == null ||
                usuario.Id_380_jh == 0 ||
                string.IsNullOrWhiteSpace(usuario.Username_380_jh) ||
                string.IsNullOrWhiteSpace(usuario.Email_380_jh))
            {
                return false;
            }

            if (!EsFormatoEmailValido_380_jh(usuario.Email_380_jh))
            {
                return false;
            }

            Usuario_380_jh usuarioNormalizado = new Usuario_380_jh
            {
                Id_380_jh = usuario.Id_380_jh,
                Username_380_jh = usuario.Username_380_jh.Trim(),
                Email_380_jh = usuario.Email_380_jh.Trim(),
                Password_380_jh = string.IsNullOrWhiteSpace(usuario.Password_380_jh) ? null : _passwordService_380_jh.Hash_380_jh(usuario.Password_380_jh),
                Nombre_380_jh = string.IsNullOrWhiteSpace(usuario.Nombre_380_jh) ? null : usuario.Nombre_380_jh.Trim(),
                Apellido_380_jh = string.IsNullOrWhiteSpace(usuario.Apellido_380_jh) ? null : usuario.Apellido_380_jh.Trim(),
                Idioma_380_jh = usuario.Idioma_380_jh,
                IdiomaPreferidoId_380_jh = usuario.IdiomaPreferidoId_380_jh,
                Estado_380_jh = string.IsNullOrWhiteSpace(usuario.Estado_380_jh) ? "ACTIVO" : usuario.Estado_380_jh.Trim()
            };

            Usuario_380_jh usuarioAnterior = _usuarioRepository_380_jh.ObtenerPorId_380_jh(usuarioNormalizado.Id_380_jh);

            if (usuarioAnterior == null)
            {
                return false;
            }

            AuditoriaMemento_380_jh estadoAnterior = usuarioAnterior.CrearMemento_380_jh();
            bool modificado = _usuarioRepository_380_jh.Modificar_380_jh(usuarioNormalizado);

            if (!modificado)
            {
                return false;
            }

            _digitoVerificadorService_380_jh.RecalcularUsuarios_380_jh();
            Usuario_380_jh usuarioPosterior = _usuarioRepository_380_jh.ObtenerPorId_380_jh(usuarioNormalizado.Id_380_jh);

            if (usuarioPosterior != null)
            {
                _auditoriaService_380_jh.RegistrarModificacion_380_jh(estadoAnterior, usuarioPosterior.CrearMemento_380_jh());
            }

            return true;
        }

        public void Borrar_380_jh(Usuario_380_jh usuario)
        {
            _usuarioRepository_380_jh.Borrar_380_jh(usuario);
            _digitoVerificadorService_380_jh.RecalcularUsuarios_380_jh();
        }

        public bool InhabilitarUsuario_380_jh(Usuario_380_jh usuario)
        {
            if (usuario == null || usuario.Id_380_jh == 0)
            {
                return false;
            }

            Usuario_380_jh usuarioAnterior = _usuarioRepository_380_jh.ObtenerPorId_380_jh(usuario.Id_380_jh);
            bool inhabilitado = _usuarioRepository_380_jh.Inhabilitar_380_jh(usuario);

            if (inhabilitado)
            {
                _digitoVerificadorService_380_jh.RecalcularUsuarios_380_jh();
                Usuario_380_jh usuarioPosterior = _usuarioRepository_380_jh.ObtenerPorId_380_jh(usuario.Id_380_jh);
                if (usuarioAnterior != null && usuarioPosterior != null)
                {
                    _auditoriaService_380_jh.RegistrarCambio_380_jh(usuarioAnterior.CrearMemento_380_jh(), usuarioPosterior.CrearMemento_380_jh(), "DISABLE");
                }
            }

            return inhabilitado;
        }

        public List<Usuario_380_jh> Listar_380_jh()
        {
            return _usuarioRepository_380_jh.Listar_380_jh();
        }

        public bool RecalcularDigitosVerificadoresUsuarios_380_jh()
        {
            return _digitoVerificadorService_380_jh.RecalcularUsuarios_380_jh();
        }

        public bool HayBloqueoDigitoVerificador_380_jh()
        {
            return !_digitoVerificadorService_380_jh.VerificarUsuarios_380_jh() ||
                   _digitoVerificadorService_380_jh.HayBloqueoUsuarios_380_jh();
        }

        public bool RestaurarCampoDesdeAuditoria_380_jh(AuditoriaRegistro_380_jh auditoria, string campo)
        {
            if (auditoria == null ||
                auditoria.Entidad_380_jh != "Usuario" ||
                auditoria.IdEntidad_380_jh == 0 ||
                string.IsNullOrWhiteSpace(campo) ||
                string.IsNullOrWhiteSpace(auditoria.EstadoAnteriorJson_380_jh))
            {
                return false;
            }

            Dictionary<string, object> estadoAnterior;
            try
            {
                estadoAnterior = _serializer_380_jh.Deserialize<Dictionary<string, object>>(auditoria.EstadoAnteriorJson_380_jh);
            }
            catch
            {
                return false;
            }

            if (estadoAnterior == null || !estadoAnterior.ContainsKey(campo))
            {
                return false;
            }

            object valorAnterior = estadoAnterior[campo];
            if (!EsCampoRestaurable_380_jh(campo) || !EsValorRestaurableValido_380_jh(campo, valorAnterior))
            {
                return false;
            }

            Usuario_380_jh usuarioAnterior = _usuarioRepository_380_jh.ObtenerPorId_380_jh(auditoria.IdEntidad_380_jh);
            if (usuarioAnterior == null)
            {
                return false;
            }

            AuditoriaMemento_380_jh estadoActual = usuarioAnterior.CrearMemento_380_jh();
            bool restaurado = _usuarioRepository_380_jh.RestaurarCampo_380_jh(auditoria.IdEntidad_380_jh, campo, valorAnterior);
            if (!restaurado)
            {
                return false;
            }

            _digitoVerificadorService_380_jh.RecalcularUsuarios_380_jh();

            Usuario_380_jh usuarioRestaurado = _usuarioRepository_380_jh.ObtenerPorId_380_jh(auditoria.IdEntidad_380_jh);
            if (usuarioRestaurado != null)
            {
                _auditoriaService_380_jh.RegistrarCambio_380_jh(estadoActual, usuarioRestaurado.CrearMemento_380_jh(), "RESTORE_FIELD");
            }

            return true;
        }

        private void RegistrarLoginFallido_380_jh(Usuario_380_jh usuario, string descripcion)
        {
            if (usuario == null)
            {
                return;
            }

            IBitacoraEvento_380_jh evento = _bitacoraFactory_380_jh.Crear_380_jh(NormalizarIdentificador_380_jh(usuario.Username_380_jh), descripcion);
            evento.IdUsuario_380_jh = usuario.Id_380_jh;
            _bitacoraRepository_380_jh.Registrar_380_jh(evento);
        }

        private void RegistrarLoginExitoso_380_jh(Usuario_380_jh usuario, string descripcion)
        {
            if (usuario == null)
            {
                return;
            }

            IBitacoraEvento_380_jh evento = _bitacoraFactory_380_jh.Crear_380_jh(NormalizarIdentificador_380_jh(usuario.Username_380_jh), descripcion);
            evento.IdUsuario_380_jh = usuario.Id_380_jh;
            _bitacoraRepository_380_jh.Registrar_380_jh(evento);
        }

        private void RegistrarRegistroFallido_380_jh(string identificador, string descripcion)
        {
            IBitacoraEvento_380_jh evento = _bitacoraFactory_380_jh.Crear_380_jh(NormalizarIdentificador_380_jh(identificador), descripcion);
            _bitacoraRepository_380_jh.Registrar_380_jh(evento);
        }

        private static string NormalizarIdentificador_380_jh(string username)
        {
            return string.IsNullOrWhiteSpace(username) ? null : username.Trim();
        }

        private static bool EsAdministrador_380_jh(Usuario_380_jh usuario)
        {
            if (usuario == null || usuario.ComponentesPermiso_380_jh == null)
            {
                return false;
            }

            foreach (ComponentePermiso_380_jh componente in usuario.ComponentesPermiso_380_jh)
            {
                if (EsComponenteAdministrador_380_jh(componente))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool EsComponenteAdministrador_380_jh(ComponentePermiso_380_jh componente)
        {
            if (componente == null)
            {
                return false;
            }

            if (componente.Codigo_380_jh == PermisosSistema_380_jh.Administrador_380_jh)
            {
                return true;
            }

            foreach (ComponentePermiso_380_jh hijo in componente.ObtenerHijos_380_jh())
            {
                if (EsComponenteAdministrador_380_jh(hijo))
                {
                    return true;
                }
            }

            return false;
        }

        public bool EsEmailValido_380_jh(string email)
        {
            return EsFormatoEmailValido_380_jh(email);
        }

        private static bool EsFormatoEmailValido_380_jh(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            string emailNormalizado = email.Trim();

            try
            {
                MailAddress direccion = new MailAddress(emailNormalizado);
                return direccion.Address == emailNormalizado;
            }
            catch
            {
                return false;
            }
        }

        private static bool EsCampoRestaurable_380_jh(string campo)
        {
            switch (campo)
            {
                case "Username":
                case "Email":
                case "Nombre":
                case "Apellido":
                case "IdiomaPreferidoId":
                case "Estado":
                case "IntentosLoginFallidos":
                case "BloqueoDigitoVerificador":
                    return true;

                default:
                    return false;
            }
        }

        private static bool EsValorRestaurableValido_380_jh(string campo, object valor)
        {
            switch (campo)
            {
                case "Username":
                    return valor != null && !string.IsNullOrWhiteSpace(valor.ToString());

                case "Email":
                    return valor != null && EsFormatoEmailValido_380_jh(valor.ToString());

                case "Estado":
                    string estado = valor == null ? null : valor.ToString();
                    return estado == "ACTIVO" || estado == "INACTIVO" || estado == "BLOQUEADO";

                default:
                    return true;
            }
        }
    }
}
