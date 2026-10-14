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

        public string ObtenerEmailCifrado_380_jh(int idDonante)
        {
            return _donanteDataMapper_380_jh.ObtenerEmailCifrado_380_jh(idDonante);
        }

        public Dictionary<int, string> ListarEmailsLegados_380_jh()
        {
            return _donanteDataMapper_380_jh.ListarEmailsLegados_380_jh();
        }

        public bool MigrarEmailsLegados_380_jh(string emailsJson, int idUsuario)
        {
            return _donanteDataMapper_380_jh.MigrarEmailsLegados_380_jh(emailsJson, idUsuario);
        }

        public CodigoOperacionPN1_380_jh Registrar_380_jh(Donante_380_jh donante, int idUsuario)
        {
            return _donanteDataMapper_380_jh.Registrar_380_jh(donante, idUsuario);
        }

        public CodigoOperacionPN1_380_jh Actualizar_380_jh(Donante_380_jh donante, string emailCifrado, bool actualizarEmail, int idUsuario)
        {
            return _donanteDataMapper_380_jh.Actualizar_380_jh(donante, emailCifrado, actualizarEmail, idUsuario);
        }
    }
}
