using System;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using System.Diagnostics;
using Application;
using Domain;
using Services;

namespace UI
{
    internal static class ComprobantePdfUI_380_jh
    {
        internal static bool VerificarInstalacion_380_jh(Control propietario)
        {
            if (new ComprobanteAutomaticoApplicationService_380_jh().Disponible_380_jh()) return true;
            MessageBox.Show(propietario, LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("RECEIPT_MIGRATION_REQUIRED"));
            return false;
        }

        internal static void CopiarOriginal_380_jh(Control propietario, string proceso, int referencia)
        {
            string original;
            try
            {
                InformeIntegridad_380_jh integridad = new IntegridadApplicationService_380_jh().Verificar_380_jh();
                if (!integridad.Disponible_380_jh || !integridad.EsValida_380_jh) throw new InvalidOperationException();
                original = new ComprobanteAutomaticoApplicationService_380_jh().ObtenerOriginal_380_jh(proceso, referencia);
                if (string.IsNullOrWhiteSpace(original) || !File.Exists(original)) throw new FileNotFoundException();
            }
            catch
            {
                MessageBox.Show(propietario, LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("RECEIPT_ORIGINAL_UNAVAILABLE"));
                return;
            }
            Guardar_380_jh(propietario, "Copia_" + proceso + "_" + referencia + ".pdf", destino => File.Copy(original, destino, false));
        }

        internal static void ProcesarPendientes_380_jh(Control propietario, bool mostrarExito, bool mostrarSinPendientes = true)
        {
            ResultadoComprobantes_380_jh resultado = new ComprobanteAutomaticoApplicationService_380_jh().ProcesarPendientes_380_jh();
            if (resultado.ErrorConsulta_380_jh || resultado.Pendientes_380_jh > 0)
            {
                MessageBox.Show(propietario, LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("RECEIPT_PENDING"));
                return;
            }
            if (mostrarExito && resultado.Archivos_380_jh.Count > 0)
            {
                string mensaje;
                try
                {
                    mensaje = string.Format(LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("RECEIPT_AUTO_SAVED"),
                        resultado.Archivos_380_jh.Count, string.Join(Environment.NewLine, resultado.Archivos_380_jh));
                }
                catch (FormatException) { mensaje = string.Join(Environment.NewLine, resultado.Archivos_380_jh); }
                if (MessageBox.Show(propietario, mensaje, "PDF", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    try { Process.Start(new ProcessStartInfo(resultado.Archivos_380_jh[resultado.Archivos_380_jh.Count - 1]) { UseShellExecute = true }); }
                    catch { MessageBox.Show(propietario, LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("RECEIPT_OPEN_ERROR")); }
                }
            }
            else if (mostrarExito && mostrarSinPendientes)
                MessageBox.Show(propietario, LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("RECEIPT_NONE_PENDING"));
        }

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
                FileName = nombre, OverwritePrompt = false,
                Title = LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("RECEIPT_SAVE")
            })
            {
                if (dialogo.ShowDialog(propietario) != DialogResult.OK) return;
                if (File.Exists(dialogo.FileName))
                {
                    MessageBox.Show(propietario, LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("RECEIPT_COPY_EXISTS"));
                    return;
                }
                string temporal = Path.Combine(Path.GetDirectoryName(dialogo.FileName), "." + Guid.NewGuid().ToString("N") + ".tmp");
                try
                {
                    generar(temporal);
                    File.Move(temporal, dialogo.FileName);
                    MessageBox.Show(propietario, LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("RECEIPT_SAVED"));
                }
                catch (Exception)
                {
                    MessageBox.Show(propietario, LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("RECEIPT_ERROR"));
                }
                finally { try { if (File.Exists(temporal)) File.Delete(temporal); } catch { } }
            }
        }
    }
}
