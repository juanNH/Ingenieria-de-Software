using DAL;

namespace Repository
{
    public class DigitoVerificadorRepository_380_jh
    {
        private readonly DigitoVerificadorDataMapper_380_jh _digitoVerificadorDataMapper_380_jh;

        public DigitoVerificadorRepository_380_jh()
            : this(new DigitoVerificadorDataMapper_380_jh())
        {
        }

        public DigitoVerificadorRepository_380_jh(DigitoVerificadorDataMapper_380_jh digitoVerificadorDataMapper)
        {
            _digitoVerificadorDataMapper_380_jh = digitoVerificadorDataMapper;
        }

        public bool VerificarUsuarios_380_jh()
        {
            return _digitoVerificadorDataMapper_380_jh.VerificarUsuarios_380_jh();
        }

        public bool RecalcularUsuarios_380_jh()
        {
            return _digitoVerificadorDataMapper_380_jh.RecalcularUsuarios_380_jh();
        }

        public bool RecalcularUsuarioYDvv_380_jh(int idUsuario)
        {
            return _digitoVerificadorDataMapper_380_jh.RecalcularUsuarioYDvv_380_jh(idUsuario);
        }

        public bool HayBloqueoUsuarios_380_jh()
        {
            return _digitoVerificadorDataMapper_380_jh.HayBloqueoUsuarios_380_jh();
        }
    }
}
