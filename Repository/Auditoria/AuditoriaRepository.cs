using DAL;
using Domain;
using System.Collections.Generic;

namespace Repository
{
    public class AuditoriaRepository_380_jh
    {
        private readonly AuditoriaDataMapper_380_jh _auditoriaDataMapper_380_jh;

        public AuditoriaRepository_380_jh()
            : this(new AuditoriaDataMapper_380_jh())
        {
        }

        public AuditoriaRepository_380_jh(AuditoriaDataMapper_380_jh auditoriaDataMapper)
        {
            _auditoriaDataMapper_380_jh = auditoriaDataMapper;
        }

        public bool Registrar_380_jh(AuditoriaRegistro_380_jh auditoria)
        {
            if (auditoria == null)
            {
                return false;
            }

            return _auditoriaDataMapper_380_jh.Insertar_380_jh(auditoria) > 0;
        }

        public List<AuditoriaRegistro_380_jh> ListarPorEntidad_380_jh(string entidad, int idEntidad)
        {
            if (string.IsNullOrWhiteSpace(entidad) || idEntidad == 0)
            {
                return new List<AuditoriaRegistro_380_jh>();
            }

            return _auditoriaDataMapper_380_jh.ListarPorEntidad_380_jh(entidad.Trim(), idEntidad);
        }

        public List<AuditoriaRegistro_380_jh> ListarTodos_380_jh()
        {
            return _auditoriaDataMapper_380_jh.ListarTodos_380_jh();
        }
    }
}
