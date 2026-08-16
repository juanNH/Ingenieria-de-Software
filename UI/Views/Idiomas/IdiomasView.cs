using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Application;
using Domain;
using Services;

namespace UI
{
    public class IdiomasView_380_jh : LocalizedUserControl_380_jh
    {
        private readonly IdiomaApplicationService_380_jh _idiomaService_380_jh;
        private readonly AutorizacionApplicationService_380_jh _autorizacionService_380_jh;
        private readonly Label lblTitulo_380_jh = new Label();
        private readonly Label lblDescripcion_380_jh = new Label();
        private readonly DataGridView dgvIdiomas_380_jh = new DataGridView();
        private readonly TreeView tvComponentes_380_jh = new TreeView();
        private readonly GroupBox groupIdioma_380_jh = new GroupBox();
        private readonly GroupBox groupTraduccion_380_jh = new GroupBox();
        private readonly Label lblComponentes_380_jh = new Label();
        private readonly TextBox txtComponenteSeleccionado_380_jh = new TextBox();
        private readonly TextBox txtCodigo_380_jh = new TextBox();
        private readonly TextBox txtNombre_380_jh = new TextBox();
        private readonly CheckBox chkActivo_380_jh = new CheckBox();
        private readonly Button btnNuevoIdioma_380_jh = new Button();
        private readonly Button btnGuardarIdioma_380_jh = new Button();
        private readonly TextBox txtKey_380_jh = new TextBox();
        private readonly TextBox txtDescripcionEtiqueta_380_jh = new TextBox();
        private readonly ComboBox cmbIdiomas_380_jh = new ComboBox();
        private readonly TextBox txtTraduccion_380_jh = new TextBox();
        private readonly Button btnGuardarTraduccion_380_jh = new Button();
        private readonly Button btnRefrescarComponentes_380_jh = new Button();

        private List<Idioma_380_jh> _idiomas_380_jh = new List<Idioma_380_jh>();
        private List<Etiqueta_380_jh> _etiquetas_380_jh = new List<Etiqueta_380_jh>();
        private Idioma_380_jh _idiomaSeleccionado_380_jh;
        private bool _cargandoDatos_380_jh;

        public IdiomasView_380_jh()
            : this(new IdiomaApplicationService_380_jh())
        {
        }

        public IdiomasView_380_jh(IdiomaApplicationService_380_jh idiomaService)
        {
            _idiomaService_380_jh = idiomaService;
            _autorizacionService_380_jh = new AutorizacionApplicationService_380_jh();
            ConstruirVista_380_jh();
            ConfigurarTraducciones_380_jh();
            ConfigurarPermisos_380_jh();
            CargarDatos_380_jh();
            PrepararNuevoIdioma_380_jh();
            CargarArbolComponentes_380_jh();
            Load += IdiomasView_Load_380_jh;
        }

        private void ConstruirVista_380_jh()
        {
            BackColor = Color.White;
            Size = new Size(960, 620);

            lblTitulo_380_jh.SetBounds(18, 18, 300, 36);
            lblTitulo_380_jh.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblDescripcion_380_jh.SetBounds(20, 58, 650, 24);
            lblDescripcion_380_jh.Font = new Font("Segoe UI", 10F);
            lblDescripcion_380_jh.ForeColor = Color.FromArgb(108, 117, 125);

            dgvIdiomas_380_jh.SetBounds(24, 98, 300, 170);
            dgvIdiomas_380_jh.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            dgvIdiomas_380_jh.AutoGenerateColumns = false;
            dgvIdiomas_380_jh.AllowUserToAddRows = false;
            dgvIdiomas_380_jh.AllowUserToDeleteRows = false;
            dgvIdiomas_380_jh.ReadOnly = true;
            dgvIdiomas_380_jh.MultiSelect = false;
            dgvIdiomas_380_jh.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvIdiomas_380_jh.RowHeadersVisible = false;
            dgvIdiomas_380_jh.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "Id", Width = 55, Tag = "GRID_ID" });
            dgvIdiomas_380_jh.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Codigo", HeaderText = "Codigo", Width = 90, Tag = "LANGUAGE_CODE" });
            dgvIdiomas_380_jh.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Nombre", HeaderText = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, Tag = "FIELD_NAME" });
            dgvIdiomas_380_jh.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = "Activo", HeaderText = "Activo", Width = 70, Tag = "LANGUAGE_ACTIVE" });
            dgvIdiomas_380_jh.SelectionChanged += dgvIdiomas_SelectionChanged_380_jh;

            lblComponentes_380_jh.SetBounds(24, 282, 178, 22);
            lblComponentes_380_jh.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            btnRefrescarComponentes_380_jh.SetBounds(212, 278, 112, 28);
            btnRefrescarComponentes_380_jh.Click += btnRefrescarComponentes_Click_380_jh;

            tvComponentes_380_jh.SetBounds(24, 312, 300, 260);
            tvComponentes_380_jh.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            tvComponentes_380_jh.HideSelection = false;
            tvComponentes_380_jh.AfterSelect += tvComponentes_AfterSelect_380_jh;

            groupIdioma_380_jh.SetBounds(340, 98, 520, 150);
            groupIdioma_380_jh.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupIdioma_380_jh.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);

            Label lblCodigo = CrearLabel_380_jh("Codigo", 16, 30, "LANGUAGE_CODE");
            Label lblNombre = CrearLabel_380_jh("Nombre", 150, 30, "FIELD_NAME");
            txtCodigo_380_jh.SetBounds(16, 50, 120, 25);
            txtNombre_380_jh.SetBounds(150, 50, 250, 25);
            chkActivo_380_jh.SetBounds(16, 84, 120, 25);
            btnNuevoIdioma_380_jh.SetBounds(230, 104, 82, 30);
            btnGuardarIdioma_380_jh.SetBounds(318, 104, 82, 30);
            btnNuevoIdioma_380_jh.Click += btnNuevoIdioma_Click_380_jh;
            btnGuardarIdioma_380_jh.Click += btnGuardarIdioma_Click_380_jh;
            groupIdioma_380_jh.Controls.AddRange(new Control[] { lblCodigo, lblNombre, txtCodigo_380_jh, txtNombre_380_jh, chkActivo_380_jh, btnNuevoIdioma_380_jh, btnGuardarIdioma_380_jh });

            groupTraduccion_380_jh.SetBounds(340, 258, 520, 314);
            groupTraduccion_380_jh.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            groupTraduccion_380_jh.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            txtComponenteSeleccionado_380_jh.SetBounds(16, 44, 484, 25);
            txtComponenteSeleccionado_380_jh.ReadOnly = true;
            txtKey_380_jh.SetBounds(16, 92, 210, 25);
            txtKey_380_jh.ReadOnly = true;
            txtDescripcionEtiqueta_380_jh.SetBounds(236, 92, 264, 25);
            txtDescripcionEtiqueta_380_jh.ReadOnly = true;
            cmbIdiomas_380_jh.SetBounds(16, 140, 210, 25);
            txtTraduccion_380_jh.SetBounds(16, 190, 360, 25);
            btnGuardarTraduccion_380_jh.SetBounds(386, 188, 114, 29);
            cmbIdiomas_380_jh.SelectedIndexChanged += cmbIdiomas_SelectedIndexChanged_380_jh;
            btnGuardarTraduccion_380_jh.Click += btnGuardarTraduccion_Click_380_jh;
            groupTraduccion_380_jh.Controls.AddRange(new Control[]
            {
                CrearLabel_380_jh("Componente", 16, 24, "COMPONENT_SELECTED"),
                txtComponenteSeleccionado_380_jh,
                CrearLabel_380_jh("Etiqueta", 16, 72, "LABEL_TAG"),
                CrearLabel_380_jh("Descripcion", 236, 72, "GRID_DESCRIPTION"),
                txtKey_380_jh,
                txtDescripcionEtiqueta_380_jh,
                CrearLabel_380_jh("Idioma", 16, 120, "LANGUAGE_SELECTOR"),
                CrearLabel_380_jh("Traduccion", 16, 170, "TRANSLATION_TEXT"),
                cmbIdiomas_380_jh,
                txtTraduccion_380_jh,
                btnGuardarTraduccion_380_jh
            });

            Controls.AddRange(new Control[]
            {
                lblTitulo_380_jh,
                lblDescripcion_380_jh,
                dgvIdiomas_380_jh,
                lblComponentes_380_jh,
                btnRefrescarComponentes_380_jh,
                tvComponentes_380_jh,
                groupIdioma_380_jh,
                groupTraduccion_380_jh
            });

            Resize += IdiomasView_Resize_380_jh;
            AjustarLayout_380_jh();
        }

        private Label CrearLabel_380_jh(string texto, int x, int y, string tag)
        {
            return new Label
            {
                Text = texto,
                Tag = tag,
                Location = new Point(x, y),
                Size = new Size(125, 18),
                Font = new Font("Segoe UI", 9F)
            };
        }

        private void ConfigurarTraducciones_380_jh()
        {
            lblTitulo_380_jh.Tag = "LANGUAGES_TITLE";
            lblDescripcion_380_jh.Tag = "LANGUAGES_DESCRIPTION";
            groupIdioma_380_jh.Tag = "LANGUAGE_DETAIL";
            groupTraduccion_380_jh.Tag = "TRANSLATION_DETAIL";
            lblComponentes_380_jh.Tag = "COMPONENT_TREE";
            chkActivo_380_jh.Tag = "LANGUAGE_ACTIVE";
            btnNuevoIdioma_380_jh.Tag = "BTN_NEW";
            btnGuardarIdioma_380_jh.Tag = "BTN_SAVE";
            btnGuardarTraduccion_380_jh.Tag = "BTN_SAVE";
            btnRefrescarComponentes_380_jh.Tag = "BTN_REFRESH";
        }

        private void CargarDatos_380_jh()
        {
            _cargandoDatos_380_jh = true;
            _idiomas_380_jh = _idiomaService_380_jh.ListarIdiomas_380_jh(false);
            _etiquetas_380_jh = _idiomaService_380_jh.ListarEtiquetas_380_jh();

            dgvIdiomas_380_jh.DataSource = null;
            dgvIdiomas_380_jh.DataSource = _idiomas_380_jh;

            cmbIdiomas_380_jh.DataSource = null;
            cmbIdiomas_380_jh.DisplayMember = "Nombre";
            cmbIdiomas_380_jh.ValueMember = "Id";
            cmbIdiomas_380_jh.DataSource = new List<Idioma_380_jh>(_idiomas_380_jh);
            _cargandoDatos_380_jh = false;
        }

        private void PrepararNuevoIdioma_380_jh()
        {
            _idiomaSeleccionado_380_jh = null;
            txtCodigo_380_jh.Clear();
            txtNombre_380_jh.Clear();
            chkActivo_380_jh.Checked = true;
        }

        private void dgvIdiomas_SelectionChanged_380_jh(object sender, EventArgs e)
        {
            if (dgvIdiomas_380_jh.CurrentRow == null)
            {
                return;
            }

            _idiomaSeleccionado_380_jh = dgvIdiomas_380_jh.CurrentRow.DataBoundItem as Idioma_380_jh;
            if (_idiomaSeleccionado_380_jh == null)
            {
                return;
            }

            txtCodigo_380_jh.Text = _idiomaSeleccionado_380_jh.Codigo_380_jh;
            txtNombre_380_jh.Text = _idiomaSeleccionado_380_jh.Nombre_380_jh;
            chkActivo_380_jh.Checked = _idiomaSeleccionado_380_jh.Activo_380_jh;
        }

        private void btnNuevoIdioma_Click_380_jh(object sender, EventArgs e)
        {
            if (!_autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.IdiomaCrear_380_jh))
            {
                MessageBox.Show(LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("SECURITY_LANGUAGE_CREATE_DENIED"));
                return;
            }

            PrepararNuevoIdioma_380_jh();
        }

        private void btnGuardarIdioma_Click_380_jh(object sender, EventArgs e)
        {
            if (_idiomaSeleccionado_380_jh == null &&
                !_autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.IdiomaCrear_380_jh))
            {
                MessageBox.Show(LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("SECURITY_LANGUAGE_CREATE_DENIED"));
                return;
            }

            if (_idiomaSeleccionado_380_jh != null &&
                !_autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.IdiomaEditar_380_jh))
            {
                MessageBox.Show(LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("SECURITY_LANGUAGE_EDIT_DENIED"));
                return;
            }

            Idioma_380_jh idioma = _idiomaSeleccionado_380_jh ?? new Idioma_380_jh();
            idioma.Codigo_380_jh = txtCodigo_380_jh.Text;
            idioma.Nombre_380_jh = txtNombre_380_jh.Text;
            idioma.Activo_380_jh = chkActivo_380_jh.Checked;

            Usuario_380_jh usuario = Sesion_380_jh.ObtenerInstancia_380_jh().ObtenerUsuario_380_jh();
            bool resultado = _idiomaService_380_jh.GuardarIdioma_380_jh(
                idioma,
                usuario == null ? (int?)null : usuario.Id_380_jh,
                "Cambio de activacion desde administracion de idiomas.");

            MessageBox.Show(resultado
                ? LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("LANGUAGE_SAVED")
                : LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("SAVE_ERROR"));

            CargarDatos_380_jh();
            CargarSelectorPrincipalSiCorresponde_380_jh();
            CargarArbolComponentes_380_jh();
        }

        private void btnGuardarTraduccion_Click_380_jh(object sender, EventArgs e)
        {
            if (!_autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.TraduccionEditar_380_jh))
            {
                MessageBox.Show(LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("SECURITY_TRANSLATION_EDIT_DENIED"));
                return;
            }

            Idioma_380_jh idioma = cmbIdiomas_380_jh.SelectedItem as Idioma_380_jh;

            bool resultado = _idiomaService_380_jh.GuardarTraduccionDetectada_380_jh(
                txtKey_380_jh.Text,
                txtDescripcionEtiqueta_380_jh.Text,
                idioma == null ? 0 : idioma.Id_380_jh,
                txtTraduccion_380_jh.Text);

            MessageBox.Show(resultado
                ? LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("TRANSLATION_SAVED")
                : LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("SAVE_ERROR"));

            if (resultado && idioma != null && LanguageManager_380_jh.Instance_380_jh.CurrentLanguage_380_jh != null &&
                idioma.Id_380_jh == LanguageManager_380_jh.Instance_380_jh.CurrentLanguage_380_jh.Id_380_jh)
            {
                LanguageManager_380_jh.Instance_380_jh.Notify_380_jh();
            }

            string key = txtKey_380_jh.Text;
            txtTraduccion_380_jh.Clear();
            CargarDatos_380_jh();
            SeleccionarEtiquetaPorClave_380_jh(key);
            CargarTraduccionSeleccionada_380_jh();
        }

        private void ConfigurarPermisos_380_jh()
        {
            bool puedeVerIdiomas = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.IdiomaVer_380_jh);
            bool puedeCrearIdiomas = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.IdiomaCrear_380_jh);
            bool puedeEditarIdiomas = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.IdiomaEditar_380_jh);
            bool puedeVerTraducciones = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.TraduccionVer_380_jh);
            bool puedeEditarTraducciones = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.TraduccionEditar_380_jh);

            dgvIdiomas_380_jh.Visible = puedeVerIdiomas;
            groupIdioma_380_jh.Visible = puedeVerIdiomas;
            lblComponentes_380_jh.Visible = puedeVerTraducciones;
            btnRefrescarComponentes_380_jh.Visible = puedeVerTraducciones;
            tvComponentes_380_jh.Visible = puedeVerTraducciones;
            groupTraduccion_380_jh.Visible = puedeVerTraducciones;

            btnNuevoIdioma_380_jh.Visible = puedeCrearIdiomas;
            btnGuardarIdioma_380_jh.Visible = puedeCrearIdiomas || puedeEditarIdiomas;
            btnGuardarTraduccion_380_jh.Visible = puedeEditarTraducciones;
            txtTraduccion_380_jh.ReadOnly = !puedeEditarTraducciones;
        }

        private void btnRefrescarComponentes_Click_380_jh(object sender, EventArgs e)
        {
            CargarArbolComponentes_380_jh();
        }

        private void IdiomasView_Resize_380_jh(object sender, EventArgs e)
        {
            AjustarLayout_380_jh();
        }

        private void IdiomasView_Load_380_jh(object sender, EventArgs e)
        {
            CargarArbolComponentes_380_jh();
        }

        private void cmbIdiomas_SelectedIndexChanged_380_jh(object sender, EventArgs e)
        {
            CargarTraduccionSeleccionada_380_jh();
        }

        private void tvComponentes_AfterSelect_380_jh(object sender, TreeViewEventArgs e)
        {
            UiComponentInfo_380_jh info = e.Node == null ? null : e.Node.Tag as UiComponentInfo_380_jh;
            if (info == null)
            {
                return;
            }

            txtComponenteSeleccionado_380_jh.Text = info.Path_380_jh;

            if (!string.IsNullOrWhiteSpace(info.Key_380_jh))
            {
                SeleccionarEtiquetaPorClave_380_jh(info.Key_380_jh);
                return;
            }

            txtKey_380_jh.Clear();
            txtDescripcionEtiqueta_380_jh.Text = "Componente sin etiqueta de UI. Asignar Tag en la pantalla para traducirlo.";
            txtTraduccion_380_jh.Clear();
            CargarTraduccionSeleccionada_380_jh();
        }

        private void CargarTraduccionSeleccionada_380_jh()
        {
            if (_cargandoDatos_380_jh)
            {
                return;
            }

            Idioma_380_jh idioma = cmbIdiomas_380_jh.SelectedItem as Idioma_380_jh;
            Etiqueta_380_jh etiqueta = ObtenerEtiquetaSeleccionada_380_jh();
            if (etiqueta == null || idioma == null)
            {
                txtTraduccion_380_jh.Clear();
                return;
            }

            Traduccion_380_jh traduccion = _idiomaService_380_jh.ObtenerTraduccion_380_jh(etiqueta.Id_380_jh, idioma.Id_380_jh);
            txtTraduccion_380_jh.Text = traduccion == null ? string.Empty : traduccion.Texto_380_jh;
        }

        private void SeleccionarEtiquetaPorClave_380_jh(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            foreach (Etiqueta_380_jh etiqueta in _etiquetas_380_jh)
            {
                if (string.Equals(etiqueta.Key_380_jh, key, StringComparison.OrdinalIgnoreCase))
                {
                    txtKey_380_jh.Text = etiqueta.Key_380_jh;
                    txtDescripcionEtiqueta_380_jh.Text = etiqueta.Descripcion_380_jh;
                    CargarTraduccionSeleccionada_380_jh();
                    return;
                }
            }

            txtKey_380_jh.Text = key;
            txtDescripcionEtiqueta_380_jh.Text = "Etiqueta detectada desde componente visual.";
            txtTraduccion_380_jh.Clear();
        }

        private void CargarArbolComponentes_380_jh()
        {
            tvComponentes_380_jh.BeginUpdate();
            tvComponentes_380_jh.Nodes.Clear();

            Form form = FindForm();
            if (form != null)
            {
                TreeNode formNode = CrearNodoRaiz_380_jh("Formulario: " + NombreVisible_380_jh(form));
                AgregarControles_380_jh(form.Controls, formNode, NombreVisible_380_jh(form));

                if (form.MainMenuStrip != null)
                {
                    TreeNode menuNode = CrearNodoRaiz_380_jh("Menu principal");
                    foreach (ToolStripItem item in form.MainMenuStrip.Items)
                    {
                        AgregarToolStripItem_380_jh(item, menuNode, "Menu principal");
                    }

                    formNode.Nodes.Insert(0, menuNode);
                }

                tvComponentes_380_jh.Nodes.Add(formNode);
            }
            else
            {
                TreeNode viewNode = CrearNodoRaiz_380_jh("Vista: " + GetType().Name);
                AgregarControles_380_jh(Controls, viewNode, GetType().Name);
                tvComponentes_380_jh.Nodes.Add(viewNode);
            }

            tvComponentes_380_jh.Nodes.Add(CrearNodoCatalogoEtiquetas_380_jh());
            tvComponentes_380_jh.ExpandAll();
            tvComponentes_380_jh.EndUpdate();
        }

        private TreeNode CrearNodoCatalogoEtiquetas_380_jh()
        {
            TreeNode catalogo = CrearNodoRaiz_380_jh("Etiquetas disponibles");

            AgregarSeccionCatalogo_380_jh(catalogo, "Menu", new[]
            {
                new UiCatalogItem_380_jh("MAIN_TITLE", "Titulo de la ventana principal"),
                new UiCatalogItem_380_jh("MAIN_USER", "Texto de usuario autenticado"),
                new UiCatalogItem_380_jh("MAIN_NO_SESSION", "Texto sin sesion"),
                new UiCatalogItem_380_jh("MENU_AUDIT", "Menu bitacora"),
                new UiCatalogItem_380_jh("MENU_USERS", "Menu usuarios"),
                new UiCatalogItem_380_jh("MENU_ROLES", "Menu roles"),
                new UiCatalogItem_380_jh("MENU_LANGUAGES", "Menu idiomas"),
                new UiCatalogItem_380_jh("MENU_LOGOUT", "Menu salir"),
                new UiCatalogItem_380_jh("LANGUAGE_SELECTOR", "Selector de idioma"),
                new UiCatalogItem_380_jh("SECURITY_ACCESS_DENIED", "Acceso denegado"),
                new UiCatalogItem_380_jh("NO_PERMISSIONS_ASSIGNED", "Sin permisos asignados")
            });

            AgregarSeccionCatalogo_380_jh(catalogo, "Bitacora", new[]
            {
                new UiCatalogItem_380_jh("AUDIT_TITLE", "Titulo de bitacora"),
                new UiCatalogItem_380_jh("AUDIT_DESCRIPTION", "Descripcion de bitacora"),
                new UiCatalogItem_380_jh("AUDIT_EMPTY", "Mensaje sin eventos"),
                new UiCatalogItem_380_jh("AUDIT_COUNT", "Cantidad de eventos registrados"),
                new UiCatalogItem_380_jh("AUDIT_FILTERS", "Filtros de bitacora"),
                new UiCatalogItem_380_jh("AUDIT_FILTER_FROM", "Filtro fecha desde"),
                new UiCatalogItem_380_jh("AUDIT_FILTER_TO", "Filtro fecha hasta"),
                new UiCatalogItem_380_jh("AUDIT_FILTER_USER", "Filtro usuario"),
                new UiCatalogItem_380_jh("AUDIT_FILTER_MODULE", "Filtro modulo"),
                new UiCatalogItem_380_jh("AUDIT_FILTER_ACTION", "Filtro accion"),
                new UiCatalogItem_380_jh("AUDIT_FILTER_LEVEL", "Filtro nivel"),
                new UiCatalogItem_380_jh("AUDIT_FILTER_DESCRIPTION", "Filtro descripcion"),
                new UiCatalogItem_380_jh("BTN_SEARCH", "Boton buscar"),
                new UiCatalogItem_380_jh("BTN_CLEAR_FILTERS", "Boton limpiar filtros"),
                new UiCatalogItem_380_jh("FILTER_ALL", "Todos"),
                new UiCatalogItem_380_jh("FILTER_ALL_ACTIONS", "Todas las acciones"),
                new UiCatalogItem_380_jh("BITACORA_MODULE_SECURITY", "Modulo seguridad"),
                new UiCatalogItem_380_jh("BITACORA_ACTION_LOGIN_SUCCESS", "Accion login exitoso"),
                new UiCatalogItem_380_jh("BITACORA_ACTION_LOGIN_FAILURE", "Accion login fallido"),
                new UiCatalogItem_380_jh("BITACORA_ACTION_REGISTER_FAILURE", "Accion registro fallido"),
                new UiCatalogItem_380_jh("BITACORA_LEVEL_INFORMATION", "Nivel informacion"),
                new UiCatalogItem_380_jh("BITACORA_LEVEL_WARNING", "Nivel advertencia"),
                new UiCatalogItem_380_jh("BITACORA_LEVEL_ERROR", "Nivel error"),
                new UiCatalogItem_380_jh("BTN_REFRESH", "Boton actualizar"),
                new UiCatalogItem_380_jh("GRID_ID", "Columna id"),
                new UiCatalogItem_380_jh("GRID_DATE", "Columna fecha"),
                new UiCatalogItem_380_jh("GRID_USER_ID", "Columna id usuario"),
                new UiCatalogItem_380_jh("GRID_USER", "Columna usuario"),
                new UiCatalogItem_380_jh("GRID_MODULE", "Columna modulo"),
                new UiCatalogItem_380_jh("GRID_ACTION", "Columna accion"),
                new UiCatalogItem_380_jh("GRID_LEVEL", "Columna nivel"),
                new UiCatalogItem_380_jh("GRID_DESCRIPTION", "Columna descripcion"),
                new UiCatalogItem_380_jh("GRID_DEVICE", "Columna equipo")
            });

            AgregarSeccionCatalogo_380_jh(catalogo, "Usuarios", new[]
            {
                new UiCatalogItem_380_jh("USERS_TITLE", "Titulo de usuarios"),
                new UiCatalogItem_380_jh("USERS_DESCRIPTION", "Descripcion de usuarios"),
                new UiCatalogItem_380_jh("USERS_DETAIL", "Detalle de usuario"),
                new UiCatalogItem_380_jh("USERS_CREATE_MODE", "Modo crear usuario"),
                new UiCatalogItem_380_jh("USERS_EDIT_MODE", "Modo modificar usuario"),
                new UiCatalogItem_380_jh("FIELD_USER", "Campo usuario"),
                new UiCatalogItem_380_jh("FIELD_EMAIL", "Campo email"),
                new UiCatalogItem_380_jh("FIELD_NAME", "Campo nombre"),
                new UiCatalogItem_380_jh("FIELD_LASTNAME", "Campo apellido"),
                new UiCatalogItem_380_jh("FIELD_NEW_PASSWORD", "Campo contrasena nueva"),
                new UiCatalogItem_380_jh("FIELD_STATUS", "Campo estado"),
                new UiCatalogItem_380_jh("GRID_ID", "Columna id"),
                new UiCatalogItem_380_jh("GRID_USER", "Columna usuario"),
                new UiCatalogItem_380_jh("GRID_EMAIL", "Columna email"),
                new UiCatalogItem_380_jh("GRID_STATUS", "Columna estado"),
                new UiCatalogItem_380_jh("GRID_DV_BLOCK", "Columna bloqueo digito verificador"),
                new UiCatalogItem_380_jh("BTN_NEW", "Boton nuevo"),
                new UiCatalogItem_380_jh("BTN_CREATE", "Boton crear"),
                new UiCatalogItem_380_jh("BTN_SAVE", "Boton guardar"),
                new UiCatalogItem_380_jh("BTN_DISABLE", "Boton inhabilitar"),
                new UiCatalogItem_380_jh("BTN_RECALCULATE_DV", "Boton recalcular digitos verificadores"),
                new UiCatalogItem_380_jh("USER_ROLES", "Grupo roles de usuario"),
                new UiCatalogItem_380_jh("USER_ROLE_SELECT_HELP", "Ayuda para seleccionar usuario"),
                new UiCatalogItem_380_jh("USER_ROLE_SELECT_ONE", "Ayuda para seleccionar rol"),
                new UiCatalogItem_380_jh("USER_ROLE_EMPTY", "Sin roles disponibles"),
                new UiCatalogItem_380_jh("USER_ROLE_EDIT_DENIED", "Permiso denegado para roles de usuario")
            });

            AgregarSeccionCatalogo_380_jh(catalogo, "Roles", new[]
            {
                new UiCatalogItem_380_jh("ROLES_TITLE", "Titulo de roles"),
                new UiCatalogItem_380_jh("ROLES_DESCRIPTION", "Descripcion de roles"),
                new UiCatalogItem_380_jh("ROLES_STRUCTURE", "Estructura de roles"),
                new UiCatalogItem_380_jh("ROLES_ADMIN", "Administracion de roles"),
                new UiCatalogItem_380_jh("ROLE_CODE", "Campo codigo de rol"),
                new UiCatalogItem_380_jh("ROLE_SELECTED_FAMILY", "Familia seleccionada"),
                new UiCatalogItem_380_jh("ROLE_CHILD_COMPONENT", "Permiso o familia a agregar"),
                new UiCatalogItem_380_jh("ROLE_SELECT_FAMILY", "Seleccionar familia"),
                new UiCatalogItem_380_jh("ROLE_SELECT_CHILD", "Seleccionar permiso hijo"),
                new UiCatalogItem_380_jh("ROLE_SELECT_FAMILY_AND_COMPONENT", "Seleccionar familia y componente"),
                new UiCatalogItem_380_jh("ROLE_CREATED", "Rol creado"),
                new UiCatalogItem_380_jh("ROLE_CREATE_ERROR", "Error al crear rol"),
                new UiCatalogItem_380_jh("ROLE_RELATION_ADDED", "Relacion agregada"),
                new UiCatalogItem_380_jh("ROLE_RELATION_ADD_ERROR", "Error al agregar relacion"),
                new UiCatalogItem_380_jh("ROLE_RELATION_REMOVED", "Relacion quitada"),
                new UiCatalogItem_380_jh("ROLE_RELATION_REMOVE_ERROR", "Error al quitar relacion"),
                new UiCatalogItem_380_jh("ROLE_RELATION_IDENTIFY_ERROR", "Error al identificar relacion"),
                new UiCatalogItem_380_jh("ROLE_SELF_REFERENCE_ERROR", "Error por autoreferencia"),
                new UiCatalogItem_380_jh("ROLE_INVALID_PARENT", "Padre invalido"),
                new UiCatalogItem_380_jh("ROLE_INVALID_CHILD", "Hijo invalido"),
                new UiCatalogItem_380_jh("ROLE_CYCLE_ERROR", "Ciclo detectado"),
                new UiCatalogItem_380_jh("BTN_CREATE_ROLE", "Boton crear rol"),
                new UiCatalogItem_380_jh("BTN_ADD", "Boton agregar"),
                new UiCatalogItem_380_jh("BTN_REMOVE_SELECTED", "Boton quitar seleccionado"),
                new UiCatalogItem_380_jh("BTN_REMOVE_FROM", "Boton quitar desde familia"),
                new UiCatalogItem_380_jh("SECURITY_ROLE_CREATE_DENIED", "Permiso denegado para crear roles"),
                new UiCatalogItem_380_jh("SECURITY_ROLE_EDIT_DENIED", "Permiso denegado para modificar roles")
            });

            return catalogo;
        }

        private void AgregarSeccionCatalogo_380_jh(TreeNode parentNode, string nombre, IEnumerable<UiCatalogItem_380_jh> items)
        {
            TreeNode seccion = CrearNodoRaiz_380_jh(nombre);

            foreach (UiCatalogItem_380_jh item in items)
            {
                seccion.Nodes.Add(CrearNodoComponente_380_jh(
                    "Etiqueta",
                    item.Key_380_jh,
                    item.Description_380_jh,
                    item.Key_380_jh,
                    nombre + "/" + item.Key_380_jh));
            }

            parentNode.Nodes.Add(seccion);
        }

        private TreeNode CrearNodoRaiz_380_jh(string texto)
        {
            return new TreeNode(texto)
            {
                Tag = new UiComponentInfo_380_jh
                {
                    Path_380_jh = texto,
                    SuggestedKey_380_jh = NormalizarClave_380_jh(texto)
                }
            };
        }

        private void AgregarControles_380_jh(Control.ControlCollection controls, TreeNode parentNode, string parentPath)
        {
            foreach (Control control in controls)
            {
                string path = parentPath + "/" + NombreVisible_380_jh(control);
                TreeNode node = CrearNodoComponente_380_jh(control.GetType().Name, NombreVisible_380_jh(control), control.Text, control.Tag as string, path);
                parentNode.Nodes.Add(node);

                DataGridView dataGridView = control as DataGridView;
                if (dataGridView != null)
                {
                    AgregarColumnas_380_jh(dataGridView, node, path);
                }

                if (control.Controls.Count > 0)
                {
                    AgregarControles_380_jh(control.Controls, node, path);
                }
            }
        }

        private void AgregarColumnas_380_jh(DataGridView dataGridView, TreeNode parentNode, string parentPath)
        {
            foreach (DataGridViewColumn column in dataGridView.Columns)
            {
                string path = parentPath + "/Column:" + NombreVisible_380_jh(column);
                parentNode.Nodes.Add(CrearNodoComponente_380_jh(
                    "DataGridViewColumn",
                    NombreVisible_380_jh(column),
                    column.HeaderText,
                    column.Tag as string,
                    path));
            }
        }

        private void AgregarToolStripItem_380_jh(ToolStripItem item, TreeNode parentNode, string parentPath)
        {
            string path = parentPath + "/" + NombreVisible_380_jh(item);
            TreeNode node = CrearNodoComponente_380_jh(item.GetType().Name, NombreVisible_380_jh(item), item.Text, item.Tag as string, path);
            parentNode.Nodes.Add(node);

            ToolStripDropDownItem dropDownItem = item as ToolStripDropDownItem;
            if (dropDownItem == null)
            {
                return;
            }

            foreach (ToolStripItem child in dropDownItem.DropDownItems)
            {
                AgregarToolStripItem_380_jh(child, node, path);
            }
        }

        private TreeNode CrearNodoComponente_380_jh(string tipo, string nombre, string texto, string key, string path)
        {
            string detalleClave = string.IsNullOrWhiteSpace(key) ? "sin etiqueta" : key;
            string detalleTexto = string.IsNullOrWhiteSpace(texto) ? string.Empty : " - " + texto;
            return new TreeNode(tipo + ": " + nombre + " [" + detalleClave + "]" + detalleTexto)
            {
                Tag = new UiComponentInfo_380_jh
                {
                    Key_380_jh = key,
                    Path_380_jh = path,
                    SuggestedKey_380_jh = NormalizarClave_380_jh(path),
                    CurrentText_380_jh = texto
                }
            };
        }

        private static string NombreVisible_380_jh(Control control)
        {
            if (!string.IsNullOrWhiteSpace(control.Name))
            {
                return control.Name;
            }

            if (!string.IsNullOrWhiteSpace(control.Text))
            {
                return control.Text;
            }

            return control.GetType().Name;
        }

        private static string NombreVisible_380_jh(DataGridViewColumn column)
        {
            if (!string.IsNullOrWhiteSpace(column.Name))
            {
                return column.Name;
            }

            if (!string.IsNullOrWhiteSpace(column.HeaderText))
            {
                return column.HeaderText;
            }

            return "Column";
        }

        private static string NombreVisible_380_jh(ToolStripItem item)
        {
            if (!string.IsNullOrWhiteSpace(item.Name))
            {
                return item.Name;
            }

            if (!string.IsNullOrWhiteSpace(item.Text))
            {
                return item.Text;
            }

            return item.GetType().Name;
        }

        private static string NormalizarClave_380_jh(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "UI_COMPONENT";
            }

            StringBuilder builder = new StringBuilder();
            foreach (char c in value)
            {
                if (char.IsLetterOrDigit(c))
                {
                    builder.Append(char.ToUpperInvariant(c));
                }
                else if (builder.Length > 0 && builder[builder.Length - 1] != '_')
                {
                    builder.Append('_');
                }
            }

            return builder.ToString().Trim('_');
        }

        private void CargarSelectorPrincipalSiCorresponde_380_jh()
        {
            Form form = FindForm();
            MainForm_380_jh mainForm = form as MainForm_380_jh;
            if (mainForm != null)
            {
                mainForm.RefrescarIdiomasDisponibles_380_jh();
            }
        }

        private void AjustarLayout_380_jh()
        {
            int margen = 24;
            int anchoDisponible = Math.Max(900, ClientSize.Width);
            int altoDisponible = Math.Max(580, ClientSize.Height);
            int anchoIzquierdo = Math.Min(620, Math.Max(420, (int)(anchoDisponible * 0.40)));
            int xDerecha = margen + anchoIzquierdo + 24;
            int anchoDerecha = Math.Max(430, anchoDisponible - xDerecha - margen);

            lblTitulo_380_jh.SetBounds(margen, 18, anchoDisponible - (margen * 2), 36);
            lblDescripcion_380_jh.SetBounds(margen + 2, 58, anchoDisponible - (margen * 2), 24);

            dgvIdiomas_380_jh.SetBounds(margen, 98, anchoIzquierdo, 170);
            lblComponentes_380_jh.SetBounds(margen, 282, anchoIzquierdo - 130, 22);
            btnRefrescarComponentes_380_jh.SetBounds(margen + anchoIzquierdo - 112, 278, 112, 28);
            tvComponentes_380_jh.SetBounds(margen, 312, anchoIzquierdo, Math.Max(230, altoDisponible - 336));

            groupIdioma_380_jh.SetBounds(xDerecha, 98, anchoDerecha, 150);
            groupTraduccion_380_jh.SetBounds(xDerecha, 258, anchoDerecha, Math.Max(314, altoDisponible - 282));

            txtNombre_380_jh.Width = Math.Max(250, groupIdioma_380_jh.Width - txtNombre_380_jh.Left - 24);
            btnGuardarIdioma_380_jh.Left = groupIdioma_380_jh.Width - btnGuardarIdioma_380_jh.Width - 24;
            btnNuevoIdioma_380_jh.Left = btnGuardarIdioma_380_jh.Left - btnNuevoIdioma_380_jh.Width - 8;

            txtComponenteSeleccionado_380_jh.Width = Math.Max(300, groupTraduccion_380_jh.Width - 32);
            txtDescripcionEtiqueta_380_jh.Width = Math.Max(240, groupTraduccion_380_jh.Width - txtDescripcionEtiqueta_380_jh.Left - 16);
            cmbIdiomas_380_jh.Width = Math.Max(210, (groupTraduccion_380_jh.Width - 32) / 2);
            txtTraduccion_380_jh.Width = Math.Max(300, groupTraduccion_380_jh.Width - btnGuardarTraduccion_380_jh.Width - 48);
            btnGuardarTraduccion_380_jh.Left = txtTraduccion_380_jh.Right + 16;
        }

        private Etiqueta_380_jh ObtenerEtiquetaSeleccionada_380_jh()
        {
            foreach (Etiqueta_380_jh etiqueta in _etiquetas_380_jh)
            {
                if (string.Equals(etiqueta.Key_380_jh, txtKey_380_jh.Text, StringComparison.OrdinalIgnoreCase))
                {
                    return etiqueta;
                }
            }

            return null;
        }

        private class UiComponentInfo_380_jh
        {
            public string Key_380_jh { get; set; }
            public string SuggestedKey_380_jh { get; set; }
            public string Path_380_jh { get; set; }
            public string CurrentText_380_jh { get; set; }
        }

        private class UiCatalogItem_380_jh
        {
            public UiCatalogItem_380_jh(string key, string description)
            {
                Key_380_jh = key;
                Description_380_jh = description;
            }

            public string Key_380_jh { get; private set; }
            public string Description_380_jh { get; private set; }
        }
    }
}
