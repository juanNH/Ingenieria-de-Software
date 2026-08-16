using System;
using System.Collections.Generic;

namespace Domain
{
    public class AuditoriaMemento_380_jh
    {
        private readonly IReadOnlyDictionary<string, object> _estado_380_jh;

        public AuditoriaMemento_380_jh(string entidad, int idEntidad, IDictionary<string, object> estado)
        {
            Entidad_380_jh = entidad;
            IdEntidad_380_jh = idEntidad;
            FechaCaptura_380_jh = DateTime.Now;
            _estado_380_jh = new Dictionary<string, object>(estado ?? new Dictionary<string, object>());
        }

        public string Entidad_380_jh { get; private set; }
        public int IdEntidad_380_jh { get; private set; }
        public DateTime FechaCaptura_380_jh { get; private set; }
        public IReadOnlyDictionary<string, object> Estado_380_jh
        {
            get { return _estado_380_jh; }
        }

        public IReadOnlyDictionary<string, object> GetSavedMemento_380_jh()
        {
            return _estado_380_jh;
        }
    }
}
