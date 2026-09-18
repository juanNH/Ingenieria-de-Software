using DAL;
using Domain;

namespace Repository
{
    public class IntegridadRepository_380_jh
    {
        private readonly IntegridadDataMapper_380_jh _mapper_380_jh = new IntegridadDataMapper_380_jh();
        public InformeIntegridad_380_jh Verificar_380_jh(int idUsuario) => _mapper_380_jh.Verificar_380_jh(idUsuario);
        public string Recuperar_380_jh(int idUsuario, string revision, bool inicializar)
            => _mapper_380_jh.Recuperar_380_jh(idUsuario, revision, inicializar);
    }
}
