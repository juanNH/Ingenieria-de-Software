using System;
using System.Windows.Forms;
using Application;
using Domain;
using Services;

namespace UI
{
    public partial class Login_380_jh : Form
    {
        private readonly UsuarioApplicationService_380_jh _usuarioService_380_jh;
        private readonly UiTextService_380_jh _uiTextService_380_jh;

        public Login_380_jh()
            : this(new UsuarioApplicationService_380_jh(), new UiTextService_380_jh())
        {
        }

        public Login_380_jh(UsuarioApplicationService_380_jh usuarioService, UiTextService_380_jh uiTextService)
        {
            _usuarioService_380_jh = usuarioService;
            _uiTextService_380_jh = uiTextService;
            InitializeComponent_380_jh();
        }

        private void btnLogin_Click_380_jh(object sender, EventArgs e)
        {
            Usuario_380_jh usuarioValidado = _usuarioService_380_jh.Login_380_jh(txtUser_380_jh.Text, txtPass_380_jh.Text);

            if (usuarioValidado != null)
            {
                Sesion_380_jh sesion = Sesion_380_jh.ObtenerInstancia_380_jh();

                sesion.IniciarSesion_380_jh(usuarioValidado);
                LanguageManager_380_jh.Instance_380_jh.Initialize_380_jh(usuarioValidado);
                MessageBox.Show(_uiTextService_380_jh.BuildWelcomeMessage_380_jh(sesion.ObtenerUsuario_380_jh().ToString()));
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                if (_usuarioService_380_jh.EstaBloqueado_380_jh(txtUser_380_jh.Text))
                {
                    MessageBox.Show("El usuario esta bloqueado. Contacte a un administrador.");
                    return;
                }

                if (_usuarioService_380_jh.HayBloqueoDigitoVerificador_380_jh())
                {
                    MessageBox.Show("Se detecto una inconsistencia de integridad. Solo un administrador puede iniciar sesion.");
                    return;
                }

                MessageBox.Show("Usuario, email o contrasena incorrectos.");
            }
        }

        private void btnRegistrar_Click_380_jh(object sender, EventArgs e)
        {
            using (Registro_380_jh registro = new Registro_380_jh(_usuarioService_380_jh))
            {
                if (registro.ShowDialog(this) == DialogResult.OK)
                {
                    txtUser_380_jh.Text = registro.UsuarioRegistrado_380_jh;
                    txtPass_380_jh.Clear();
                    txtPass_380_jh.Focus();
                }
            }
        }
    }
}
