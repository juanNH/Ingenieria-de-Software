using System;
using System.Windows.Forms;
using System.Linq;
using Application;
using Domain;
using Services;

namespace UI
{
    internal static class ComprobantePdfUI_380_jh
    {
        internal static void GuardarPN2_380_jh(Control propietario, PN2ApplicationService_380_jh servicio, int idSolicitud, int? idEntrega)
        {
            string nombre = idEntrega.HasValue ? "PN2_Entrega_" + idEntrega.Value + ".pdf" : "PN2_Solicitud_" + idSolicitud + ".pdf";
            Guardar_380_jh(propietario, nombre, archivo =>
            {
                AutorizacionApplicationService_380_jh autorizacion = new AutorizacionApplicationService_380_jh();
                if (!autorizacion.TienePermiso_380_jh(PermisosSistema_380_jh.SolicitudVer_380_jh) ||
                    !autorizacion.TienePermiso_380_jh(PermisosSistema_380_jh.ReservaVer_380_jh)) throw new InvalidOperationException();
                InformeIntegridad_380_jh integridad = new IntegridadApplicationService_380_jh().Verificar_380_jh();
                if (!integridad.Disponible_380_jh || !integridad.EsValida_380_jh) throw new InvalidOperationException();
                SolicitudTransfusional_380_jh solicitud = servicio.ListarSolicitudes_380_jh().FirstOrDefault(s => s.Id_380_jh == idSolicitud);
                if (solicitud == null) throw new InvalidOperationException();
                var entregas = servicio.ListarEntregas_380_jh(idSolicitud);
                Usuario_380_jh usuario = Sesion_380_jh.ObtenerInstancia_380_jh().ObtenerUsuario_380_jh();
                new ComprobantePdfService_380_jh().GuardarPN2_380_jh(archivo, solicitud, entregas, usuario == null ? "" : usuario.Username_380_jh, idEntrega);
            });
        }

        internal static void Guardar_380_jh(Control propietario, string nombre, Action<string> generar)
        {
            using (SaveFileDialog dialogo = new SaveFileDialog
            {
                AddExtension = true, DefaultExt = "pdf", Filter = "PDF (*.pdf)|*.pdf",
                FileName = nombre, OverwritePrompt = true,
                Title = LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("RECEIPT_SAVE")
            })
            {
                if (dialogo.ShowDialog(propietario) != DialogResult.OK) return;
                try
                {
                    generar(dialogo.FileName);
                    MessageBox.Show(propietario, LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("RECEIPT_SAVED"));
                }
                catch (Exception)
                {
                    MessageBox.Show(propietario, LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("RECEIPT_ERROR"));
                }
            }
        }
    }
}
