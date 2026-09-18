using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Application;
using Domain;
using Services;

namespace UI
{
    public class UnidadesView_380_jh : LocalizedUserControl_380_jh
    {
        private readonly UnidadApplicationService_380_jh _unidadService_380_jh;
        private readonly AutorizacionApplicationService_380_jh _autorizacionService_380_jh;
        private readonly List<Unidad_380_jh> _unidades_380_jh = new List<Unidad_380_jh>();
        private DataGridView _grid_380_jh;
        private TextBox _txtBuscar_380_jh;
        private TextBox _txtComponente_380_jh;
        private ComboBox _cmbGrupo_380_jh;
        private ComboBox _cmbRh_380_jh;
        private DateTimePicker _dtpVencimiento_380_jh;
        private TextBox _txtObservaciones_380_jh;
        private Button _btnClasificar_380_jh;
        private Button _btnLiberar_380_jh;
        private Button _btnBloquear_380_jh;
        private Button _btnDescartar_380_jh;
        private Label _lblEstado_380_jh;

        public UnidadesView_380_jh()
            : this(new UnidadApplicationService_380_jh(), new AutorizacionApplicationService_380_jh())
        {
        }

        public UnidadesView_380_jh(
            UnidadApplicationService_380_jh unidadService,
            AutorizacionApplicationService_380_jh autorizacionService)
        {
            _unidadService_380_jh = unidadService;
            _autorizacionService_380_jh = autorizacionService;
            ConstruirInterfaz_380_jh();
            new ProteccionIntegridad_380_jh(this, _btnClasificar_380_jh, _btnLiberar_380_jh, _btnBloquear_380_jh, _btnDescartar_380_jh);
            CargarUnidades_380_jh();
        }

        protected override void ApplyTranslations_380_jh()
        {
            base.ApplyTranslations_380_jh();
            PintarGrilla_380_jh();
        }

        private void ConstruirInterfaz_380_jh()
        {
            BackColor = Color.White;
            Padding = new Padding(24);

            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);
            layout.Controls.Add(CrearLabel_380_jh("UNITS_TITLE", 18, true), 0, 0);
            layout.Controls.Add(CrearLabel_380_jh("UNITS_DESCRIPTION", 9, false), 0, 1);

            FlowLayoutPanel filtro = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            Label etiquetaBusqueda = CrearLabel_380_jh("UNIT_CODE", 8, false);
            etiquetaBusqueda.AutoSize = true;
            etiquetaBusqueda.Dock = DockStyle.None;
            etiquetaBusqueda.Padding = new Padding(0, 6, 4, 0);
            filtro.Controls.Add(etiquetaBusqueda);
            _txtBuscar_380_jh = new TextBox { Width = 240 };
            _txtBuscar_380_jh.KeyDown += Buscar_KeyDown_380_jh;
            filtro.Controls.Add(_txtBuscar_380_jh);
            Button buscar = CrearBoton_380_jh("BTN_SEARCH");
            buscar.Click += delegate { CargarUnidades_380_jh(); };
            Button actualizar = CrearBoton_380_jh("BTN_REFRESH");
            actualizar.Click += delegate { CargarUnidades_380_jh(); };
            filtro.Controls.Add(buscar);
            filtro.Controls.Add(actualizar);
            layout.Controls.Add(filtro, 0, 2);

            SplitContainer split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 50,
                Panel1MinSize = 25,
                Panel2MinSize = 25
            };
            split.SizeChanged += Split_SizeChanged_380_jh;
            layout.Controls.Add(split, 0, 3);

            Panel grilla = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 4, 12, 0) };
            split.Panel1.Controls.Add(grilla);
            _grid_380_jh = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            AgregarColumna_380_jh("codigo", "UNIT_CODE", 115);
            AgregarColumna_380_jh("donante", "UNIT_DONOR", 150);
            AgregarColumna_380_jh("componente", "UNIT_COMPONENT", 105);
            AgregarColumna_380_jh("grupo", "UNIT_BLOOD_GROUP", 55);
            AgregarColumna_380_jh("rh", "UNIT_RH", 45);
            AgregarColumna_380_jh("vencimiento", "UNIT_EXPIRATION", 90);
            AgregarColumna_380_jh("estado", "UNIT_STATUS", 100);
            _grid_380_jh.SelectionChanged += Grid_SelectionChanged_380_jh;
            grilla.Controls.Add(_grid_380_jh);
            _lblEstado_380_jh = new Label { Dock = DockStyle.Bottom, Height = 24, TextAlign = ContentAlignment.MiddleLeft };
            grilla.Controls.Add(_lblEstado_380_jh);

            GroupBox detalle = new GroupBox { Dock = DockStyle.Fill, Tag = "UNIT_DETAIL", Padding = new Padding(10) };
            split.Panel2.Controls.Add(detalle);
            TableLayoutPanel formulario = new TableLayoutPanel { Dock = DockStyle.Top, Height = 250, ColumnCount = 2, RowCount = 6 };
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            detalle.Controls.Add(formulario);

            _txtComponente_380_jh = new TextBox { Dock = DockStyle.Fill, MaxLength = 100 };
            _cmbGrupo_380_jh = new ComboBox { Dock = DockStyle.Left, DropDownStyle = ComboBoxStyle.DropDownList, Width = 100 };
            _cmbGrupo_380_jh.Items.AddRange(new object[] { "A", "B", "AB", "O" });
            _cmbRh_380_jh = new ComboBox { Dock = DockStyle.Left, DropDownStyle = ComboBoxStyle.DropDownList, Width = 100 };
            _cmbRh_380_jh.Items.AddRange(new object[] { "+", "-" });
            _dtpVencimiento_380_jh = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short };
            _txtObservaciones_380_jh = new TextBox { Dock = DockStyle.Fill, Multiline = true, Height = 55, MaxLength = 500, ScrollBars = ScrollBars.Vertical };
            AgregarCampo_380_jh(formulario, "UNIT_COMPONENT", _txtComponente_380_jh, 0);
            AgregarCampo_380_jh(formulario, "UNIT_BLOOD_GROUP", _cmbGrupo_380_jh, 1);
            AgregarCampo_380_jh(formulario, "UNIT_RH", _cmbRh_380_jh, 2);
            AgregarCampo_380_jh(formulario, "UNIT_EXPIRATION", _dtpVencimiento_380_jh, 3);
            AgregarCampo_380_jh(formulario, "UNIT_OBSERVATIONS", _txtObservaciones_380_jh, 4);
            formulario.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            formulario.Controls.Add(new Label(), 0, 5);

            FlowLayoutPanel acciones = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
            _btnClasificar_380_jh = CrearBoton_380_jh("UNIT_CLASSIFY");
            _btnClasificar_380_jh.Click += Clasificar_Click_380_jh;
            _btnLiberar_380_jh = CrearBoton_380_jh("UNIT_RELEASE");
            _btnLiberar_380_jh.Click += delegate { CambiarEstado_Click_380_jh(EstadoUnidadPN1_380_jh.Liberada_380_jh); };
            _btnBloquear_380_jh = CrearBoton_380_jh("UNIT_BLOCK");
            _btnBloquear_380_jh.Click += delegate { CambiarEstado_Click_380_jh(EstadoUnidadPN1_380_jh.Bloqueada_380_jh); };
            _btnDescartar_380_jh = CrearBoton_380_jh("UNIT_DISCARD");
            _btnDescartar_380_jh.Click += delegate { CambiarEstado_Click_380_jh(EstadoUnidadPN1_380_jh.Descartada_380_jh); };
            acciones.Controls.Add(_btnClasificar_380_jh);
            acciones.Controls.Add(_btnLiberar_380_jh);
            acciones.Controls.Add(_btnBloquear_380_jh);
            acciones.Controls.Add(_btnDescartar_380_jh);
            formulario.Controls.Add(acciones, 1, 5);

            ConfigurarPermisos_380_jh();
        }

        private static void Split_SizeChanged_380_jh(object sender, EventArgs e)
        {
            SplitContainer split = sender as SplitContainer;
            if (split == null || split.Width <= split.SplitterWidth)
            {
                return;
            }

            int espacioDisponible = split.Width - split.SplitterWidth;
            int minimoPanel1 = Math.Min(400, Math.Max(25, espacioDisponible / 2));
            int minimoPanel2 = Math.Min(360, Math.Max(25, espacioDisponible / 2));

            if (minimoPanel1 + minimoPanel2 > espacioDisponible)
            {
                minimoPanel1 = Math.Max(25, espacioDisponible / 2);
                minimoPanel2 = Math.Max(25, espacioDisponible - minimoPanel1);
            }

            if (split.Panel1MinSize != minimoPanel1)
            {
                split.Panel1MinSize = minimoPanel1;
            }

            if (split.Panel2MinSize != minimoPanel2)
            {
                split.Panel2MinSize = minimoPanel2;
            }

            int distanciaMaxima = espacioDisponible - minimoPanel2;
            int distanciaDeseada = Math.Min(650, distanciaMaxima);
            distanciaDeseada = Math.Max(minimoPanel1, distanciaDeseada);

            if (split.SplitterDistance != distanciaDeseada)
            {
                split.SplitterDistance = distanciaDeseada;
            }
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

        private void AgregarCampo_380_jh(TableLayoutPanel tabla, string key, Control control, int fila)
        {
            Label label = CrearLabel_380_jh(key, 8, false);
            label.Dock = DockStyle.None;
            label.AutoSize = true;
            tabla.Controls.Add(label, 0, fila);
            tabla.Controls.Add(control, 1, fila);
        }

        private void AgregarColumna_380_jh(string nombre, string key, float peso)
        {
            _grid_380_jh.Columns.Add(new DataGridViewTextBoxColumn { Name = nombre, Tag = key, FillWeight = peso });
        }

        private void ConfigurarPermisos_380_jh()
        {
            _btnClasificar_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UnidadClasificar_380_jh);
            _btnLiberar_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UnidadLiberar_380_jh);
            _btnBloquear_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UnidadBloquear_380_jh);
            _btnDescartar_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.UnidadDescartar_380_jh);
        }

        private void CargarUnidades_380_jh()
        {
            new IntegridadApplicationService_380_jh().Verificar_380_jh();
            List<Unidad_380_jh> unidades = _unidadService_380_jh.Listar_380_jh(_txtBuscar_380_jh == null ? null : _txtBuscar_380_jh.Text);
            _unidades_380_jh.Clear();
            _unidades_380_jh.AddRange(unidades);
            PintarGrilla_380_jh();
        }

        private void PintarGrilla_380_jh()
        {
            if (_grid_380_jh == null)
            {
                return;
            }

            _grid_380_jh.Rows.Clear();
            foreach (Unidad_380_jh unidad in _unidades_380_jh)
            {
                int fila = _grid_380_jh.Rows.Add(
                    unidad.CodigoIdentificacion_380_jh,
                    unidad.Donante_380_jh,
                    unidad.TipoComponente_380_jh,
                    unidad.GrupoSanguineo_380_jh,
                    unidad.FactorRh_380_jh,
                    unidad.FechaVencimiento_380_jh.ToString("dd/MM/yyyy"),
                    TraducirEstado_380_jh(unidad.EstadoOperativo_380_jh));
                _grid_380_jh.Rows[fila].Tag = unidad;
            }

            _lblEstado_380_jh.Text = _unidades_380_jh.Count + " " + LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("MENU_UNITS");
        }

        private void Grid_SelectionChanged_380_jh(object sender, EventArgs e)
        {
            if (_grid_380_jh.SelectedRows.Count == 0)
            {
                return;
            }

            Unidad_380_jh unidad = _grid_380_jh.SelectedRows[0].Tag as Unidad_380_jh;
            if (unidad == null)
            {
                return;
            }

            _txtComponente_380_jh.Text = unidad.TipoComponente_380_jh ?? string.Empty;
            _cmbGrupo_380_jh.SelectedItem = unidad.GrupoSanguineo_380_jh;
            _cmbRh_380_jh.SelectedItem = unidad.FactorRh_380_jh;
            // Un dato alterado puede estar fuera del rango admitido por WinForms.
            // La consulta sigue visible y bloqueada; no se corrige el dato en la base.
            _dtpVencimiento_380_jh.Value = unidad.FechaVencimiento_380_jh < _dtpVencimiento_380_jh.MinDate
                ? _dtpVencimiento_380_jh.MinDate : unidad.FechaVencimiento_380_jh > _dtpVencimiento_380_jh.MaxDate
                ? _dtpVencimiento_380_jh.MaxDate : unidad.FechaVencimiento_380_jh;
            _txtObservaciones_380_jh.Text = unidad.Observaciones_380_jh ?? string.Empty;
        }

        private Unidad_380_jh ObtenerSeleccionada_380_jh()
        {
            return _grid_380_jh.SelectedRows.Count == 0 ? null : _grid_380_jh.SelectedRows[0].Tag as Unidad_380_jh;
        }

        private void Clasificar_Click_380_jh(object sender, EventArgs e)
        {
            Unidad_380_jh unidad = ObtenerSeleccionada_380_jh();
            if (unidad == null)
            {
                MessageBox.Show(LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("UNIT_INVALID"));
                return;
            }

            unidad.TipoComponente_380_jh = _txtComponente_380_jh.Text;
            unidad.GrupoSanguineo_380_jh = _cmbGrupo_380_jh.SelectedItem == null ? null : _cmbGrupo_380_jh.SelectedItem.ToString();
            unidad.FactorRh_380_jh = _cmbRh_380_jh.SelectedItem == null ? null : _cmbRh_380_jh.SelectedItem.ToString();
            unidad.FechaVencimiento_380_jh = _dtpVencimiento_380_jh.Value.Date;
            unidad.Observaciones_380_jh = _txtObservaciones_380_jh.Text;
            CodigoOperacionPN1_380_jh resultado = _unidadService_380_jh.Clasificar_380_jh(unidad);
            MessageBox.Show(LanguageManager_380_jh.Instance_380_jh.Translate_380_jh(ObtenerClaveResultado_380_jh(resultado, null)));
            if (resultado == CodigoOperacionPN1_380_jh.Ok_380_jh)
            {
                CargarUnidades_380_jh();
            }
        }

        private void CambiarEstado_Click_380_jh(string estado)
        {
            Unidad_380_jh unidad = ObtenerSeleccionada_380_jh();
            if (unidad == null)
            {
                MessageBox.Show(LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("UNIT_INVALID"));
                return;
            }

            CodigoOperacionPN1_380_jh resultado = _unidadService_380_jh.CambiarEstado_380_jh(unidad, estado, _txtObservaciones_380_jh.Text);
            MessageBox.Show(LanguageManager_380_jh.Instance_380_jh.Translate_380_jh(ObtenerClaveResultado_380_jh(resultado, estado)));
            if (resultado == CodigoOperacionPN1_380_jh.Ok_380_jh)
            {
                CargarUnidades_380_jh();
            }
        }

        private void Buscar_KeyDown_380_jh(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CargarUnidades_380_jh();
                e.SuppressKeyPress = true;
            }
        }

        private static string ObtenerClaveResultado_380_jh(CodigoOperacionPN1_380_jh resultado, string estado)
        {
            if (resultado == CodigoOperacionPN1_380_jh.Ok_380_jh)
            {
                switch (estado)
                {
                    case EstadoUnidadPN1_380_jh.Liberada_380_jh: return "UNIT_RELEASED";
                    case EstadoUnidadPN1_380_jh.Bloqueada_380_jh: return "UNIT_BLOCKED";
                    case EstadoUnidadPN1_380_jh.Descartada_380_jh: return "UNIT_DISCARDED";
                    default: return "UNIT_CLASSIFIED";
                }
            }

            switch (resultado)
            {
                case CodigoOperacionPN1_380_jh.IntegridadInvalida_380_jh: return "INTEGRITY_BLOCKED";
                case CodigoOperacionPN1_380_jh.DatosInvalidos_380_jh: return "UNIT_INVALID";
                case CodigoOperacionPN1_380_jh.ConflictoEstado_380_jh: return "UNIT_CONFLICT";
                case CodigoOperacionPN1_380_jh.NoAutorizado_380_jh: return "OPERATION_NOT_AUTHORIZED";
                default: return "UNIT_ERROR";
            }
        }

        private string TraducirEstado_380_jh(string estado)
        {
            string clave;
            switch (estado)
            {
                case EstadoUnidadPN1_380_jh.Liberada_380_jh: clave = "UNIT_STATE_LIBERADA"; break;
                case EstadoUnidadPN1_380_jh.Bloqueada_380_jh: clave = "UNIT_STATE_BLOQUEADA"; break;
                case EstadoUnidadPN1_380_jh.Descartada_380_jh: clave = "UNIT_STATE_DESCARTADA"; break;
                default: clave = "UNIT_STATE_EN_REVISION"; break;
            }

            return LanguageManager_380_jh.Instance_380_jh.Translate_380_jh(clave);
        }
    }
}
