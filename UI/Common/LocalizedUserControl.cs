using System;
using System.Windows.Forms;
using Domain;
using Services;

namespace UI
{
    public class LocalizedUserControl_380_jh : UserControl, IObserverLanguage_380_jh
    {
        protected LocalizedUserControl_380_jh()
        {
            Load += LocalizedUserControl_Load_380_jh;
            Disposed += LocalizedUserControl_Disposed_380_jh;
        }

        public virtual void OnLanguageChanged_380_jh(Idioma_380_jh idioma)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => OnLanguageChanged_380_jh(idioma)));
                return;
            }

            ApplyTranslations_380_jh();
        }

        protected virtual void ApplyTranslations_380_jh()
        {
            TranslationApplier_380_jh.Apply_380_jh(this);
        }

        private void LocalizedUserControl_Load_380_jh(object sender, EventArgs e)
        {
            LanguageManager_380_jh.Instance_380_jh.Attach_380_jh(this);
            OnLanguageChanged_380_jh(LanguageManager_380_jh.Instance_380_jh.CurrentLanguage_380_jh);
        }

        private void LocalizedUserControl_Disposed_380_jh(object sender, EventArgs e)
        {
            LanguageManager_380_jh.Instance_380_jh.Detach_380_jh(this);
        }
    }
}
