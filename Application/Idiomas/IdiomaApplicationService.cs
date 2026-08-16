using System.Collections.Generic;
using Domain;
using Repository;

namespace Application
{
    public class IdiomaApplicationService_380_jh
    {
        private readonly LanguageRepository_380_jh _languageRepository_380_jh;
        private readonly TranslationRepository_380_jh _translationRepository_380_jh;
        private readonly AuditoriaApplicationService_380_jh _auditoriaService_380_jh;

        public IdiomaApplicationService_380_jh()
            : this(new LanguageRepository_380_jh(), new TranslationRepository_380_jh(), new AuditoriaApplicationService_380_jh())
        {
        }

        public IdiomaApplicationService_380_jh(
            LanguageRepository_380_jh languageRepository,
            TranslationRepository_380_jh translationRepository)
            : this(languageRepository, translationRepository, new AuditoriaApplicationService_380_jh())
        {
        }

        public IdiomaApplicationService_380_jh(
            LanguageRepository_380_jh languageRepository,
            TranslationRepository_380_jh translationRepository,
            AuditoriaApplicationService_380_jh auditoriaService)
        {
            _languageRepository_380_jh = languageRepository;
            _translationRepository_380_jh = translationRepository;
            _auditoriaService_380_jh = auditoriaService;
        }

        public List<Idioma_380_jh> ListarIdiomas_380_jh(bool soloActivos)
        {
            return _languageRepository_380_jh.Listar_380_jh(soloActivos);
        }

        public bool GuardarIdioma_380_jh(Idioma_380_jh idioma, int? idUsuarioResponsable, string motivo)
        {
            if (idioma == null ||
                string.IsNullOrWhiteSpace(idioma.Codigo_380_jh) ||
                string.IsNullOrWhiteSpace(idioma.Nombre_380_jh))
            {
                return false;
            }

            idioma.Codigo_380_jh = idioma.Codigo_380_jh.Trim();
            idioma.Nombre_380_jh = idioma.Nombre_380_jh.Trim();

            if (idioma.Id_380_jh == 0)
            {
                bool creado = _languageRepository_380_jh.Crear_380_jh(idioma, idUsuarioResponsable) > 0;
                if (creado)
                {
                    _auditoriaService_380_jh.RegistrarAlta_380_jh(idioma.SaveToMemento_380_jh());
                }

                return creado;
            }

            Idioma_380_jh idiomaAnterior = _languageRepository_380_jh.ObtenerPorId_380_jh(idioma.Id_380_jh);
            bool actualizado = _languageRepository_380_jh.Actualizar_380_jh(idioma, idUsuarioResponsable, motivo);

            if (actualizado)
            {
                Idioma_380_jh idiomaNuevo = _languageRepository_380_jh.ObtenerPorId_380_jh(idioma.Id_380_jh);
                if (idiomaAnterior != null && idiomaNuevo != null)
                {
                    _auditoriaService_380_jh.RegistrarModificacion_380_jh(idiomaAnterior.SaveToMemento_380_jh(), idiomaNuevo.SaveToMemento_380_jh());
                }
            }

            return actualizado;
        }

        public List<Etiqueta_380_jh> ListarEtiquetas_380_jh()
        {
            return _translationRepository_380_jh.ListarEtiquetas_380_jh();
        }

        public bool CrearEtiqueta_380_jh(Etiqueta_380_jh etiqueta)
        {
            if (etiqueta == null || string.IsNullOrWhiteSpace(etiqueta.Key_380_jh))
            {
                return false;
            }

            etiqueta.Key_380_jh = etiqueta.Key_380_jh.Trim();
            etiqueta.Descripcion_380_jh = string.IsNullOrWhiteSpace(etiqueta.Descripcion_380_jh)
                ? null
                : etiqueta.Descripcion_380_jh.Trim();

            bool creada = _translationRepository_380_jh.CrearEtiqueta_380_jh(etiqueta) > 0;
            if (creada)
            {
                _auditoriaService_380_jh.RegistrarAlta_380_jh(etiqueta.SaveToMemento_380_jh());
            }

            return creada;
        }

        public bool GuardarTraduccion_380_jh(Traduccion_380_jh traduccion)
        {
            if (traduccion == null ||
                traduccion.EtiquetaId_380_jh == 0 ||
                traduccion.IdiomaId_380_jh == 0 ||
                string.IsNullOrWhiteSpace(traduccion.Texto_380_jh))
            {
                return false;
            }

            traduccion.Texto_380_jh = traduccion.Texto_380_jh.Trim();
            Traduccion_380_jh traduccionAnterior = _translationRepository_380_jh.ObtenerTraduccion_380_jh(traduccion.EtiquetaId_380_jh, traduccion.IdiomaId_380_jh);
            bool guardada = _translationRepository_380_jh.GuardarTraduccion_380_jh(traduccion);

            if (guardada)
            {
                Traduccion_380_jh traduccionNueva = _translationRepository_380_jh.ObtenerTraduccion_380_jh(traduccion.EtiquetaId_380_jh, traduccion.IdiomaId_380_jh);
                if (traduccionNueva != null)
                {
                    if (traduccionAnterior == null)
                    {
                        _auditoriaService_380_jh.RegistrarAlta_380_jh(traduccionNueva.SaveToMemento_380_jh());
                    }
                    else
                    {
                        _auditoriaService_380_jh.RegistrarModificacion_380_jh(traduccionAnterior.SaveToMemento_380_jh(), traduccionNueva.SaveToMemento_380_jh());
                    }
                }
            }

            return guardada;
        }

        public bool GuardarTraduccionDetectada_380_jh(string key, string descripcion, int idiomaId, string texto)
        {
            if (string.IsNullOrWhiteSpace(key) ||
                idiomaId == 0 ||
                string.IsNullOrWhiteSpace(texto))
            {
                return false;
            }

            key = key.Trim();
            texto = texto.Trim();
            descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();

            Etiqueta_380_jh etiqueta = null;
            foreach (Etiqueta_380_jh item in _translationRepository_380_jh.ListarEtiquetas_380_jh())
            {
                if (string.Equals(item.Key_380_jh, key, System.StringComparison.OrdinalIgnoreCase))
                {
                    etiqueta = item;
                    break;
                }
            }

            if (etiqueta == null)
            {
                etiqueta = new Etiqueta_380_jh
                {
                    Key_380_jh = key,
                    Descripcion_380_jh = descripcion
                };

                if (_translationRepository_380_jh.CrearEtiqueta_380_jh(etiqueta) <= 0)
                {
                    return false;
                }

                _auditoriaService_380_jh.RegistrarAlta_380_jh(etiqueta.SaveToMemento_380_jh());
            }

            Traduccion_380_jh traduccion = new Traduccion_380_jh
            {
                EtiquetaId_380_jh = etiqueta.Id_380_jh,
                IdiomaId_380_jh = idiomaId,
                Texto_380_jh = texto
            };

            return GuardarTraduccion_380_jh(traduccion);
        }

        public List<Traduccion_380_jh> ListarTraducciones_380_jh()
        {
            return _translationRepository_380_jh.ListarTraducciones_380_jh();
        }

        public Traduccion_380_jh ObtenerTraduccion_380_jh(int etiquetaId, int idiomaId)
        {
            if (etiquetaId == 0 || idiomaId == 0)
            {
                return null;
            }

            return _translationRepository_380_jh.ObtenerTraduccion_380_jh(etiquetaId, idiomaId);
        }
    }
}
