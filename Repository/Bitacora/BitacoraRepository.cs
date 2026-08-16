using DAL;
using Domain;
using System.Collections.Generic;

namespace Repository
{
    public class BitacoraRepository_380_jh
    {
        private readonly BitacoraDataMapper_380_jh _bitacoraDataMapper_380_jh;

        public BitacoraRepository_380_jh()
            : this(new BitacoraDataMapper_380_jh())
        {
        }

        public BitacoraRepository_380_jh(BitacoraDataMapper_380_jh bitacoraDataMapper)
        {
            _bitacoraDataMapper_380_jh = bitacoraDataMapper;
        }

        public bool Registrar_380_jh(IBitacoraEvento_380_jh bitacora)
        {
            if (bitacora == null)
            {
                return false;
            }

            BitacoraRegistro_380_jh registro = BitacoraRegistroMapper_380_jh.Mapear_380_jh(bitacora);
            return _bitacoraDataMapper_380_jh.Insertar_380_jh(registro) > 0;
        }

        public List<BitacoraRegistro_380_jh> Listar_380_jh()
        {
            return Listar_380_jh(null);
        }

        public List<BitacoraRegistro_380_jh> Listar_380_jh(BitacoraFiltro_380_jh filtro)
        {
            return _bitacoraDataMapper_380_jh.Listar_380_jh(filtro);
        }

    }
}
