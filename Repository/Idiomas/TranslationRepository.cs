using System.Collections.Generic;
using DAL;
using Domain;

namespace Repository
{
    public class TranslationRepository_380_jh
    {
        private readonly TraduccionDataMapper_380_jh _traduccionDataMapper_380_jh;

        public TranslationRepository_380_jh()
            : this(new TraduccionDataMapper_380_jh())
        {
        }

        public TranslationRepository_380_jh(TraduccionDataMapper_380_jh traduccionDataMapper)
        {
            _traduccionDataMapper_380_jh = traduccionDataMapper;
        }

        public int CrearEtiqueta_380_jh(Etiqueta_380_jh etiqueta)
        {
            return _traduccionDataMapper_380_jh.CrearEtiqueta_380_jh(etiqueta);
        }

        public bool GuardarTraduccion_380_jh(Traduccion_380_jh traduccion)
        {
            return _traduccionDataMapper_380_jh.GuardarTraduccion_380_jh(traduccion);
        }

        public string ObtenerTexto_380_jh(string key, int idiomaId)
        {
            return _traduccionDataMapper_380_jh.ObtenerTexto_380_jh(key, idiomaId);
        }

        public Traduccion_380_jh ObtenerTraduccion_380_jh(int etiquetaId, int idiomaId)
        {
            return _traduccionDataMapper_380_jh.ObtenerTraduccion_380_jh(etiquetaId, idiomaId);
        }

        public Dictionary<string, string> ListarPorIdioma_380_jh(int idiomaId)
        {
            return _traduccionDataMapper_380_jh.ListarPorIdioma_380_jh(idiomaId);
        }

        public List<Etiqueta_380_jh> ListarEtiquetas_380_jh()
        {
            return _traduccionDataMapper_380_jh.ListarEtiquetas_380_jh();
        }

        public List<Traduccion_380_jh> ListarTraducciones_380_jh()
        {
            return _traduccionDataMapper_380_jh.ListarTraducciones_380_jh();
        }
    }
}
