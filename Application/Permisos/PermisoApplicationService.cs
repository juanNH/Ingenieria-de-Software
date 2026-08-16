using System.Collections.Generic;
using System.Text;
using Domain;
using Repository;

namespace Application
{
    public class PermisoApplicationService_380_jh
    {
        private readonly PermisoRepository_380_jh _permisoRepository_380_jh;
        private readonly AuditoriaApplicationService_380_jh _auditoriaService_380_jh;

        public PermisoApplicationService_380_jh()
            : this(new PermisoRepository_380_jh(), new AuditoriaApplicationService_380_jh())
        {
        }

        public PermisoApplicationService_380_jh(PermisoRepository_380_jh permisoRepository)
            : this(permisoRepository, new AuditoriaApplicationService_380_jh())
        {
        }

        public PermisoApplicationService_380_jh(PermisoRepository_380_jh permisoRepository, AuditoriaApplicationService_380_jh auditoriaService)
        {
            _permisoRepository_380_jh = permisoRepository;
            _auditoriaService_380_jh = auditoriaService;
        }

        public List<ComponentePermiso_380_jh> ListarAsignadosPorUsuario_380_jh(int idUsuario)
        {
            return _permisoRepository_380_jh.ListarAsignadosPorUsuario_380_jh(idUsuario);
        }

        public List<ComponentePermiso_380_jh> ListarArbolCompleto_380_jh()
        {
            return _permisoRepository_380_jh.ListarArbolCompleto_380_jh();
        }

        public List<ComponentePermiso_380_jh> ListarComponentes_380_jh()
        {
            return _permisoRepository_380_jh.ListarComponentes_380_jh();
        }

        public List<ComponentePermiso_380_jh> ListarFamilias_380_jh()
        {
            return _permisoRepository_380_jh.ListarFamilias_380_jh();
        }

        public List<int> ListarIdsComponentesAsignadosPorUsuario_380_jh(int idUsuario)
        {
            return _permisoRepository_380_jh.ListarIdsComponentesAsignadosPorUsuario_380_jh(idUsuario);
        }

        public bool CrearFamilia_380_jh(string codigo, string nombre, string descripcion)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return false;
            }

            string codigoNormalizado = NormalizarCodigo_380_jh(string.IsNullOrWhiteSpace(codigo) ? nombre : codigo);
            if (string.IsNullOrWhiteSpace(codigoNormalizado))
            {
                return false;
            }

            ComponentePermiso_380_jh componenteAnterior = BuscarComponentePorCodigo_380_jh(codigoNormalizado);
            bool guardado = _permisoRepository_380_jh.CrearFamilia_380_jh(codigoNormalizado, nombre.Trim(), descripcion);

            if (guardado)
            {
                ComponentePermiso_380_jh componenteNuevo = BuscarComponentePorCodigo_380_jh(codigoNormalizado);
                if (componenteNuevo != null)
                {
                    if (componenteAnterior == null)
                    {
                        _auditoriaService_380_jh.RegistrarAlta_380_jh(componenteNuevo.SaveToMemento_380_jh());
                    }
                    else
                    {
                        _auditoriaService_380_jh.RegistrarModificacion_380_jh(componenteAnterior.SaveToMemento_380_jh(), componenteNuevo.SaveToMemento_380_jh());
                    }
                }
            }

            return guardado;
        }

        public string AgregarRelacion_380_jh(int idPadre, int idHijo)
        {
            if (idPadre == 0 || idHijo == 0)
            {
                return "DATOS_INVALIDOS";
            }

            string resultado = _permisoRepository_380_jh.AgregarRelacion_380_jh(idPadre, idHijo);
            if (resultado == "OK")
            {
                _auditoriaService_380_jh.RegistrarSnapshot_380_jh(
                    "ComponentePermisoRelacion",
                    CrearIdRelacion_380_jh(idPadre, idHijo),
                    "CREATE",
                    CrearEstadoRelacion_380_jh(idPadre, idHijo));
            }

            return resultado;
        }

        public bool QuitarRelacion_380_jh(int idPadre, int idHijo)
        {
            if (idPadre == 0 || idHijo == 0)
            {
                return false;
            }

            bool quitada = _permisoRepository_380_jh.QuitarRelacion_380_jh(idPadre, idHijo);
            if (quitada)
            {
                _auditoriaService_380_jh.RegistrarSnapshot_380_jh(
                    "ComponentePermisoRelacion",
                    CrearIdRelacion_380_jh(idPadre, idHijo),
                    "DELETE",
                    CrearEstadoRelacion_380_jh(idPadre, idHijo));
            }

            return quitada;
        }

        public bool GuardarComponentesUsuario_380_jh(int idUsuario, List<int> idsComponentes)
        {
            if (idUsuario == 0)
            {
                return false;
            }

            List<int> idsAnteriores = _permisoRepository_380_jh.ListarIdsComponentesAsignadosPorUsuario_380_jh(idUsuario);
            bool guardado = _permisoRepository_380_jh.GuardarComponentesUsuario_380_jh(idUsuario, idsComponentes);

            if (guardado)
            {
                List<int> idsNuevos = _permisoRepository_380_jh.ListarIdsComponentesAsignadosPorUsuario_380_jh(idUsuario);
                _auditoriaService_380_jh.RegistrarCambio_380_jh(
                    CrearMementoComponentesUsuario_380_jh(idUsuario, idsAnteriores),
                    CrearMementoComponentesUsuario_380_jh(idUsuario, idsNuevos),
                    "UPDATE");
            }

            return guardado;
        }

        public bool PuedeAgregarRelacion_380_jh(int idPadre, int idHijo)
        {
            return _permisoRepository_380_jh.PuedeAgregarRelacion_380_jh(idPadre, idHijo);
        }

        private static string NormalizarCodigo_380_jh(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();

            foreach (char caracter in texto.Trim().ToUpperInvariant())
            {
                if (char.IsLetterOrDigit(caracter))
                {
                    builder.Append(caracter);
                }
                else if (builder.Length > 0 && builder[builder.Length - 1] != '_')
                {
                    builder.Append('_');
                }
            }

            return builder.ToString().Trim('_');
        }

        private ComponentePermiso_380_jh BuscarComponentePorCodigo_380_jh(string codigo)
        {
            foreach (ComponentePermiso_380_jh componente in _permisoRepository_380_jh.ListarComponentes_380_jh())
            {
                if (componente.Codigo_380_jh == codigo)
                {
                    return componente;
                }
            }

            return null;
        }

        private static int CrearIdRelacion_380_jh(int idPadre, int idHijo)
        {
            return (idPadre * 100000) + idHijo;
        }

        private static Dictionary<string, object> CrearEstadoRelacion_380_jh(int idPadre, int idHijo)
        {
            return new Dictionary<string, object>
            {
                { "IdPadre", idPadre },
                { "IdHijo", idHijo }
            };
        }

        private static AuditoriaMemento_380_jh CrearMementoComponentesUsuario_380_jh(int idUsuario, List<int> idsComponentes)
        {
            List<int> idsOrdenados = idsComponentes == null
                ? new List<int>()
                : new List<int>(idsComponentes);
            idsOrdenados.Sort();

            return new AuditoriaMemento_380_jh("UsuarioComponentePermiso", idUsuario, new Dictionary<string, object>
            {
                { "IdUsuario", idUsuario },
                { "IdsComponentes", string.Join(",", idsOrdenados) }
            });
        }
    }
}
