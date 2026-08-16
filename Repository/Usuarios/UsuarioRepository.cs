using System.Collections.Generic;
using DAL;
using Domain;

namespace Repository
{
    public class UsuarioRepository_380_jh
    {
        private readonly UsuarioDataMapper_380_jh _usuarioDataMapper_380_jh;

        public UsuarioRepository_380_jh()
            : this(new UsuarioDataMapper_380_jh())
        {
        }

        public UsuarioRepository_380_jh(UsuarioDataMapper_380_jh usuarioDataMapper)
        {
            _usuarioDataMapper_380_jh = usuarioDataMapper;
        }

        public CodigoRegistroUsuario_380_jh Crear_380_jh(Usuario_380_jh usuario)
        {
            if (usuario == null)
            {
                return CodigoRegistroUsuario_380_jh.DatosInvalidos_380_jh;
            }

            return _usuarioDataMapper_380_jh.Insertar_380_jh(usuario);
        }

        public Usuario_380_jh ObtenerPorCredenciales_380_jh(string username, string password)
        {
            return _usuarioDataMapper_380_jh.ObtenerPorCredenciales_380_jh(username, password);
        }

        public bool Existe_380_jh(string username)
        {
            return _usuarioDataMapper_380_jh.ExistePorNombreUsuario_380_jh(username);
        }

        public Usuario_380_jh ObtenerActivoPorIdentificador_380_jh(string identificador)
        {
            return _usuarioDataMapper_380_jh.ObtenerActivoPorIdentificador_380_jh(identificador);
        }

        public Usuario_380_jh ObtenerPorId_380_jh(int id)
        {
            if (id == 0)
            {
                return null;
            }

            return _usuarioDataMapper_380_jh.ObtenerPorId_380_jh(id);
        }

        public bool EstaBloqueadoPorIdentificador_380_jh(string identificador)
        {
            return _usuarioDataMapper_380_jh.EstaBloqueadoPorIdentificador_380_jh(identificador);
        }

        public int RegistrarLoginFallidoPorIdentificador_380_jh(string identificador)
        {
            return _usuarioDataMapper_380_jh.RegistrarLoginFallidoPorIdentificador_380_jh(identificador);
        }

        public bool ReiniciarIntentosLoginFallidos_380_jh(int idUsuario)
        {
            if (idUsuario == 0)
            {
                return false;
            }

            return _usuarioDataMapper_380_jh.ReiniciarIntentosLoginFallidos_380_jh(idUsuario) >= 0;
        }

        public void Guardar_380_jh(Usuario_380_jh usuario)
        {
            if (usuario == null)
            {
                return;
            }

            if (usuario.Id_380_jh == 0)
            {
                _usuarioDataMapper_380_jh.Insertar_380_jh(usuario);
            }
            else
            {
                _usuarioDataMapper_380_jh.Editar_380_jh(usuario);
            }
        }

        public bool Modificar_380_jh(Usuario_380_jh usuario)
        {
            if (usuario == null || usuario.Id_380_jh == 0)
            {
                return false;
            }

            return _usuarioDataMapper_380_jh.Editar_380_jh(usuario) > 0;
        }

        public bool ActualizarIdiomaPreferido_380_jh(int usuarioId, int idiomaId)
        {
            if (usuarioId == 0 || idiomaId == 0)
            {
                return false;
            }

            bool actualizado = _usuarioDataMapper_380_jh.ActualizarIdiomaPreferido_380_jh(usuarioId, idiomaId) > 0;

            if (actualizado)
            {
                new DigitoVerificadorDataMapper_380_jh().RecalcularUsuarios_380_jh();
            }

            return actualizado;
        }

        public bool RestaurarCampo_380_jh(int idUsuario, string campo, object valor)
        {
            if (idUsuario == 0)
            {
                return false;
            }

            return _usuarioDataMapper_380_jh.RestaurarCampo_380_jh(idUsuario, campo, valor) > 0;
        }

        public void Borrar_380_jh(Usuario_380_jh usuario)
        {
            if (usuario == null)
            {
                return;
            }

            _usuarioDataMapper_380_jh.Borrar_380_jh(usuario);
        }

        public bool Inhabilitar_380_jh(Usuario_380_jh usuario)
        {
            if (usuario == null || usuario.Id_380_jh == 0)
            {
                return false;
            }

            return _usuarioDataMapper_380_jh.Borrar_380_jh(usuario) > 0;
        }

        public List<Usuario_380_jh> Listar_380_jh()
        {
            return _usuarioDataMapper_380_jh.Listar_380_jh();
        }

    }
}
