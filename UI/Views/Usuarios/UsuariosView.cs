using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Application;
using Domain;
using Services;

namespace UI
{
    public partial class UsuariosView_380_jh : LocalizedUserControl_380_jh
    {
        private readonly UsuarioApplicationService_380_jh _usuarioService_380_jh;
        private readonly PermisoApplicationService_380_jh _permisoService_380_jh;
        private readonly AutorizacionApplicationService_380_jh _autorizacionService_380_jh;
        private List<Usuario_380_jh> _usuarios_380_jh;
        private List<ComponentePermiso_380_jh> _familiasDisponibles_380_jh;
        private Usuario_380_jh _usuarioSeleccionado_380_jh;
        private bool _preparandoFormulario_380_jh;
        private GroupBox groupBoxRolesUsuario_380_jh;
        private Label lblRolesInfo_380_jh;
        private ComboBox cmbRolUsuario_380_jh;
        private Button btnGuardarRoles_380_jh;
        private Button btnRecalcularDigitos_380_jh;

        public UsuariosView_380_jh()
            : this(new UsuarioApplicationService_380_jh())
        {
        }

        public UsuariosView_380_jh(UsuarioApplicationService_380_jh usuarioService)
        {
            _usuarioService_380_jh = usuarioService;
            _permisoService_380_jh = new PermisoApplicationService_380_jh();
            _autorizacionService_380_jh = new AutorizacionApplicationService_380_jh();
            InitializeComponent_380_jh();
            ConfigurarTraducciones_380_jh();
            ConfigurarGrilla_380_jh();
            ConfigurarAsignacionRoles_380_jh();
            ConfigurarPermisos_380_jh();
            CargarUsuarios_380_jh();
            CargarRolesDisponibles_380_jh();
            PrepararNuevoUsuario_380_jh();
        }

        private void ConfigurarTraducciones_380_jh()
        {
            lblTitulo_380_jh.Tag = "USERS_TITLE";
            lblDescripcion_380_jh.Tag = "USERS_DESCRIPTION";
            groupBoxDetalle_380_jh.Tag = "USERS_DETAIL";
            lblUsuario_380_jh.Tag = "FIELD_USER";
            lblEmail_380_jh.Tag = "FIELD_EMAIL";
            lblNombre_380_jh.Tag = "FIELD_NAME";
            lblApellido_380_jh.Tag = "FIELD_LASTNAME";
            lblPassword_380_jh.Tag = "FIELD_NEW_PASSWORD";
            lblEstado_380_jh.Tag = "FIELD_STATUS";
            btnNuevo_380_jh.Tag = "BTN_NEW";
            btnInhabilitar_380_jh.Tag = "BTN_DISABLE";
            columnId_380_jh.Tag = "GRID_ID";
            columnUsuario_380_jh.Tag = "GRID_USER";
            columnEmail_380_jh.Tag = "GRID_EMAIL";
            columnEstado_380_jh.Tag = "GRID_STATUS";
            columnBloqueoDigitoVerificador_380_jh.Tag = "GRID_DV_BLOCK";
        }

        private void ConfigurarGrilla_380_jh()
        {
            dgvUsuarios_380_jh.AutoGenerateColumns = false;
            columnId_380_jh.Width = 45;
            columnUsuario_380_jh.Width = 90;
            columnEmail_380_jh.MinimumWidth = 110;
            columnEstado_380_jh.Width = 70;
            columnBloqueoDigitoVerificador_380_jh.Width = 75;
            cmbEstado_380_jh.Items.AddRange(new object[] { "ACTIVO", "INACTIVO" });
        }

        protected override void ApplyTranslations_380_jh()
        {
            base.ApplyTranslations_380_jh();
            if (btnRecalcularDigitos_380_jh != null)
            {
                btnRecalcularDigitos_380_jh.Text = LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("BTN_RECALCULATE_DV");
            }

            ActualizarEstadoRolesUsuario_380_jh();
        }

        private void CargarUsuarios_380_jh()
        {
            _usuarios_380_jh = _usuarioService_380_jh.Listar_380_jh();
            dgvUsuarios_380_jh.DataSource = null;
            dgvUsuarios_380_jh.DataSource = _usuarios_380_jh;
        }

        private void dgvUsuarios_SelectionChanged_380_jh(object sender, EventArgs e)
        {
            if (_preparandoFormulario_380_jh)
            {
                return;
            }

            if (dgvUsuarios_380_jh.CurrentRow == null)
            {
                return;
            }

            Usuario_380_jh usuario = dgvUsuarios_380_jh.CurrentRow.DataBoundItem as Usuario_380_jh;
            if (usuario == null)
            {
                return;
            }

            _usuarioSeleccionado_380_jh = usuario;
            txtUsuario_380_jh.Text = usuario.Username_380_jh;
            txtEmail_380_jh.Text = usuario.Email_380_jh;
            txtNombre_380_jh.Text = usuario.Nombre_380_jh;
            txtApellido_380_jh.Text = usuario.Apellido_380_jh;
            txtPassword_380_jh.Text = string.Empty;
            cmbEstado_380_jh.SelectedItem = string.IsNullOrWhiteSpace(usuario.Estado_380_jh) ? "ACTIVO" : usuario.Estado_380_jh;
            lblModo_380_jh.Tag = "USERS_EDIT_MODE";
            lblModo_380_jh.Text = LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("USERS_EDIT_MODE");
            btnGuardar_380_jh.Tag = "BTN_SAVE";
            btnGuardar_380_jh.Text = LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("BTN_SAVE");
            btnGuardar_380_jh.Enabled = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UsuarioEditar_380_jh);
            btnInhabilitar_380_jh.Enabled = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UsuarioInhabilitar_380_jh) &&
                                     usuario.Id_380_jh > 0 &&
                                     usuario.Estado_380_jh != "INACTIVO";
            CargarRolesUsuario_380_jh(usuario.Id_380_jh);
            ActualizarEstadoRolesUsuario_380_jh();
        }

        private void btnNuevo_Click_380_jh(object sender, EventArgs e)
        {
            PrepararNuevoUsuario_380_jh();
        }

        private void btnGuardar_Click_380_jh(object sender, EventArgs e)
        {
            if (_usuarioSeleccionado_380_jh == null)
            {
                CrearUsuario_380_jh();
                return;
            }

            ModificarUsuario_380_jh();
        }

        private void btnInhabilitar_Click_380_jh(object sender, EventArgs e)
        {
            if (_usuarioSeleccionado_380_jh == null)
            {
                MessageBox.Show("Selecciona un usuario para inhabilitar.");
                return;
            }

            DialogResult confirmacion = MessageBox.Show(
                "El usuario no podra iniciar sesion mientras este inactivo. Deseas continuar?",
                "Inhabilitar usuario",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes)
            {
                return;
            }

            bool resultado = _usuarioService_380_jh.InhabilitarUsuario_380_jh(_usuarioSeleccionado_380_jh);
            MessageBox.Show(resultado ? "Usuario inhabilitado correctamente." : "No se pudo inhabilitar el usuario.");
            CargarUsuarios_380_jh();
            PrepararNuevoUsuario_380_jh();
        }

        private void CrearUsuario_380_jh()
        {
            if (!ValidarCamposAlta_380_jh())
            {
                return;
            }

            Usuario_380_jh usuario = CrearUsuarioDesdeFormulario_380_jh();
            CodigoRegistroUsuario_380_jh resultado = _usuarioService_380_jh.CrearUsuario_380_jh(usuario);

            if (resultado == CodigoRegistroUsuario_380_jh.Creado_380_jh)
            {
                MessageBox.Show("Usuario creado correctamente.");
                CargarUsuarios_380_jh();
                PrepararNuevoUsuario_380_jh();
                return;
            }

            MessageBox.Show(ObtenerMensajeRegistro_380_jh(resultado));
        }

        private void ModificarUsuario_380_jh()
        {
            if (!ValidarCamposModificacion_380_jh())
            {
                return;
            }

            Usuario_380_jh usuario = CrearUsuarioDesdeFormulario_380_jh();
            usuario.Id_380_jh = _usuarioSeleccionado_380_jh.Id_380_jh;

            bool resultado = _usuarioService_380_jh.ModificarUsuario_380_jh(usuario);
            MessageBox.Show(resultado ? "Usuario modificado correctamente." : "No se pudo modificar el usuario.");
            CargarUsuarios_380_jh();
            PrepararNuevoUsuario_380_jh();
        }

        private Usuario_380_jh CrearUsuarioDesdeFormulario_380_jh()
        {
            return new Usuario_380_jh
            {
                Username_380_jh = txtUsuario_380_jh.Text.Trim(),
                Email_380_jh = txtEmail_380_jh.Text.Trim(),
                Password_380_jh = txtPassword_380_jh.Text,
                Nombre_380_jh = txtNombre_380_jh.Text.Trim(),
                Apellido_380_jh = txtApellido_380_jh.Text.Trim(),
                Estado_380_jh = cmbEstado_380_jh.SelectedItem == null ? "ACTIVO" : cmbEstado_380_jh.SelectedItem.ToString()
            };
        }

        private bool ValidarCamposAlta_380_jh()
        {
            if (string.IsNullOrWhiteSpace(txtPassword_380_jh.Text) ||
                string.IsNullOrWhiteSpace(txtUsuario_380_jh.Text) ||
                string.IsNullOrWhiteSpace(txtEmail_380_jh.Text))
            {
                MessageBox.Show("Completa usuario, email y contrasena para crear.");
                return false;
            }

            return ValidarCamposModificacion_380_jh();
        }

        private bool ValidarCamposModificacion_380_jh()
        {
            if (string.IsNullOrWhiteSpace(txtUsuario_380_jh.Text) ||
                string.IsNullOrWhiteSpace(txtEmail_380_jh.Text))
            {
                MessageBox.Show("Completa usuario y email.");
                return false;
            }

            if (!_usuarioService_380_jh.EsEmailValido_380_jh(txtEmail_380_jh.Text))
            {
                MessageBox.Show("Ingresa un email valido.");
                return false;
            }

            return true;
        }

        private void PrepararNuevoUsuario_380_jh()
        {
            _preparandoFormulario_380_jh = true;
            _usuarioSeleccionado_380_jh = null;
            dgvUsuarios_380_jh.ClearSelection();
            txtUsuario_380_jh.Clear();
            txtEmail_380_jh.Clear();
            txtNombre_380_jh.Clear();
            txtApellido_380_jh.Clear();
            txtPassword_380_jh.Clear();
            cmbEstado_380_jh.SelectedItem = "ACTIVO";
            lblModo_380_jh.Tag = "USERS_CREATE_MODE";
            lblModo_380_jh.Text = LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("USERS_CREATE_MODE");
            btnGuardar_380_jh.Tag = "BTN_CREATE";
            btnGuardar_380_jh.Text = LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("BTN_CREATE");
            btnGuardar_380_jh.Enabled = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UsuarioCrear_380_jh);
            btnInhabilitar_380_jh.Enabled = false;
            LimpiarRolesUsuario_380_jh();
            ActualizarEstadoRolesUsuario_380_jh();
            txtUsuario_380_jh.Focus();
            _preparandoFormulario_380_jh = false;
        }

        private void ConfigurarPermisos_380_jh()
        {
            btnNuevo_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UsuarioCrear_380_jh);
            btnGuardar_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UsuarioCrear_380_jh) ||
                                 _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UsuarioEditar_380_jh);
            btnInhabilitar_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UsuarioInhabilitar_380_jh);
            btnGuardarRoles_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UsuarioEditar_380_jh);
            btnRecalcularDigitos_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UsuarioEditar_380_jh);
            ActualizarEstadoRolesUsuario_380_jh();
        }

        private void ConfigurarAsignacionRoles_380_jh()
        {
            dgvUsuarios_380_jh.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            dgvUsuarios_380_jh.Location = new Point(24, 98);
            dgvUsuarios_380_jh.Size = new Size(390, 390);

            btnRecalcularDigitos_380_jh = new Button
            {
                BackColor = Color.FromArgb(13, 110, 253),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(424, 54),
                Size = new Size(120, 31),
                Tag = "BTN_RECALCULATE_DV",
                Text = LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("BTN_RECALCULATE_DV"),
                UseVisualStyleBackColor = false
            };
            btnRecalcularDigitos_380_jh.Click += btnRecalcularDigitos_Click_380_jh;

            groupBoxRolesUsuario_380_jh = new GroupBox
            {
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                Location = new Point(424, 98),
                Size = new Size(120, 390),
                Tag = "USER_ROLES",
                Text = "Roles"
            };

            lblRolesInfo_380_jh = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(108, 117, 125),
                Location = new Point(10, 24),
                Size = new Size(100, 82),
                Tag = "USER_ROLE_SELECT_HELP",
                Text = "Selecciona un usuario para asignarle un rol."
            };

            cmbRolUsuario_380_jh = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(10, 126),
                Size = new Size(100, 23)
            };

            btnGuardarRoles_380_jh = new Button
            {
                BackColor = Color.FromArgb(25, 135, 84),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 343),
                Size = new Size(100, 31),
                Tag = "BTN_SAVE",
                Text = "Guardar",
                UseVisualStyleBackColor = false
            };
            btnGuardarRoles_380_jh.Click += btnGuardarRoles_Click_380_jh;

            groupBoxRolesUsuario_380_jh.Controls.Add(lblRolesInfo_380_jh);
            groupBoxRolesUsuario_380_jh.Controls.Add(cmbRolUsuario_380_jh);
            groupBoxRolesUsuario_380_jh.Controls.Add(btnGuardarRoles_380_jh);
            Controls.Add(btnRecalcularDigitos_380_jh);
            Controls.Add(groupBoxRolesUsuario_380_jh);
        }

        private void CargarRolesDisponibles_380_jh()
        {
            _familiasDisponibles_380_jh = _permisoService_380_jh.ListarFamilias_380_jh();
            LimpiarRolesUsuario_380_jh();
            ActualizarEstadoRolesUsuario_380_jh();
        }

        private void CargarRolesUsuario_380_jh(int idUsuario)
        {
            if (_familiasDisponibles_380_jh == null)
            {
                CargarRolesDisponibles_380_jh();
            }

            List<int> idsAsignados = _permisoService_380_jh.ListarIdsComponentesAsignadosPorUsuario_380_jh(idUsuario);

            cmbRolUsuario_380_jh.DataSource = null;
            cmbRolUsuario_380_jh.DataSource = _familiasDisponibles_380_jh;

            ComponentePermiso_380_jh rolAsignado = null;
            foreach (ComponentePermiso_380_jh familia in _familiasDisponibles_380_jh)
            {
                if (idsAsignados.Contains(familia.Id_380_jh))
                {
                    rolAsignado = familia;
                    break;
                }
            }

            cmbRolUsuario_380_jh.SelectedItem = rolAsignado;
            ActualizarEstadoRolesUsuario_380_jh();
        }

        private void LimpiarRolesUsuario_380_jh()
        {
            cmbRolUsuario_380_jh.DataSource = null;

            if (_familiasDisponibles_380_jh == null)
            {
                return;
            }

            cmbRolUsuario_380_jh.DataSource = _familiasDisponibles_380_jh;
            cmbRolUsuario_380_jh.SelectedIndex = -1;
        }

        private void ActualizarEstadoRolesUsuario_380_jh()
        {
            if (cmbRolUsuario_380_jh == null || btnGuardarRoles_380_jh == null || lblRolesInfo_380_jh == null)
            {
                return;
            }

            bool puedeEditar = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UsuarioEditar_380_jh);
            bool hayUsuarioSeleccionado = _usuarioSeleccionado_380_jh != null && _usuarioSeleccionado_380_jh.Id_380_jh > 0;
            bool hayRoles = cmbRolUsuario_380_jh.Items.Count > 0;

            cmbRolUsuario_380_jh.Enabled = puedeEditar && hayUsuarioSeleccionado && hayRoles;
            btnGuardarRoles_380_jh.Enabled = puedeEditar && hayUsuarioSeleccionado && hayRoles;

            if (!puedeEditar)
            {
                lblRolesInfo_380_jh.Text = LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("USER_ROLE_EDIT_DENIED");
                return;
            }

            if (!hayRoles)
            {
                lblRolesInfo_380_jh.Text = LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("USER_ROLE_EMPTY");
                return;
            }

            lblRolesInfo_380_jh.Text = hayUsuarioSeleccionado
                ? LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("USER_ROLE_SELECT_ONE")
                : LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("USER_ROLE_SELECT_HELP");
        }

        private void btnGuardarRoles_Click_380_jh(object sender, EventArgs e)
        {
            if (_usuarioSeleccionado_380_jh == null)
            {
                MessageBox.Show("Selecciona un usuario para asignarle roles.");
                return;
            }

            if (!_autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UsuarioEditar_380_jh))
            {
                MessageBox.Show("No tenes permisos para modificar roles de usuarios.");
                return;
            }

            ComponentePermiso_380_jh rolSeleccionado = cmbRolUsuario_380_jh.SelectedItem as ComponentePermiso_380_jh;

            if (rolSeleccionado == null)
            {
                MessageBox.Show("Selecciona un rol para el usuario.");
                return;
            }

            List<int> idsSeleccionados = new List<int> { rolSeleccionado.Id_380_jh };

            bool guardado = _permisoService_380_jh.GuardarComponentesUsuario_380_jh(_usuarioSeleccionado_380_jh.Id_380_jh, idsSeleccionados);
            MessageBox.Show(guardado ? "Rol asignado correctamente." : "No se pudo asignar el rol.");

            if (guardado)
            {
                CargarRolesUsuario_380_jh(_usuarioSeleccionado_380_jh.Id_380_jh);
            }
        }

        private void btnRecalcularDigitos_Click_380_jh(object sender, EventArgs e)
        {
            if (!_autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UsuarioEditar_380_jh))
            {
                MessageBox.Show("No tenes permisos para recalcular digitos verificadores.");
                return;
            }

            DialogResult confirmacion = MessageBox.Show(
                "Se recalcularan los digitos verificadores de todos los usuarios y se quitaran los bloqueos por DV. Deseas continuar?",
                "Recalcular digitos verificadores",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes)
            {
                return;
            }

            bool recalculado = _usuarioService_380_jh.RecalcularDigitosVerificadoresUsuarios_380_jh();
            MessageBox.Show(recalculado
                ? "Digitos verificadores recalculados correctamente."
                : "No se pudieron recalcular los digitos verificadores.");

            CargarUsuarios_380_jh();
            PrepararNuevoUsuario_380_jh();
        }

        private static string ObtenerMensajeRegistro_380_jh(CodigoRegistroUsuario_380_jh resultado)
        {
            switch (resultado)
            {
                case CodigoRegistroUsuario_380_jh.DatosInvalidos_380_jh:
                    return "Completa todos los campos obligatorios.";

                case CodigoRegistroUsuario_380_jh.EmailInvalido_380_jh:
                    return "Ingresa un email valido.";

                case CodigoRegistroUsuario_380_jh.UsuarioExistente_380_jh:
                    return "Ya existe un usuario con ese nombre.";

                case CodigoRegistroUsuario_380_jh.EmailExistente_380_jh:
                    return "Ya existe un usuario con ese email.";

                case CodigoRegistroUsuario_380_jh.IdiomaDefaultInexistente_380_jh:
                    return "No se pudo crear el usuario porque falta el idioma default.";

                default:
                    return "Ocurrio un error tecnico al crear el usuario.";
            }
        }
    }
}
