using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Application;
using Domain;
using Services;

namespace UI
{
    public class DonantesView_380_jh : LocalizedUserControl_380_jh
    {
        private readonly DonanteApplicationService_380_jh _donanteService_380_jh;
        private readonly AutorizacionApplicationService_380_jh _autorizacionService_380_jh;
        private readonly List<Donante_380_jh> _donantes_380_jh = new List<Donante_380_jh>();
        private DataGridView _grid_380_jh;
        private TextBox _txtDocumento_380_jh;
        private TextBox _txtNombre_380_jh;
        private TextBox _txtApellido_380_jh;
        private DateTimePicker _dtpNacimiento_380_jh;
        private TextBox _txtTelefono_380_jh;
        private TextBox _txtEmail_380_jh;
        private TextBox _txtDomicilio_380_jh;
        private TextBox _txtBuscar_380_jh;
        private Button _btnRegistrar_380_jh;
        private Button _btnNuevo_380_jh;
        private Button _btnVerEmail_380_jh;
        private readonly ToolTip _toolTipEmail_380_jh = new ToolTip();
        private Label _lblEstado_380_jh;
        private int? _idDonanteSeleccionado_380_jh;
        private bool _tieneEmailSeleccionado_380_jh;
        private bool _emailVisible_380_jh;
        private bool _emailModificado_380_jh;
        private bool _cargandoFormulario_380_jh;
        private string _emailEditado_380_jh;

        public DonantesView_380_jh()
            : this(new DonanteApplicationService_380_jh(), new AutorizacionApplicationService_380_jh())
        {
        }

        public DonantesView_380_jh(
            DonanteApplicationService_380_jh donanteService,
            AutorizacionApplicationService_380_jh autorizacionService)
        {
            _donanteService_380_jh = donanteService;
            _autorizacionService_380_jh = autorizacionService;
            ConstruirInterfaz_380_jh();
            new ProteccionIntegridad_380_jh(this, _btnRegistrar_380_jh);
            CargarDonantes_380_jh();
        }

        protected override void ApplyTranslations_380_jh()
        {
            base.ApplyTranslations_380_jh();
            PintarGrilla_380_jh();
            if (_btnVerEmail_380_jh != null)
            {
                ActualizarBotonEmail_380_jh();
            }
        }

        private void ConstruirInterfaz_380_jh()
        {
            BackColor = Color.White;
            Padding = new Padding(24);

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                AutoScroll = true
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 190));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);

            Label titulo = CrearLabel_380_jh("DONORS_TITLE", 18, true);
            layout.Controls.Add(titulo, 0, 0);
            layout.Controls.Add(CrearLabel_380_jh("DONORS_DESCRIPTION", 9, false), 0, 1);

            GroupBox detalle = new GroupBox
            {
                Dock = DockStyle.Fill,
                Tag = "DONORS_DETAIL",
                Padding = new Padding(10)
            };
            layout.Controls.Add(detalle, 0, 2);

            TableLayoutPanel formulario = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 3
            };
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            formulario.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            formulario.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            formulario.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            detalle.Controls.Add(formulario);

            _txtDocumento_380_jh = new TextBox { Dock = DockStyle.Fill, MaxLength = 30 };
            _txtNombre_380_jh = new TextBox { Dock = DockStyle.Fill, MaxLength = 100 };
            _txtApellido_380_jh = new TextBox { Dock = DockStyle.Fill, MaxLength = 100 };
            _dtpNacimiento_380_jh = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            _txtTelefono_380_jh = new TextBox { Dock = DockStyle.Fill, MaxLength = 50 };
            _txtEmail_380_jh = new TextBox { Dock = DockStyle.Fill, MaxLength = 150 };
            _txtEmail_380_jh.TextChanged += Email_TextChanged_380_jh;
            _txtDomicilio_380_jh = new TextBox { Dock = DockStyle.Fill, MaxLength = 255 };

            AgregarCampo_380_jh(formulario, "DONOR_DOCUMENT", _txtDocumento_380_jh, 0, 0);
            AgregarCampo_380_jh(formulario, "DONOR_NAME", _txtNombre_380_jh, 2, 0);
            AgregarCampo_380_jh(formulario, "DONOR_LASTNAME", _txtApellido_380_jh, 4, 0);
            AgregarCampo_380_jh(formulario, "DONOR_BIRTHDATE", _dtpNacimiento_380_jh, 0, 1);
            AgregarCampo_380_jh(formulario, "DONOR_PHONE", _txtTelefono_380_jh, 2, 1);
            TableLayoutPanel emailPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
            emailPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            emailPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38));
            emailPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            _btnVerEmail_380_jh = new Button
            {
                AutoSize = false,
                Size = new Size(32, 30),
                Anchor = AnchorStyles.None,
                Text = "👁",
                Font = new Font("Segoe UI Symbol", 10F),
                Enabled = false,
                Margin = Padding.Empty,
                AccessibleName = "Mostrar correo"
            };
            _btnVerEmail_380_jh.Click += VerEmail_Click_380_jh;
            emailPanel.Controls.Add(_txtEmail_380_jh, 0, 0);
            emailPanel.Controls.Add(_btnVerEmail_380_jh, 1, 0);
            _toolTipEmail_380_jh.SetToolTip(_btnVerEmail_380_jh, Traducir_380_jh("DONOR_SHOW_EMAIL"));
            AgregarCampo_380_jh(formulario, "DONOR_EMAIL", emailPanel, 4, 1);
            AgregarCampo_380_jh(formulario, "DONOR_ADDRESS", _txtDomicilio_380_jh, 0, 2);
            formulario.SetColumnSpan(_txtDomicilio_380_jh, 4);

            FlowLayoutPanel acciones = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            _btnRegistrar_380_jh = CrearBoton_380_jh("BTN_REGISTER");
            _btnRegistrar_380_jh.Click += Registrar_Click_380_jh;
            _btnNuevo_380_jh = CrearBoton_380_jh("BTN_NEW");
            _btnNuevo_380_jh.Click += Nuevo_Click_380_jh;
            acciones.Controls.Add(_btnRegistrar_380_jh);
            acciones.Controls.Add(_btnNuevo_380_jh);
            formulario.Controls.Add(acciones, 5, 2);

            Panel grillaPanel = new Panel { Dock = DockStyle.Fill };
            layout.Controls.Add(grillaPanel, 0, 3);
            FlowLayoutPanel filtro = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 36, WrapContents = false };
            _txtBuscar_380_jh = new TextBox { Width = 240 };
            _txtBuscar_380_jh.KeyDown += Buscar_KeyDown_380_jh;
            Button buscar = CrearBoton_380_jh("BTN_SEARCH");
            buscar.Click += delegate { CargarDonantes_380_jh(); };
            Button actualizar = CrearBoton_380_jh("BTN_REFRESH");
            actualizar.Click += delegate { CargarDonantes_380_jh(); };
            filtro.Controls.Add(_txtBuscar_380_jh);
            filtro.Controls.Add(buscar);
            filtro.Controls.Add(actualizar);
            _grid_380_jh = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersVisible = true,
                ColumnHeadersHeight = 34,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(222, 226, 230),
                RowHeadersVisible = false,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(248, 249, 250) },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(33, 37, 41),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding = new Padding(6, 0, 0, 0)
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(33, 37, 41),
                    SelectionBackColor = Color.FromArgb(207, 226, 255),
                    SelectionForeColor = Color.FromArgb(33, 37, 41),
                    Padding = new Padding(6, 2, 6, 2)
                },
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AgregarColumna_380_jh("id_donante", "GRID_ID");
            AgregarColumna_380_jh("documento", "DONOR_DOCUMENT");
            AgregarColumna_380_jh("nombreCompleto", "GRID_USER");
            AgregarColumna_380_jh("telefono", "DONOR_PHONE");
            AgregarColumna_380_jh("email", "DONOR_EMAIL");
            AgregarColumna_380_jh("estado", "DONOR_STATUS");
            _grid_380_jh.CellClick += Grilla_CellClick_380_jh;
            grillaPanel.Controls.Add(_grid_380_jh);

            _lblEstado_380_jh = new Label { Dock = DockStyle.Bottom, Height = 22, TextAlign = ContentAlignment.MiddleLeft };
            grillaPanel.Controls.Add(_lblEstado_380_jh);
            // Docked controls are laid out in reverse z-order: reserve filter and status space before filling the remainder.
            grillaPanel.Controls.Add(filtro);
            _btnRegistrar_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.DonanteCrear_380_jh);
        }

        private Label CrearLabel_380_jh(string key, float size, bool bold)
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                Tag = key,
                Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private static Button CrearBoton_380_jh(string key)
        {
            return new Button { AutoSize = true, Tag = key, Padding = new Padding(8, 2, 8, 2) };
        }

        private void AgregarCampo_380_jh(TableLayoutPanel tabla, string key, Control control, int columna, int fila)
        {
            Label label = CrearLabel_380_jh(key, 8, false);
            label.Dock = DockStyle.None;
            label.AutoSize = true;
            tabla.Controls.Add(label, columna, fila);
            tabla.Controls.Add(control, columna + 1, fila);
        }

        private void AgregarColumna_380_jh(string propiedad, string key)
        {
            DataGridViewTextBoxColumn columna = new DataGridViewTextBoxColumn
            {
                DataPropertyName = propiedad,
                Name = propiedad,
                HeaderText = ObtenerEncabezadoPredeterminado_380_jh(propiedad),
                Tag = key,
                FillWeight = propiedad == "nombreCompleto" || propiedad == "email" ? 150 : 85,
                MinimumWidth = propiedad == "id_donante" ? 55 : 90
            };
            _grid_380_jh.Columns.Add(columna);
        }

        private static string ObtenerEncabezadoPredeterminado_380_jh(string propiedad)
        {
            switch (propiedad)
            {
                case "id_donante": return "ID";
                case "documento": return "Documento";
                case "nombreCompleto": return "Donante";
                case "telefono": return "Teléfono";
                case "email": return "Email";
                case "estado": return "Estado";
                default: return propiedad;
            }
        }

        private void CargarDonantes_380_jh()
        {
            if (_idDonanteSeleccionado_380_jh.HasValue)
            {
                Limpiar_380_jh();
            }

            new IntegridadApplicationService_380_jh().Verificar_380_jh();
            if (_grid_380_jh == null || !_autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.DonanteVer_380_jh) &&
                !_autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.DonanteCrear_380_jh))
            {
                return;
            }

            List<Donante_380_jh> donantes = _donanteService_380_jh.Listar_380_jh(_txtBuscar_380_jh == null ? null : _txtBuscar_380_jh.Text);
            _donantes_380_jh.Clear();
            _donantes_380_jh.AddRange(donantes);
            PintarGrilla_380_jh();
        }

        private void PintarGrilla_380_jh()
        {
            if (_grid_380_jh == null)
            {
                return;
            }

            _grid_380_jh.Rows.Clear();
            foreach (Donante_380_jh donante in _donantes_380_jh)
            {
                _grid_380_jh.Rows.Add(donante.Id_380_jh, donante.Documento_380_jh, donante.NombreCompleto_380_jh,
                    donante.Telefono_380_jh, donante.TieneEmail_380_jh ? "••••••••" : "—", TraducirEstado_380_jh(donante.Estado_380_jh));
                _grid_380_jh.Rows[_grid_380_jh.Rows.Count - 1].Tag = donante;
            }

            _lblEstado_380_jh.Text = _donantes_380_jh.Count.ToString() + " " + LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("DONORS_TITLE");
        }

        private void Registrar_Click_380_jh(object sender, EventArgs e)
        {
            Donante_380_jh donante = new Donante_380_jh
            {
                Id_380_jh = _idDonanteSeleccionado_380_jh.GetValueOrDefault(),
                Documento_380_jh = _txtDocumento_380_jh.Text,
                Nombre_380_jh = _txtNombre_380_jh.Text,
                Apellido_380_jh = _txtApellido_380_jh.Text,
                FechaNacimiento_380_jh = _dtpNacimiento_380_jh.Checked ? (DateTime?)_dtpNacimiento_380_jh.Value.Date : null,
                Telefono_380_jh = _txtTelefono_380_jh.Text,
                Email_380_jh = _idDonanteSeleccionado_380_jh.HasValue
                    ? (_emailModificado_380_jh ? _emailEditado_380_jh : null)
                    : _txtEmail_380_jh.Text,
                Domicilio_380_jh = _txtDomicilio_380_jh.Text
            };

            bool editando = _idDonanteSeleccionado_380_jh.HasValue;
            CodigoOperacionPN1_380_jh resultado = editando
                ? _donanteService_380_jh.Actualizar_380_jh(donante, _emailModificado_380_jh)
                : _donanteService_380_jh.Registrar_380_jh(donante);
            string claveResultado = editando ? ObtenerClaveActualizacion_380_jh(resultado) : ObtenerClaveResultado_380_jh(resultado);
            MessageBox.Show(Traducir_380_jh(claveResultado));
            if (resultado == CodigoOperacionPN1_380_jh.Ok_380_jh)
            {
                Limpiar_380_jh();
                CargarDonantes_380_jh();
            }
        }

        private void Nuevo_Click_380_jh(object sender, EventArgs e)
        {
            Limpiar_380_jh();
        }

        private void Grilla_CellClick_380_jh(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _grid_380_jh.Rows.Count)
            {
                return;
            }

            Donante_380_jh donante = _grid_380_jh.Rows[e.RowIndex].Tag as Donante_380_jh;
            if (donante == null)
            {
                return;
            }

            CargarSeleccion_380_jh(donante);
        }

        private void CargarSeleccion_380_jh(Donante_380_jh donante)
        {
            _cargandoFormulario_380_jh = true;
            try
            {
                _idDonanteSeleccionado_380_jh = donante.Id_380_jh;
                _txtDocumento_380_jh.Text = donante.Documento_380_jh;
                _txtNombre_380_jh.Text = donante.Nombre_380_jh;
                _txtApellido_380_jh.Text = donante.Apellido_380_jh;
                _dtpNacimiento_380_jh.Checked = donante.FechaNacimiento_380_jh.HasValue;
                if (donante.FechaNacimiento_380_jh.HasValue)
                {
                    _dtpNacimiento_380_jh.Value = donante.FechaNacimiento_380_jh.Value;
                }
                _txtTelefono_380_jh.Text = donante.Telefono_380_jh;
                _txtDomicilio_380_jh.Text = donante.Domicilio_380_jh;
                _tieneEmailSeleccionado_380_jh = donante.TieneEmail_380_jh;
                _emailVisible_380_jh = false;
                _emailModificado_380_jh = false;
                _emailEditado_380_jh = null;
                _txtEmail_380_jh.Text = _tieneEmailSeleccionado_380_jh ? "••••••••" : string.Empty;
                _btnVerEmail_380_jh.Enabled = _tieneEmailSeleccionado_380_jh;
                ActualizarBotonEmail_380_jh();

                bool puedeEditar = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.DonanteCrear_380_jh);
                _txtEmail_380_jh.ReadOnly = _tieneEmailSeleccionado_380_jh || !puedeEditar;
                _txtDocumento_380_jh.ReadOnly = !puedeEditar;
                _txtNombre_380_jh.ReadOnly = !puedeEditar;
                _txtApellido_380_jh.ReadOnly = !puedeEditar;
                _dtpNacimiento_380_jh.Enabled = puedeEditar;
                _txtTelefono_380_jh.ReadOnly = !puedeEditar;
                _txtDomicilio_380_jh.ReadOnly = !puedeEditar;
                _btnRegistrar_380_jh.Visible = puedeEditar;
                _btnRegistrar_380_jh.Tag = puedeEditar ? "BTN_UPDATE_DONOR" : null;
                TranslationApplier_380_jh.Apply_380_jh(_btnRegistrar_380_jh);
            }
            finally
            {
                _cargandoFormulario_380_jh = false;
            }
        }

        private void VerEmail_Click_380_jh(object sender, EventArgs e)
        {
            if (!_idDonanteSeleccionado_380_jh.HasValue || !_tieneEmailSeleccionado_380_jh)
            {
                return;
            }

            if (_emailVisible_380_jh)
            {
                _cargandoFormulario_380_jh = true;
                _txtEmail_380_jh.Text = "••••••••";
                _txtEmail_380_jh.ReadOnly = true;
                _cargandoFormulario_380_jh = false;
                _emailVisible_380_jh = false;
                ActualizarBotonEmail_380_jh();
                return;
            }

            try
            {
                string email = _emailModificado_380_jh
                    ? _emailEditado_380_jh
                    : _donanteService_380_jh.ObtenerEmail_380_jh(_idDonanteSeleccionado_380_jh.Value);
                if (email == null)
                {
                    MessageBox.Show(Traducir_380_jh("DONOR_EMAIL_UNAVAILABLE"));
                    return;
                }

                _cargandoFormulario_380_jh = true;
                _txtEmail_380_jh.Text = email;
                _txtEmail_380_jh.ReadOnly = !_autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.DonanteCrear_380_jh);
                _cargandoFormulario_380_jh = false;
                _emailVisible_380_jh = true;
                if (_emailEditado_380_jh == null)
                {
                    _emailEditado_380_jh = email;
                }
                ActualizarBotonEmail_380_jh();
            }
            catch
            {
                _cargandoFormulario_380_jh = false;
                MessageBox.Show(Traducir_380_jh("DONOR_EMAIL_UNAVAILABLE"));
            }
        }

        private void ActualizarBotonEmail_380_jh()
        {
            _btnVerEmail_380_jh.Text = _emailVisible_380_jh ? "◉" : "👁";
            string clave = _emailVisible_380_jh ? "DONOR_HIDE_EMAIL" : "DONOR_SHOW_EMAIL";
            string texto = Traducir_380_jh(clave);
            _btnVerEmail_380_jh.AccessibleName = texto;
            _toolTipEmail_380_jh.SetToolTip(_btnVerEmail_380_jh, texto);
        }

        private void Email_TextChanged_380_jh(object sender, EventArgs e)
        {
            if (_cargandoFormulario_380_jh || !_idDonanteSeleccionado_380_jh.HasValue ||
                (!_emailVisible_380_jh && _tieneEmailSeleccionado_380_jh))
            {
                return;
            }

            _emailModificado_380_jh = true;
            _emailEditado_380_jh = _txtEmail_380_jh.Text;
        }

        private string Traducir_380_jh(string clave)
        {
            return LanguageManager_380_jh.Instance_380_jh.Translate_380_jh(clave);
        }

        private void Buscar_KeyDown_380_jh(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CargarDonantes_380_jh();
                e.SuppressKeyPress = true;
            }
        }

        private void Limpiar_380_jh()
        {
            _cargandoFormulario_380_jh = true;
            _idDonanteSeleccionado_380_jh = null;
            _tieneEmailSeleccionado_380_jh = false;
            _emailVisible_380_jh = false;
            _emailModificado_380_jh = false;
            _emailEditado_380_jh = null;
            _txtDocumento_380_jh.Clear();
            _txtNombre_380_jh.Clear();
            _txtApellido_380_jh.Clear();
            _dtpNacimiento_380_jh.Checked = false;
            _txtTelefono_380_jh.Clear();
            _txtEmail_380_jh.Clear();
            _txtEmail_380_jh.ReadOnly = false;
            _btnVerEmail_380_jh.Enabled = false;
            ActualizarBotonEmail_380_jh();
            _txtDomicilio_380_jh.Clear();
            _txtDocumento_380_jh.ReadOnly = false;
            _txtNombre_380_jh.ReadOnly = false;
            _txtApellido_380_jh.ReadOnly = false;
            _dtpNacimiento_380_jh.Enabled = true;
            _txtTelefono_380_jh.ReadOnly = false;
            _txtDomicilio_380_jh.ReadOnly = false;
            _btnRegistrar_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.DonanteCrear_380_jh);
            _btnRegistrar_380_jh.Tag = "BTN_REGISTER";
            TranslationApplier_380_jh.Apply_380_jh(_btnRegistrar_380_jh);
            _grid_380_jh.ClearSelection();
            _cargandoFormulario_380_jh = false;
            _txtDocumento_380_jh.Focus();
        }

        private static string ObtenerClaveResultado_380_jh(CodigoOperacionPN1_380_jh resultado)
        {
            switch (resultado)
            {
                case CodigoOperacionPN1_380_jh.Ok_380_jh: return "DONOR_REGISTERED";
                case CodigoOperacionPN1_380_jh.Duplicado_380_jh: return "DONOR_DUPLICATE";
                case CodigoOperacionPN1_380_jh.IntegridadInvalida_380_jh: return "INTEGRITY_BLOCKED";
                case CodigoOperacionPN1_380_jh.DatosInvalidos_380_jh: return "DONOR_INVALID";
                case CodigoOperacionPN1_380_jh.NoAutorizado_380_jh: return "OPERATION_NOT_AUTHORIZED";
                case CodigoOperacionPN1_380_jh.NoEncontrado_380_jh: return "DONOR_NOT_FOUND";
                default: return "DONOR_ERROR";
            }
        }

        private static string ObtenerClaveActualizacion_380_jh(CodigoOperacionPN1_380_jh resultado)
        {
            switch (resultado)
            {
                case CodigoOperacionPN1_380_jh.Ok_380_jh: return "DONOR_UPDATED";
                case CodigoOperacionPN1_380_jh.Duplicado_380_jh: return "DONOR_DUPLICATE";
                case CodigoOperacionPN1_380_jh.IntegridadInvalida_380_jh: return "INTEGRITY_BLOCKED";
                case CodigoOperacionPN1_380_jh.DatosInvalidos_380_jh: return "DONOR_INVALID";
                case CodigoOperacionPN1_380_jh.NoAutorizado_380_jh: return "OPERATION_NOT_AUTHORIZED";
                case CodigoOperacionPN1_380_jh.NoEncontrado_380_jh: return "DONOR_NOT_FOUND";
                default: return "DONOR_UPDATE_ERROR";
            }
        }

        private string TraducirEstado_380_jh(string estado)
        {
            return string.Equals(estado, "ACTIVO", StringComparison.OrdinalIgnoreCase)
                ? LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("STATUS_ACTIVE")
                : LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("STATUS_INACTIVE");
        }
    }
}
