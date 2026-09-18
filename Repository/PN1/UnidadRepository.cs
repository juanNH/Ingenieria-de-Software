using System.Collections.Generic;
using DAL;
using Domain;

namespace Repository
{
    public class UnidadRepository_380_jh
    {
        private readonly UnidadDataMapper_380_jh _unidadDataMapper_380_jh;

        public UnidadRepository_380_jh()
            : this(new UnidadDataMapper_380_jh())
        {
        }

        public UnidadRepository_380_jh(UnidadDataMapper_380_jh unidadDataMapper)
        {
            _unidadDataMapper_380_jh = unidadDataMapper;
        }

        public List<Unidad_380_jh> Listar_380_jh(string buscar)
        {
            return _unidadDataMapper_380_jh.Listar_380_jh(buscar);
        }

        public CodigoOperacionPN1_380_jh Clasificar_380_jh(Unidad_380_jh unidad, int idUsuario)
        {
            return _unidadDataMapper_380_jh.Clasificar_380_jh(unidad, idUsuario);
        }

        public CodigoOperacionPN1_380_jh CambiarEstado_380_jh(Unidad_380_jh unidad, string estado, string observacion, int idUsuario)
        {
            return _unidadDataMapper_380_jh.CambiarEstado_380_jh(unidad, estado, observacion, idUsuario);
        }
    }
}
