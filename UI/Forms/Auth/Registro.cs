using System;
using System.Windows.Forms;
using Application;
using Domain;

namespace UI
{
    public partial class Registro_380_jh : Form
    {
        private readonly UsuarioApplicationService_380_jh _usuarioService_380_jh;

        public string UsuarioRegistrado_380_jh { get; private set; }

        public Registro_380_jh(UsuarioApplicationService_380_jh usuarioService)
        {
            _usuarioService_380_jh = usuarioService;
            InitializeComponent_380_jh();
        }

        private void btnCrearCuenta_Click_380_jh(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUser_380_jh.Text) ||
                string.IsNullOrWhiteSpace(txtEmail_380_jh.Text) ||
                string.IsNullOrWhiteSpace(txtPass_380_jh.Text) ||
                string.IsNullOrWhiteSpace(txtNombre_380_jh.Text) ||
                string.IsNullOrWhiteSpace(txtApellido_380_jh.Text))
            {
                MessageBox.Show("Completa todos los campos para registrarte.");
                return;
            }

            Usuario_380_jh nuevo = new Usuario_380_jh
            {
                Username_380_jh = txtUser_380_jh.Text.Trim(),
                Email_380_jh = txtEmail_380_jh.Text.Trim(),
                Password_380_jh = txtPass_380_jh.Text,
                Nombre_380_jh = txtNombre_380_jh.Text.Trim(),
                Apellido_380_jh = txtApellido_380_jh.Text.Trim()
            };

            CodigoRegistroUsuario_380_jh resultado = _usuarioService_380_jh.CrearUsuario_380_jh(nuevo);

            if (resultado == CodigoRegistroUsuario_380_jh.Creado_380_jh)
            {
                UsuarioRegistrado_380_jh = nuevo.Username_380_jh;
                MessageBox.Show("Usuario registrado con exito.");
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show(ObtenerMensajeRegistro_380_jh(resultado));
            }
        }

        private static string ObtenerMensajeRegistro_380_jh(CodigoRegistroUsuario_380_jh resultado)
        {
            switch (resultado)
            {
                case CodigoRegistroUsuario_380_jh.DatosInvalidos_380_jh:
                    return "Completa todos los campos para registrarte.";

                case CodigoRegistroUsuario_380_jh.EmailInvalido_380_jh:
                    return "Ingresa un email valido.";

                case CodigoRegistroUsuario_380_jh.UsuarioExistente_380_jh:
                    return "Ya existe un usuario con ese nombre.";

                case CodigoRegistroUsuario_380_jh.EmailExistente_380_jh:
                    return "Ya existe un usuario con ese email.";

                case CodigoRegistroUsuario_380_jh.IdiomaDefaultInexistente_380_jh:
                    return "No se pudo registrar el usuario porque falta el idioma default.";

                default:
                    return "Ocurrio un error tecnico al registrar el usuario.";
            }
        }

        private void btnCancelar_Click_380_jh(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
