using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Application;
using Domain;
using Services;

namespace UI
{
    public partial class RolesView_380_jh : LocalizedUserControl_380_jh
    {
        private readonly PermisoApplicationService_380_jh _permisoService_380_jh;
        private readonly AutorizacionApplicationService_380_jh _autorizacionService_380_jh;
        private TextBox txtCodigoRol_380_jh;
        private TextBox txtNombreRol_380_jh;
        private TextBox txtDescripcionRol_380_jh;
        private Label lblFamiliaSeleccionada_380_jh;
        private ComboBox cmbComponenteHijo_380_jh;
        private Button btnCrearRol_380_jh;
        private Button btnAgregarHijo_380_jh;
        private Button btnQuitarHijo_380_jh;

        public RolesView_380_jh()
            : this(new PermisoApplicationService_380_jh())
        {
        }

        public RolesView_380_jh(PermisoApplicationService_380_jh permisoService)
        {
            _permisoService_380_jh = permisoService;
            _autorizacionService_380_jh = new AutorizacionApplicationService_380_jh();
            InitializeComponent_380_jh();
            ConfigurarTraducciones_380_jh();
            ConfigurarAdministracion_380_jh();
            CargarArbol_380_jh();
            CargarCombos_380_jh();
            ActualizarAccionesSeleccionadas_380_jh();
        }

        private void ConfigurarTraducciones_380_jh()
        {
            lblTitulo_380_jh.Tag = "ROLES_TITLE";
            lblDescripcion_380_jh.Tag = "ROLES_DESCRIPTION";
            groupBoxRoles_380_jh.Tag = "ROLES_STRUCTURE";
        }

        protected override void ApplyTranslations_380_jh()
        {
            base.ApplyTranslations_380_jh();
            if (lblFamiliaSeleccionada_380_jh != null)
            {
                ActualizarAccionesSeleccionadas_380_jh();
            }
        }

        private void CargarArbol_380_jh()
        {
            treeViewRoles_380_jh.Nodes.Clear();
            List<ComponentePermiso_380_jh> componentes = _permisoService_380_jh.ListarArbolCompleto_380_jh();

            foreach (ComponentePermiso_380_jh componente in componentes)
            {
                AgregarNodo_380_jh(componente, treeViewRoles_380_jh.Nodes);
            }

            treeViewRoles_380_jh.AfterSelect -= treeViewRoles_AfterSelect_380_jh;
            treeViewRoles_380_jh.AfterSelect += treeViewRoles_AfterSelect_380_jh;
            treeViewRoles_380_jh.ExpandAll();
        }

        private void ConfigurarAdministracion_380_jh()
        {
            groupBoxRoles_380_jh.Location = new Point(24, 98);
            groupBoxRoles_380_jh.Size = new Size(500, 390);

            GroupBox groupBoxEdicion = new GroupBox
            {
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                Location = new Point(548, 98),
                Name = "groupBoxEdicionRoles",
                Size = new Size(328, 390),
                Tag = "ROLES_ADMIN",
                Text = "Administracion de roles"
            };

            Label lblCodigo = CrearLabel_380_jh("Codigo", 18, 30, "ROLE_CODE");
            txtCodigoRol_380_jh = CrearTextBox_380_jh(18, 48);

            Label lblNombre = CrearLabel_380_jh("Nombre", 18, 80, "FIELD_NAME");
            txtNombreRol_380_jh = CrearTextBox_380_jh(18, 98);

            Label lblDescripcionRol = CrearLabel_380_jh("Descripcion", 18, 130, "GRID_DESCRIPTION");
            txtDescripcionRol_380_jh = CrearTextBox_380_jh(18, 148);

            btnCrearRol_380_jh = CrearBoton_380_jh("Crear rol", 18, 181, "BTN_CREATE_ROLE");
            btnCrearRol_380_jh.Click += btnCrearRol_Click_380_jh;

            Label lblRolPadre = CrearLabel_380_jh("Familia seleccionada en el arbol", 18, 222, "ROLE_SELECTED_FAMILY");
            lblFamiliaSeleccionada_380_jh = new Label
            {
                AutoSize = false,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(18, 240),
                Size = new Size(280, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label lblHijo = CrearLabel_380_jh("Permiso o familia a agregar", 18, 272, "ROLE_CHILD_COMPONENT");
            cmbComponenteHijo_380_jh = CrearCombo_380_jh(18, 290);

            btnAgregarHijo_380_jh = CrearBoton_380_jh("Agregar", 18, 326, "BTN_ADD");
            btnAgregarHijo_380_jh.Click += btnAgregarHijo_Click_380_jh;

            btnQuitarHijo_380_jh = CrearBoton_380_jh("Quitar seleccionado", 128, 326, "BTN_REMOVE_SELECTED");
            btnQuitarHijo_380_jh.Size = new Size(170, 31);
            btnQuitarHijo_380_jh.Click += btnQuitarHijo_Click_380_jh;

            groupBoxEdicion.Controls.Add(lblCodigo);
            groupBoxEdicion.Controls.Add(txtCodigoRol_380_jh);
            groupBoxEdicion.Controls.Add(lblNombre);
            groupBoxEdicion.Controls.Add(txtNombreRol_380_jh);
            groupBoxEdicion.Controls.Add(lblDescripcionRol);
            groupBoxEdicion.Controls.Add(txtDescripcionRol_380_jh);
            groupBoxEdicion.Controls.Add(btnCrearRol_380_jh);
            groupBoxEdicion.Controls.Add(lblRolPadre);
            groupBoxEdicion.Controls.Add(lblFamiliaSeleccionada_380_jh);
            groupBoxEdicion.Controls.Add(lblHijo);
            groupBoxEdicion.Controls.Add(cmbComponenteHijo_380_jh);
            groupBoxEdicion.Controls.Add(btnAgregarHijo_380_jh);
            groupBoxEdicion.Controls.Add(btnQuitarHijo_380_jh);
            Controls.Add(groupBoxEdicion);

            btnCrearRol_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.RolCrear_380_jh);
            btnAgregarHijo_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.RolEditar_380_jh);
            btnQuitarHijo_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.RolEditar_380_jh);
        }

        private void CargarCombos_380_jh()
        {
            List<ComponentePermiso_380_jh> componentes = _permisoService_380_jh.ListarComponentes_380_jh();

            cmbComponenteHijo_380_jh.DataSource = null;
            cmbComponenteHijo_380_jh.DataSource = componentes;
        }

        private void btnCrearRol_Click_380_jh(object sender, System.EventArgs e)
        {
            if (!_autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.RolCrear_380_jh))
            {
                MessageBox.Show(LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("SECURITY_ROLE_CREATE_DENIED"));
                return;
            }

            bool creado = _permisoService_380_jh.CrearFamilia_380_jh(
                txtCodigoRol_380_jh.Text,
                txtNombreRol_380_jh.Text,
                txtDescripcionRol_380_jh.Text);

            MessageBox.Show(creado
                ? LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("ROLE_CREATED")
                : LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("ROLE_CREATE_ERROR"));

            if (creado)
            {
                txtCodigoRol_380_jh.Clear();
                txtNombreRol_380_jh.Clear();
                txtDescripcionRol_380_jh.Clear();
                CargarArbol_380_jh();
                CargarCombos_380_jh();
                ActualizarAccionesSeleccionadas_380_jh();
            }
        }

        private void btnAgregarHijo_Click_380_jh(object sender, System.EventArgs e)
        {
            if (!_autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.RolEditar_380_jh))
            {
                MessageBox.Show(LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("SECURITY_ROLE_EDIT_DENIED"));
                return;
            }

            ComponentePermiso_380_jh padre = ObtenerFamiliaSeleccionada_380_jh();
            ComponentePermiso_380_jh hijo = cmbComponenteHijo_380_jh.SelectedItem as ComponentePermiso_380_jh;

            if (padre == null || hijo == null)
            {
                MessageBox.Show(LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("ROLE_SELECT_FAMILY_AND_COMPONENT"));
                return;
            }

            string resultado = _permisoService_380_jh.AgregarRelacion_380_jh(padre.Id_380_jh, hijo.Id_380_jh);
            MessageBox.Show(ObtenerMensajeRelacion_380_jh(resultado));
            CargarArbol_380_jh();
            CargarCombos_380_jh();
            ActualizarAccionesSeleccionadas_380_jh();
        }

        private void btnQuitarHijo_Click_380_jh(object sender, System.EventArgs e)
        {
            if (!_autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.RolEditar_380_jh))
            {
                MessageBox.Show(LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("SECURITY_ROLE_EDIT_DENIED"));
                return;
            }

            if (treeViewRoles_380_jh.SelectedNode == null || treeViewRoles_380_jh.SelectedNode.Parent == null)
            {
                MessageBox.Show(LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("ROLE_SELECT_CHILD"));
                return;
            }

            ComponentePermiso_380_jh padre = treeViewRoles_380_jh.SelectedNode.Parent.Tag as ComponentePermiso_380_jh;
            ComponentePermiso_380_jh hijo = treeViewRoles_380_jh.SelectedNode.Tag as ComponentePermiso_380_jh;

            if (padre == null || hijo == null)
            {
                MessageBox.Show(LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("ROLE_RELATION_IDENTIFY_ERROR"));
                return;
            }

            bool quitado = _permisoService_380_jh.QuitarRelacion_380_jh(padre.Id_380_jh, hijo.Id_380_jh);
            MessageBox.Show(quitado
                ? LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("ROLE_RELATION_REMOVED")
                : LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("ROLE_RELATION_REMOVE_ERROR"));
            CargarArbol_380_jh();
            CargarCombos_380_jh();
            ActualizarAccionesSeleccionadas_380_jh();
        }

        private void treeViewRoles_AfterSelect_380_jh(object sender, TreeViewEventArgs e)
        {
            ActualizarAccionesSeleccionadas_380_jh();
        }

        private static void AgregarNodo_380_jh(ComponentePermiso_380_jh componente, TreeNodeCollection destino)
        {
            TreeNode nodo = new TreeNode(FormatearTexto_380_jh(componente))
            {
                Tag = componente
            };

            destino.Add(nodo);

            foreach (ComponentePermiso_380_jh hijo in componente.ObtenerHijos_380_jh())
            {
                AgregarNodo_380_jh(hijo, nodo.Nodes);
            }
        }

        private static string FormatearTexto_380_jh(ComponentePermiso_380_jh componente)
        {
            return componente.Tipo_380_jh == TipoComponentePermiso_380_jh.Familia_380_jh
                ? "Familia: " + componente.Nombre_380_jh
                : componente.Codigo_380_jh + " - " + componente.Nombre_380_jh;
        }

        private ComponentePermiso_380_jh ObtenerFamiliaSeleccionada_380_jh()
        {
            if (treeViewRoles_380_jh.SelectedNode == null)
            {
                return null;
            }

            ComponentePermiso_380_jh componente = treeViewRoles_380_jh.SelectedNode.Tag as ComponentePermiso_380_jh;

            if (componente != null && componente.Tipo_380_jh == TipoComponentePermiso_380_jh.Familia_380_jh)
            {
                return componente;
            }

            return null;
        }

        private void ActualizarAccionesSeleccionadas_380_jh()
        {
            ComponentePermiso_380_jh familiaSeleccionada = ObtenerFamiliaSeleccionada_380_jh();
            lblFamiliaSeleccionada_380_jh.Text = familiaSeleccionada == null
                ? LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("ROLE_SELECT_FAMILY")
                : familiaSeleccionada.Nombre_380_jh;

            btnAgregarHijo_380_jh.Enabled = familiaSeleccionada != null &&
                                     _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.RolEditar_380_jh);

            if (treeViewRoles_380_jh.SelectedNode != null && treeViewRoles_380_jh.SelectedNode.Parent != null)
            {
                ComponentePermiso_380_jh hijo = treeViewRoles_380_jh.SelectedNode.Tag as ComponentePermiso_380_jh;
                ComponentePermiso_380_jh padre = treeViewRoles_380_jh.SelectedNode.Parent.Tag as ComponentePermiso_380_jh;
                btnQuitarHijo_380_jh.Text = hijo == null || padre == null
                    ? LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("BTN_REMOVE_SELECTED")
                    : string.Format(LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("BTN_REMOVE_FROM"), padre.Nombre_380_jh);
                btnQuitarHijo_380_jh.Enabled = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.RolEditar_380_jh);
                return;
            }

            btnQuitarHijo_380_jh.Text = LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("BTN_REMOVE_SELECTED");
            btnQuitarHijo_380_jh.Enabled = false;
        }

        private static Label CrearLabel_380_jh(string texto, int x, int y, string tag)
        {
            return new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(x, y),
                Tag = tag,
                Text = texto
            };
        }

        private static TextBox CrearTextBox_380_jh(int x, int y)
        {
            return new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(x, y),
                Size = new Size(280, 25)
            };
        }

        private static ComboBox CrearCombo_380_jh(int x, int y)
        {
            return new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(x, y),
                Size = new Size(280, 25)
            };
        }

        private static Button CrearBoton_380_jh(string texto, int x, int y, string tag)
        {
            return new Button
            {
                BackColor = Color.FromArgb(13, 110, 253),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(x, y),
                Size = new Size(100, 31),
                Tag = tag,
                Text = texto,
                UseVisualStyleBackColor = false
            };
        }

        private static string ObtenerMensajeRelacion_380_jh(string resultado)
        {
            switch (resultado)
            {
                case "OK":
                    return LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("ROLE_RELATION_ADDED");

                case "AUTO_REFERENCIA":
                    return LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("ROLE_SELF_REFERENCE_ERROR");

                case "PADRE_INVALIDO":
                    return LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("ROLE_INVALID_PARENT");

                case "HIJO_INVALIDO":
                    return LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("ROLE_INVALID_CHILD");

                case "CICLO_DETECTADO":
                    return LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("ROLE_CYCLE_ERROR");

                default:
                    return LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("ROLE_RELATION_ADD_ERROR");
            }
        }
    }
}
