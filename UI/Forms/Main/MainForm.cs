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
        private ToolStripMenuItem _bancoSangreToolStripMenuItem_380_jh;
        private ToolStripMenuItem _donantesToolStripMenuItem_380_jh;
        private ToolStripMenuItem _donacionesToolStripMenuItem_380_jh;
        private ToolStripMenuItem _unidadesToolStripMenuItem_380_jh;
        private bool _cargandoIdiomas_380_jh;
        private ToolStripMenuItem _integridadMenu_380_jh;
        private ToolStripStatusLabel _integridadEstado_380_jh;
        private ToolStripButton _integridadVerificar_380_jh;

        public MainForm_380_jh()
            : this(new AutorizacionApplicationService_380_jh(), new UsuarioApplicationService_380_jh())
        {
        }

        public MainForm_380_jh(AutorizacionApplicationService_380_jh autorizacionService, UsuarioApplicationService_380_jh usuarioService)
        {
            _autorizacionService_380_jh = autorizacionService;
            _usuarioService_380_jh = usuarioService;
            InitializeComponent_380_jh();
            ConfigurarMenuNegocio_380_jh();
            ConfigurarIntegridad_380_jh();
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
            new IntegridadApplicationService_380_jh().Verificar_380_jh();
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
            ActualizarIntegridad_380_jh(this, EventArgs.Empty);
        }

        private void ConfigurarTraducciones_380_jh()
        {
            bitacoraToolStripMenuItem_380_jh.Tag = "MENU_AUDIT";
            auditoriaCambiosToolStripMenuItem_380_jh.Tag = "MENU_CHANGE_AUDIT";
            usuariosToolStripMenuItem_380_jh.Tag = "MENU_USERS";
            rolesToolStripMenuItem_380_jh.Tag = "MENU_ROLES";
            idiomasToolStripMenuItem_380_jh.Tag = "MENU_LANGUAGES";
            _bancoSangreToolStripMenuItem_380_jh.Tag = "MENU_BLOOD_BANK";
            _donantesToolStripMenuItem_380_jh.Tag = "MENU_DONORS";
            _donacionesToolStripMenuItem_380_jh.Tag = "MENU_DONATIONS";
            _unidadesToolStripMenuItem_380_jh.Tag = "MENU_UNITS";
            salirToolStripMenuItem_380_jh.Tag = "MENU_LOGOUT";
            lblIdioma_380_jh.Tag = "LANGUAGE_SELECTOR";
        }

        private void ConfigurarMenuNegocio_380_jh()
        {
            _bancoSangreToolStripMenuItem_380_jh = new ToolStripMenuItem
            {
                Name = "bancoSangreToolStripMenuItem",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.White
            };
            _donantesToolStripMenuItem_380_jh = new ToolStripMenuItem { Name = "donantesToolStripMenuItem" };
            _donantesToolStripMenuItem_380_jh.Click += donantesToolStripMenuItem_Click_380_jh;
            _donacionesToolStripMenuItem_380_jh = new ToolStripMenuItem { Name = "donacionesToolStripMenuItem" };
            _donacionesToolStripMenuItem_380_jh.Click += donacionesToolStripMenuItem_Click_380_jh;
            _unidadesToolStripMenuItem_380_jh = new ToolStripMenuItem { Name = "unidadesToolStripMenuItem" };
            _unidadesToolStripMenuItem_380_jh.Click += unidadesToolStripMenuItem_Click_380_jh;
            _bancoSangreToolStripMenuItem_380_jh.DropDownItems.AddRange(new ToolStripItem[]
            {
                _donantesToolStripMenuItem_380_jh,
                _donacionesToolStripMenuItem_380_jh,
                _unidadesToolStripMenuItem_380_jh
            });

            int posicionSalida = menuStrip1_380_jh.Items.IndexOf(salirToolStripMenuItem_380_jh);
            menuStrip1_380_jh.Items.Insert(posicionSalida < 0 ? menuStrip1_380_jh.Items.Count : posicionSalida, _bancoSangreToolStripMenuItem_380_jh);
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

        private void donantesToolStripMenuItem_Click_380_jh(object sender, EventArgs e)
        {
            if (!_autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.DonanteVer_380_jh) &&
                !ValidarPermiso_380_jh(PermisosSistema_380_jh.DonanteCrear_380_jh))
            {
                return;
            }

            ShowScreen_380_jh(new DonantesView_380_jh());
        }

        private void donacionesToolStripMenuItem_Click_380_jh(object sender, EventArgs e)
        {
            if (!ValidarPermiso_380_jh(PermisosSistema_380_jh.DonacionCrear_380_jh))
            {
                return;
            }

            ShowScreen_380_jh(new DonacionesView_380_jh());
        }

        private void unidadesToolStripMenuItem_Click_380_jh(object sender, EventArgs e)
        {
            if (!TienePermisoUnidades_380_jh())
            {
                MessageBox.Show(LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("SECURITY_ACCESS_DENIED"));
                return;
            }

            ShowScreen_380_jh(new UnidadesView_380_jh());
        }

        private void ShowScreen_380_jh(UserControl screen)
        {
            while (contentPanel_380_jh.Controls.Count > 0) contentPanel_380_jh.Controls[0].Dispose();
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
            cmbIdiomas_380_jh.DisplayMember = "Nombre_380_jh";
            cmbIdiomas_380_jh.ValueMember = "Id_380_jh";
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
            _integridadMenu_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.IntegridadVer_380_jh);
            bitacoraToolStripMenuItem_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.BitacoraVer_380_jh);
            auditoriaCambiosToolStripMenuItem_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.AuditoriaCambiosVer_380_jh);
            usuariosToolStripMenuItem_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UsuarioVer_380_jh);
            rolesToolStripMenuItem_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.RolVer_380_jh);
            idiomasToolStripMenuItem_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.IdiomaVer_380_jh) ||
                                               _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.TraduccionVer_380_jh);

            bool mostrarDonantes = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.DonanteVer_380_jh) ||
                                    _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.DonanteCrear_380_jh);
            bool mostrarDonaciones = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.DonacionCrear_380_jh);
            bool mostrarUnidades = TienePermisoUnidades_380_jh();

            _donantesToolStripMenuItem_380_jh.Visible = mostrarDonantes;
            _donacionesToolStripMenuItem_380_jh.Visible = mostrarDonaciones;
            _unidadesToolStripMenuItem_380_jh.Visible = mostrarUnidades;
            _bancoSangreToolStripMenuItem_380_jh.Visible = mostrarDonantes || mostrarDonaciones || mostrarUnidades;
        }

        private bool TienePermisoUnidades_380_jh()
        {
            return _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UnidadVer_380_jh) ||
                   _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UnidadClasificar_380_jh) ||
                   _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UnidadLiberar_380_jh) ||
                   _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UnidadBloquear_380_jh) ||
                   _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UnidadDescartar_380_jh);
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

            if (_donantesToolStripMenuItem_380_jh.Available)
            {
                ShowScreen_380_jh(new DonantesView_380_jh());
                return;
            }

            if (_donacionesToolStripMenuItem_380_jh.Available)
            {
                ShowScreen_380_jh(new DonacionesView_380_jh());
                return;
            }

            if (_unidadesToolStripMenuItem_380_jh.Available)
            {
                ShowScreen_380_jh(new UnidadesView_380_jh());
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
            IntegridadApplicationService_380_jh.EstadoCambiado_380_jh -= ActualizarIntegridad_380_jh;
            LanguageManager_380_jh.Instance_380_jh.Detach_380_jh(this);
        }

        private void ConfigurarIntegridad_380_jh()
        {
            _integridadMenu_380_jh = new ToolStripMenuItem { Tag = "MENU_INTEGRITY", ForeColor = Color.White, Font = new Font("Segoe UI", 10F) };
            _integridadMenu_380_jh.Click += delegate { AbrirIntegridad_380_jh(); };
            menuStrip1_380_jh.Items.Insert(menuStrip1_380_jh.Items.IndexOf(salirToolStripMenuItem_380_jh), _integridadMenu_380_jh);
            var barra = new StatusStrip { Dock = DockStyle.Bottom };
            _integridadEstado_380_jh = new ToolStripStatusLabel { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            _integridadEstado_380_jh.Click += delegate { AbrirIntegridad_380_jh(); };
            barra.Items.Add(_integridadEstado_380_jh);
            _integridadVerificar_380_jh = new ToolStripButton();
            _integridadVerificar_380_jh.Click += delegate { new IntegridadApplicationService_380_jh().Verificar_380_jh(); };
            barra.Items.Add(_integridadVerificar_380_jh);
            Controls.Add(barra);
            IntegridadApplicationService_380_jh.EstadoCambiado_380_jh += ActualizarIntegridad_380_jh;
            Disposed += delegate { IntegridadApplicationService_380_jh.EstadoCambiado_380_jh -= ActualizarIntegridad_380_jh; };
        }

        private void AbrirIntegridad_380_jh()
        {
            if (ValidarPermiso_380_jh(PermisosSistema_380_jh.IntegridadVer_380_jh))
                ShowScreen_380_jh(new IntegridadView_380_jh());
        }

        private void ActualizarIntegridad_380_jh(object sender, EventArgs e)
        {
            if (_integridadEstado_380_jh == null || IsDisposed) return;
            var estado = IntegridadApplicationService_380_jh.Estado_380_jh;
            _integridadVerificar_380_jh.Text = LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("INTEGRITY_VERIFY");
            _integridadEstado_380_jh.Text = LanguageManager_380_jh.Instance_380_jh.Translate_380_jh(
                !estado.Disponible_380_jh ? "INTEGRITY_UNAVAILABLE" : estado.EsValida_380_jh ? "INTEGRITY_OK" : "INTEGRITY_BLOCKED");
            _integridadEstado_380_jh.BackColor = estado.EsValida_380_jh ? Color.Honeydew : Color.MistyRose;
            _integridadEstado_380_jh.ForeColor = estado.EsValida_380_jh ? Color.DarkGreen : Color.DarkRed;
        }
    }
}
