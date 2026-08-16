using Repository;

namespace Application
{
    public class DigitoVerificadorApplicationService_380_jh
    {
        private readonly DigitoVerificadorRepository_380_jh _digitoVerificadorRepository_380_jh;

        public DigitoVerificadorApplicationService_380_jh()
            : this(new DigitoVerificadorRepository_380_jh())
        {
        }

        public DigitoVerificadorApplicationService_380_jh(DigitoVerificadorRepository_380_jh digitoVerificadorRepository)
        {
            _digitoVerificadorRepository_380_jh = digitoVerificadorRepository;
        }

        public bool VerificarUsuarios_380_jh()
        {
            return _digitoVerificadorRepository_380_jh.VerificarUsuarios_380_jh();
        }

        public bool RecalcularUsuarios_380_jh()
        {
            return _digitoVerificadorRepository_380_jh.RecalcularUsuarios_380_jh();
        }

        public bool RecalcularUsuarioYDvv_380_jh(int idUsuario)
        {
            return _digitoVerificadorRepository_380_jh.RecalcularUsuarioYDvv_380_jh(idUsuario);
        }

        public bool HayBloqueoUsuarios_380_jh()
        {
            return _digitoVerificadorRepository_380_jh.HayBloqueoUsuarios_380_jh();
        }
    }
}
