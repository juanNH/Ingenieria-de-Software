using System.Collections.Generic;
using Domain;
using Repository;

namespace Services
{
    public class TranslationService_380_jh
    {
        private readonly TranslationRepository_380_jh _translationRepository_380_jh;

        public TranslationService_380_jh()
            : this(new TranslationRepository_380_jh())
        {
        }

        public TranslationService_380_jh(TranslationRepository_380_jh translationRepository)
        {
            _translationRepository_380_jh = translationRepository;
        }

        public string Translate_380_jh(string key, Idioma_380_jh idioma)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            if (idioma == null || idioma.Id_380_jh == 0)
            {
                return key;
            }

            string texto = _translationRepository_380_jh.ObtenerTexto_380_jh(key, idioma.Id_380_jh);
            return string.IsNullOrWhiteSpace(texto) ? key : texto;
        }

        public Dictionary<string, string> ListarPorIdioma_380_jh(Idioma_380_jh idioma)
        {
            if (idioma == null || idioma.Id_380_jh == 0)
            {
                return new Dictionary<string, string>();
            }

            return _translationRepository_380_jh.ListarPorIdioma_380_jh(idioma.Id_380_jh);
        }
    }
}
