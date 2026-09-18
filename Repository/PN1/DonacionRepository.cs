using DAL;
using Domain;

namespace Repository
{
    public class DonacionRepository_380_jh
    {
        private readonly DonacionDataMapper_380_jh _donacionDataMapper_380_jh;

        public DonacionRepository_380_jh()
            : this(new DonacionDataMapper_380_jh())
        {
        }

        public DonacionRepository_380_jh(DonacionDataMapper_380_jh donacionDataMapper)
        {
            _donacionDataMapper_380_jh = donacionDataMapper;
        }

        public CodigoOperacionPN1_380_jh Registrar_380_jh(Donacion_380_jh donacion, int idUsuario)
        {
            return _donacionDataMapper_380_jh.Registrar_380_jh(donacion, idUsuario);
        }
    }
}
