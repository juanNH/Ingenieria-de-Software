using System;
using System.Collections.Generic;
using Domain;
using Repository;

namespace Services
{
    public class LanguageManager_380_jh : IObservableLanguage_380_jh
    {
        private static readonly Lazy<LanguageManager_380_jh> LazyInstance_380_jh =
            new Lazy<LanguageManager_380_jh>(() => new LanguageManager_380_jh());

        private readonly List<IObserverLanguage_380_jh> _observers_380_jh = new List<IObserverLanguage_380_jh>();
        private readonly LanguageRepository_380_jh _languageRepository_380_jh;
        private readonly TranslationService_380_jh _translationService_380_jh;
        private readonly UsuarioRepository_380_jh _usuarioRepository_380_jh;

        public static LanguageManager_380_jh Instance_380_jh
        {
            get { return LazyInstance_380_jh.Value; }
        }

        public Idioma_380_jh CurrentLanguage_380_jh { get; private set; }

        public LanguageManager_380_jh()
            : this(new LanguageRepository_380_jh(), new TranslationService_380_jh(), new UsuarioRepository_380_jh())
        {
        }

        public LanguageManager_380_jh(
            LanguageRepository_380_jh languageRepository,
            TranslationService_380_jh translationService,
            UsuarioRepository_380_jh usuarioRepository)
        {
            _languageRepository_380_jh = languageRepository;
            _translationService_380_jh = translationService;
            _usuarioRepository_380_jh = usuarioRepository;
        }

        public void Initialize_380_jh(Usuario_380_jh usuario)
        {
            Idioma_380_jh idioma = null;

            if (usuario != null && usuario.IdiomaPreferidoId_380_jh.HasValue)
            {
                idioma = _languageRepository_380_jh.ObtenerPorId_380_jh(usuario.IdiomaPreferidoId_380_jh.Value);
            }

            if (idioma == null || !idioma.Activo_380_jh)
            {
                idioma = _languageRepository_380_jh.ObtenerDefault_380_jh();
            }

            CurrentLanguage_380_jh = idioma;
            Notify_380_jh();
        }

        public void ChangeLanguage_380_jh(Idioma_380_jh idioma, Usuario_380_jh usuario)
        {
            if (idioma == null || idioma.Id_380_jh == 0 || !idioma.Activo_380_jh)
            {
                return;
            }

            CurrentLanguage_380_jh = idioma;

            if (usuario != null && usuario.Id_380_jh > 0)
            {
                usuario.IdiomaPreferidoId_380_jh = idioma.Id_380_jh;
                usuario.Idioma_380_jh = idioma.Id_380_jh.ToString();
                _usuarioRepository_380_jh.ActualizarIdiomaPreferido_380_jh(usuario.Id_380_jh, idioma.Id_380_jh);
            }

            Notify_380_jh();
        }

        public string Translate_380_jh(string key)
        {
            return _translationService_380_jh.Translate_380_jh(key, CurrentLanguage_380_jh);
        }

        public List<Idioma_380_jh> ListarIdiomasActivos_380_jh()
        {
            return _languageRepository_380_jh.Listar_380_jh(true);
        }

        public void Attach_380_jh(IObserverLanguage_380_jh observer)
        {
            if (observer == null || _observers_380_jh.Contains(observer))
            {
                return;
            }

            _observers_380_jh.Add(observer);
        }

        public void Detach_380_jh(IObserverLanguage_380_jh observer)
        {
            if (observer == null)
            {
                return;
            }

            _observers_380_jh.Remove(observer);
        }

        public void Notify_380_jh()
        {
            foreach (IObserverLanguage_380_jh observer in _observers_380_jh.ToArray())
            {
                observer.OnLanguageChanged_380_jh(CurrentLanguage_380_jh);
            }
        }
    }
}
