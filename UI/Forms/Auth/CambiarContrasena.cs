using System;
using System.Drawing;
using System.Windows.Forms;
using Application;
using Domain;
using Services;

namespace UI
{
    public class CambiarContrasena_380_jh : Form
    {
        private readonly UsuarioApplicationService_380_jh _usuarioService_380_jh;
        private readonly TextBox _txtContrasenaActual_380_jh;
        private readonly TextBox _txtContrasenaNueva_380_jh;
        private readonly TextBox _txtConfirmarContrasena_380_jh;

        public CambiarContrasena_380_jh(UsuarioApplicationService_380_jh usuarioService)
        {
            _usuarioService_380_jh = usuarioService;
            _txtContrasenaActual_380_jh = CrearCampoContrasena_380_jh();
            _txtContrasenaNueva_380_jh = CrearCampoContrasena_380_jh();
            _txtConfirmarContrasena_380_jh = CrearCampoContrasena_380_jh();
            Inicializar_380_jh();
        }

        private void Inicializar_380_jh()
        {
            Text = Traducir_380_jh("CHANGE_PASSWORD_TITLE");
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = Color.FromArgb(245, 247, 250);
            ClientSize = new Size(440, 330);
            Font = new Font("Segoe UI", 9F);

            Label titulo = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                Location = new Point(28, 20),
                Text = Traducir_380_jh("CHANGE_PASSWORD_TITLE")
            };
            Label ayuda = new Label
            {
                AutoSize = false,
                ForeColor = Color.FromArgb(108, 117, 125),
                Location = new Point(30, 58),
                Size = new Size(380, 38),
                Text = Traducir_380_jh("CHANGE_PASSWORD_HELP")
            };

            Label lblActual = CrearEtiqueta_380_jh("CHANGE_PASSWORD_CURRENT", 30, 106);
            _txtContrasenaActual_380_jh.Location = new Point(30, 128);
            Label lblNueva = CrearEtiqueta_380_jh("CHANGE_PASSWORD_NEW", 30, 165);
            _txtContrasenaNueva_380_jh.Location = new Point(30, 187);
            Label lblConfirmar = CrearEtiqueta_380_jh("CHANGE_PASSWORD_CONFIRM", 30, 224);
            _txtConfirmarContrasena_380_jh.Location = new Point(30, 246);

            Button btnGuardar = new Button
            {
                BackColor = Color.FromArgb(52, 120, 246),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Location = new Point(196, 286),
                Size = new Size(128, 32),
                Text = Traducir_380_jh("CHANGE_PASSWORD_SAVE"),
                UseVisualStyleBackColor = false
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += Guardar_Click_380_jh;

            Button btnCancelar = new Button
            {
                DialogResult = DialogResult.Cancel,
                Location = new Point(332, 286),
                Size = new Size(78, 32),
                Text = Traducir_380_jh("CHANGE_PASSWORD_CANCEL")
            };

            Controls.AddRange(new Control[]
            {
                titulo, ayuda, lblActual, _txtContrasenaActual_380_jh,
                lblNueva, _txtContrasenaNueva_380_jh,
                lblConfirmar, _txtConfirmarContrasena_380_jh,
                btnGuardar, btnCancelar
            });
            AcceptButton = btnGuardar;
            CancelButton = btnCancelar;
        }

        private static TextBox CrearCampoContrasena_380_jh()
        {
            return new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10F),
                Size = new Size(380, 25),
                UseSystemPasswordChar = true
            };
        }

        private static Label CrearEtiqueta_380_jh(string clave, int x, int y)
        {
            return new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(57, 62, 70),
                Location = new Point(x, y),
                Text = Traducir_380_jh(clave)
            };
        }

        private void Guardar_Click_380_jh(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtContrasenaActual_380_jh.Text) ||
                string.IsNullOrWhiteSpace(_txtContrasenaNueva_380_jh.Text) ||
                string.IsNullOrWhiteSpace(_txtConfirmarContrasena_380_jh.Text))
            {
                MostrarMensaje_380_jh("CHANGE_PASSWORD_REQUIRED");
                return;
            }

            if (!string.Equals(_txtContrasenaNueva_380_jh.Text, _txtConfirmarContrasena_380_jh.Text, StringComparison.Ordinal))
            {
                MostrarMensaje_380_jh("CHANGE_PASSWORD_CONFIRM_MISMATCH");
                _txtConfirmarContrasena_380_jh.Focus();
                return;
            }

            ResultadoCambioContrasena_380_jh resultado = _usuarioService_380_jh.CambiarContrasena_380_jh(
                _txtContrasenaActual_380_jh.Text,
                _txtContrasenaNueva_380_jh.Text);

            switch (resultado)
            {
                case ResultadoCambioContrasena_380_jh.Actualizada_380_jh:
                    MostrarMensaje_380_jh("CHANGE_PASSWORD_SUCCESS");
                    DialogResult = DialogResult.OK;
                    Close();
                    break;

                case ResultadoCambioContrasena_380_jh.ErrorIntegridad_380_jh:
                    MostrarMensaje_380_jh("CHANGE_PASSWORD_INTEGRITY_WARNING");
                    DialogResult = DialogResult.OK;
                    Close();
                    break;

                case ResultadoCambioContrasena_380_jh.ContrasenaActualIncorrecta_380_jh:
                    MostrarMensaje_380_jh("CHANGE_PASSWORD_CURRENT_INVALID");
                    _txtContrasenaActual_380_jh.Clear();
                    _txtContrasenaActual_380_jh.Focus();
                    break;

                case ResultadoCambioContrasena_380_jh.NuevaIgualActual_380_jh:
                    MostrarMensaje_380_jh("CHANGE_PASSWORD_SAME");
                    _txtContrasenaNueva_380_jh.Clear();
                    _txtConfirmarContrasena_380_jh.Clear();
                    _txtContrasenaNueva_380_jh.Focus();
                    break;

                case ResultadoCambioContrasena_380_jh.DatosInvalidos_380_jh:
                    MostrarMensaje_380_jh("CHANGE_PASSWORD_REQUIRED");
                    break;

                default:
                    MostrarMensaje_380_jh("CHANGE_PASSWORD_SAVE_ERROR");
                    break;
            }
        }

        private void MostrarMensaje_380_jh(string clave)
        {
            MessageBox.Show(this, Traducir_380_jh(clave), Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static string Traducir_380_jh(string clave)
        {
            return LanguageManager_380_jh.Instance_380_jh.Translate_380_jh(clave);
        }
    }
}
