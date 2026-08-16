using System.Collections.Generic;
using DAL;
using Domain;

namespace Repository
{
    public class PermisoRepository_380_jh
    {
        private readonly PermisoDataMapper_380_jh _permisoDataMapper_380_jh;

        public PermisoRepository_380_jh()
            : this(new PermisoDataMapper_380_jh())
        {
        }

        public PermisoRepository_380_jh(PermisoDataMapper_380_jh permisoDataMapper)
        {
            _permisoDataMapper_380_jh = permisoDataMapper;
        }

        public List<ComponentePermiso_380_jh> ListarAsignadosPorUsuario_380_jh(int idUsuario)
        {
            return _permisoDataMapper_380_jh.ListarAsignadosPorUsuario_380_jh(idUsuario);
        }

        public List<ComponentePermiso_380_jh> ListarArbolCompleto_380_jh()
        {
            return _permisoDataMapper_380_jh.ListarArbolCompleto_380_jh();
        }

        public List<ComponentePermiso_380_jh> ListarComponentes_380_jh()
        {
            return _permisoDataMapper_380_jh.ListarComponentes_380_jh();
        }

        public List<ComponentePermiso_380_jh> ListarFamilias_380_jh()
        {
            return _permisoDataMapper_380_jh.ListarFamilias_380_jh();
        }

        public List<int> ListarIdsComponentesAsignadosPorUsuario_380_jh(int idUsuario)
        {
            return _permisoDataMapper_380_jh.ListarIdsComponentesAsignadosPorUsuario_380_jh(idUsuario);
        }

        public bool CrearFamilia_380_jh(string codigo, string nombre, string descripcion)
        {
            return _permisoDataMapper_380_jh.CrearFamilia_380_jh(codigo, nombre, descripcion);
        }

        public string AgregarRelacion_380_jh(int idPadre, int idHijo)
        {
            return _permisoDataMapper_380_jh.AgregarRelacion_380_jh(idPadre, idHijo);
        }

        public bool QuitarRelacion_380_jh(int idPadre, int idHijo)
        {
            return _permisoDataMapper_380_jh.QuitarRelacion_380_jh(idPadre, idHijo);
        }

        public bool GuardarComponentesUsuario_380_jh(int idUsuario, List<int> idsComponentes)
        {
            return _permisoDataMapper_380_jh.GuardarComponentesUsuario_380_jh(idUsuario, idsComponentes);
        }

        public bool PuedeAgregarRelacion_380_jh(int idPadre, int idHijo)
        {
            return _permisoDataMapper_380_jh.PuedeAgregarRelacion_380_jh(idPadre, idHijo);
        }
    }
}
