using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Application;
using Domain;
using Services;

namespace UI
{
    public partial class MainForm_380_jh : Form, IObserverLanguage_380_jh
    {
        private readonly AutorizacionApplicationService_380_jh _autorizacionService_380_jh;
        private readonly UsuarioApplicationService_380_jh _usuarioService_380_jh;
        private bool _cargandoIdiomas_380_jh;

        public MainForm_380_jh()
            : this(new AutorizacionApplicationService_380_jh(), new UsuarioApplicationService_380_jh())
        {
        }

        public MainForm_380_jh(AutorizacionApplicationService_380_jh autorizacionService, UsuarioApplicationService_380_jh usuarioService)
        {
            _autorizacionService_380_jh = autorizacionService;
            _usuarioService_380_jh = usuarioService;
            InitializeComponent_380_jh();
            ConfigurarTraducciones_380_jh();
            Load += MainForm_Load_380_jh;
            FormClosed += MainForm_FormClosed_380_jh;
        }

        private void MainForm_Load_380_jh(object sender, EventArgs e)
        {
            Usuario_380_jh usuario = Sesion_380_jh.ObtenerInstancia_380_jh().ObtenerUsuario_380_jh();
            LanguageManager_380_jh.Instance_380_jh.Attach_380_jh(this);
            LanguageManager_380_jh.Instance_380_jh.Initialize_380_jh(usuario);
            CargarSelectorIdiomas_380_jh();
            ActualizarUsuario_380_jh();

            ConfigurarMenuPorPermisos_380_jh();
            MostrarPantallaInicial_380_jh();
        }

        public void OnLanguageChanged_380_jh(Idioma_380_jh idioma)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => OnLanguageChanged_380_jh(idioma)));
                return;
            }

            TranslationApplier_380_jh.ApplyMenu_380_jh(menuStrip1_380_jh);
            TranslationApplier_380_jh.Apply_380_jh(topPanel_380_jh);
            Text = LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("MAIN_TITLE");
            ActualizarUsuario_380_jh();
            SeleccionarIdiomaActual_380_jh();
        }

        private void ConfigurarTraducciones_380_jh()
        {
            bitacoraToolStripMenuItem_380_jh.Tag = "MENU_AUDIT";
            auditoriaCambiosToolStripMenuItem_380_jh.Tag = "MENU_CHANGE_AUDIT";
            usuariosToolStripMenuItem_380_jh.Tag = "MENU_USERS";
            rolesToolStripMenuItem_380_jh.Tag = "MENU_ROLES";
            idiomasToolStripMenuItem_380_jh.Tag = "MENU_LANGUAGES";
            salirToolStripMenuItem_380_jh.Tag = "MENU_LOGOUT";
            lblIdioma_380_jh.Tag = "LANGUAGE_SELECTOR";
        }

        private void bitacoraToolStripMenuItem_Click_380_jh(object sender, EventArgs e)
        {
            if (!ValidarPermiso_380_jh(PermisosSistema_380_jh.BitacoraVer_380_jh))
            {
                return;
            }

            ShowScreen_380_jh(new BitacoraView_380_jh());
        }

        private void auditoriaCambiosToolStripMenuItem_Click_380_jh(object sender, EventArgs e)
        {
            if (!ValidarPermiso_380_jh(PermisosSistema_380_jh.AuditoriaCambiosVer_380_jh))
            {
                return;
            }

            ShowScreen_380_jh(new AuditoriaCambiosView_380_jh());
        }

        private void rolesToolStripMenuItem_Click_380_jh(object sender, EventArgs e)
        {
            if (!ValidarPermiso_380_jh(PermisosSistema_380_jh.RolVer_380_jh))
            {
                return;
            }

            ShowScreen_380_jh(new RolesView_380_jh());
        }

        private void idiomasToolStripMenuItem_Click_380_jh(object sender, EventArgs e)
        {
            if (!_autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.IdiomaVer_380_jh) &&
                !_autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.TraduccionVer_380_jh))
            {
                MessageBox.Show(LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("SECURITY_ACCESS_DENIED"));
                return;
            }

            ShowScreen_380_jh(new IdiomasView_380_jh());
        }

        private void usuariosToolStripMenuItem_Click_380_jh(object sender, EventArgs e)
        {
            if (!ValidarPermiso_380_jh(PermisosSistema_380_jh.UsuarioVer_380_jh))
            {
                return;
            }

            ShowScreen_380_jh(new UsuariosView_380_jh());
        }

        private void salirToolStripMenuItem_Click_380_jh(object sender, EventArgs e)
        {
            _usuarioService_380_jh.RecalcularDigitosVerificadoresUsuarios_380_jh();
            Sesion_380_jh.ObtenerInstancia_380_jh().Logout_380_jh();
            DialogResult = DialogResult.Retry;
            Close();
        }

        private void ShowScreen_380_jh(UserControl screen)
        {
            contentPanel_380_jh.Controls.Clear();
            screen.Dock = DockStyle.Fill;
            contentPanel_380_jh.Controls.Add(screen);
        }

        public void RefrescarIdiomasDisponibles_380_jh()
        {
            CargarSelectorIdiomas_380_jh();
        }

        private void CargarSelectorIdiomas_380_jh()
        {
            _cargandoIdiomas_380_jh = true;
            List<Idioma_380_jh> idiomas = LanguageManager_380_jh.Instance_380_jh.ListarIdiomasActivos_380_jh();
            cmbIdiomas_380_jh.DataSource = null;
            cmbIdiomas_380_jh.DisplayMember = "Nombre";
            cmbIdiomas_380_jh.ValueMember = "Id";
            cmbIdiomas_380_jh.DataSource = idiomas;
            _cargandoIdiomas_380_jh = false;
            SeleccionarIdiomaActual_380_jh();
        }

        private void SeleccionarIdiomaActual_380_jh()
        {
            if (_cargandoIdiomas_380_jh || LanguageManager_380_jh.Instance_380_jh.CurrentLanguage_380_jh == null)
            {
                return;
            }

            cmbIdiomas_380_jh.SelectedValue = LanguageManager_380_jh.Instance_380_jh.CurrentLanguage_380_jh.Id_380_jh;
        }

        private void cmbIdiomas_SelectedIndexChanged_380_jh(object sender, EventArgs e)
        {
            if (_cargandoIdiomas_380_jh)
            {
                return;
            }

            Idioma_380_jh idioma = cmbIdiomas_380_jh.SelectedItem as Idioma_380_jh;
            Usuario_380_jh usuario = Sesion_380_jh.ObtenerInstancia_380_jh().ObtenerUsuario_380_jh();
            LanguageManager_380_jh.Instance_380_jh.ChangeLanguage_380_jh(idioma, usuario);
        }

        private void ConfigurarMenuPorPermisos_380_jh()
        {
            bitacoraToolStripMenuItem_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.BitacoraVer_380_jh);
            auditoriaCambiosToolStripMenuItem_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.AuditoriaCambiosVer_380_jh);
            usuariosToolStripMenuItem_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UsuarioVer_380_jh);
            rolesToolStripMenuItem_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.RolVer_380_jh);
            idiomasToolStripMenuItem_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.IdiomaVer_380_jh) ||
                                               _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.TraduccionVer_380_jh);
        }

        private void MostrarPantallaInicial_380_jh()
        {
            if (bitacoraToolStripMenuItem_380_jh.Visible)
            {
                ShowScreen_380_jh(new BitacoraView_380_jh());
                return;
            }

            if (auditoriaCambiosToolStripMenuItem_380_jh.Visible)
            {
                ShowScreen_380_jh(new AuditoriaCambiosView_380_jh());
                return;
            }

            if (usuariosToolStripMenuItem_380_jh.Visible)
            {
                ShowScreen_380_jh(new UsuariosView_380_jh());
                return;
            }

            if (rolesToolStripMenuItem_380_jh.Visible)
            {
                ShowScreen_380_jh(new RolesView_380_jh());
                return;
            }

            if (idiomasToolStripMenuItem_380_jh.Visible)
            {
                ShowScreen_380_jh(new IdiomasView_380_jh());
                return;
            }

            Label mensaje = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12F),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("NO_PERMISSIONS_ASSIGNED")
            };

            contentPanel_380_jh.Controls.Clear();
            contentPanel_380_jh.Controls.Add(mensaje);
        }

        private bool ValidarPermiso_380_jh(string codigoPermiso)
        {
            if (_autorizacionService_380_jh.TienePermiso_380_jh(codigoPermiso))
            {
                return true;
            }

            MessageBox.Show(LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("SECURITY_ACCESS_DENIED"));
            return false;
        }

        private void ActualizarUsuario_380_jh()
        {
            Usuario_380_jh usuario = Sesion_380_jh.ObtenerInstancia_380_jh().ObtenerUsuario_380_jh();
            lblUsuario_380_jh.Text = usuario != null
                ? string.Format(LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("MAIN_USER"), usuario)
                : LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("MAIN_NO_SESSION");
        }

        private void MainForm_FormClosed_380_jh(object sender, FormClosedEventArgs e)
        {
            LanguageManager_380_jh.Instance_380_jh.Detach_380_jh(this);
        }
    }
}
