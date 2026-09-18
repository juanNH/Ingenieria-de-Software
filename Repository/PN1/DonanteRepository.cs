using System.Collections.Generic;
using DAL;
using Domain;

namespace Repository
{
    public class DonanteRepository_380_jh
    {
        private readonly DonanteDataMapper_380_jh _donanteDataMapper_380_jh;

        public DonanteRepository_380_jh()
            : this(new DonanteDataMapper_380_jh())
        {
        }

        public DonanteRepository_380_jh(DonanteDataMapper_380_jh donanteDataMapper)
        {
            _donanteDataMapper_380_jh = donanteDataMapper;
        }

        public List<Donante_380_jh> Listar_380_jh(string buscar)
        {
            return _donanteDataMapper_380_jh.Listar_380_jh(buscar);
        }

        public CodigoOperacionPN1_380_jh Registrar_380_jh(Donante_380_jh donante, int idUsuario)
        {
            return _donanteDataMapper_380_jh.Registrar_380_jh(donante, idUsuario);
        }
    }
}
