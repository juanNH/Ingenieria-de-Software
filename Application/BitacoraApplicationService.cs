using System.Collections.Generic;
using Domain;
using Repository;

namespace Application
{
    public class BitacoraApplicationService_380_jh
    {
        private readonly BitacoraRepository_380_jh _bitacoraRepository_380_jh;

        public BitacoraApplicationService_380_jh()
            : this(new BitacoraRepository_380_jh())
        {
        }

        public BitacoraApplicationService_380_jh(BitacoraRepository_380_jh bitacoraRepository)
        {
            _bitacoraRepository_380_jh = bitacoraRepository;
        }

        public List<BitacoraRegistro_380_jh> Listar_380_jh()
        {
            return Listar_380_jh(null);
        }

        public List<BitacoraRegistro_380_jh> Listar_380_jh(BitacoraFiltro_380_jh filtro)
        {
            return _bitacoraRepository_380_jh.Listar_380_jh(filtro);
        }
    }
}
