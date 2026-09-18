using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Application;
using Domain;
using Services;

namespace UI
{
    public class DonacionesView_380_jh : LocalizedUserControl_380_jh
    {
        private readonly DonanteApplicationService_380_jh _donanteService_380_jh;
        private readonly DonacionApplicationService_380_jh _donacionService_380_jh;
        private readonly AutorizacionApplicationService_380_jh _autorizacionService_380_jh;
        private readonly List<Unidad_380_jh> _unidades_380_jh = new List<Unidad_380_jh>();
        private ComboBox _cmbDonante_380_jh;
        private DateTimePicker _dtpDonacion_380_jh;
        private NumericUpDown _numCantidad_380_jh;
        private TextBox _txtComponente_380_jh;
        private DateTimePicker _dtpVencimiento_380_jh;
        private TextBox _txtObservaciones_380_jh;
        private DataGridView _gridUnidades_380_jh;
        private Button _btnRegistrar_380_jh;
        private Label _lblEstado_380_jh;

        public DonacionesView_380_jh()
            : this(new DonanteApplicationService_380_jh(), new DonacionApplicationService_380_jh(), new AutorizacionApplicationService_380_jh())
        {
        }

        public DonacionesView_380_jh(
            DonanteApplicationService_380_jh donanteService,
            DonacionApplicationService_380_jh donacionService,
            AutorizacionApplicationService_380_jh autorizacionService)
        {
            _donanteService_380_jh = donanteService;
            _donacionService_380_jh = donacionService;
            _autorizacionService_380_jh = autorizacionService;
            ConstruirInterfaz_380_jh();
            new ProteccionIntegridad_380_jh(this, _btnRegistrar_380_jh);
            CargarDonantes_380_jh();
        }

        protected override void ApplyTranslations_380_jh()
        {
            base.ApplyTranslations_380_jh();
            PintarUnidades_380_jh();
        }

        private void ConstruirInterfaz_380_jh()
        {
            BackColor = Color.White;
            Padding = new Padding(24);

            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, AutoScroll = true };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 195));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);
            layout.Controls.Add(CrearLabel_380_jh("DONATION_TITLE", 18, true), 0, 0);
            layout.Controls.Add(CrearLabel_380_jh("DONATION_DESCRIPTION", 9, false), 0, 1);

            GroupBox detalle = new GroupBox { Dock = DockStyle.Fill, Tag = "DONATION_DETAIL", Padding = new Padding(10) };
            layout.Controls.Add(detalle, 0, 2);

            TableLayoutPanel formulario = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 4 };
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            detalle.Controls.Add(formulario);

            _cmbDonante_380_jh = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _dtpDonacion_380_jh = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, Value = DateTime.Now };
            _numCantidad_380_jh = new NumericUpDown { Dock = DockStyle.Left, Minimum = 1, Maximum = 1000, Value = 1, Width = 120 };
            _txtComponente_380_jh = new TextBox { Dock = DockStyle.Fill, MaxLength = 100 };
            _dtpVencimiento_380_jh = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddDays(35) };
            _txtObservaciones_380_jh = new TextBox { Dock = DockStyle.Fill, MaxLength = 500 };

            AgregarCampo_380_jh(formulario, "DONATION_DONOR", _cmbDonante_380_jh, 0, 0);
            AgregarCampo_380_jh(formulario, "DONATION_DATE", _dtpDonacion_380_jh, 2, 0);
            AgregarCampo_380_jh(formulario, "DONATION_QUANTITY", _numCantidad_380_jh, 0, 1);
            AgregarCampo_380_jh(formulario, "DONATION_COMPONENT", _txtComponente_380_jh, 2, 1);
            AgregarCampo_380_jh(formulario, "DONATION_EXPIRATION", _dtpVencimiento_380_jh, 0, 2);
            AgregarCampo_380_jh(formulario, "DONATION_OBSERVATIONS", _txtObservaciones_380_jh, 2, 2);
            formulario.SetColumnSpan(_txtObservaciones_380_jh, 1);

            FlowLayoutPanel acciones = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            _btnRegistrar_380_jh = CrearBoton_380_jh("BTN_REGISTER");
            _btnRegistrar_380_jh.Click += Registrar_Click_380_jh;
            Button actualizar = CrearBoton_380_jh("BTN_REFRESH");
            actualizar.Click += delegate { CargarDonantes_380_jh(); };
            acciones.Controls.Add(_btnRegistrar_380_jh);
            acciones.Controls.Add(actualizar);
            formulario.Controls.Add(acciones, 2, 3);

            Panel unidadesPanel = new Panel { Dock = DockStyle.Fill };
            layout.Controls.Add(unidadesPanel, 0, 3);
            _gridUnidades_380_jh = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            AgregarColumna_380_jh("codigo_identificacion", "UNIT_CODE");
            AgregarColumna_380_jh("tipo_componente", "UNIT_COMPONENT");
            AgregarColumna_380_jh("fecha_vencimiento", "UNIT_EXPIRATION");
            AgregarColumna_380_jh("estado_operativo", "UNIT_STATUS");
            unidadesPanel.Controls.Add(_gridUnidades_380_jh);
            _lblEstado_380_jh = new Label { Dock = DockStyle.Bottom, Height = 24, TextAlign = ContentAlignment.MiddleLeft };
            unidadesPanel.Controls.Add(_lblEstado_380_jh);

            _btnRegistrar_380_jh.Enabled = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.DonacionCrear_380_jh);
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
            _gridUnidades_380_jh.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = propiedad,
                DataPropertyName = propiedad,
                Tag = key
            });
        }

        private void CargarDonantes_380_jh()
        {
            new IntegridadApplicationService_380_jh().Verificar_380_jh();
            if (!_autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.DonacionCrear_380_jh))
            {
                return;
            }

            List<Donante_380_jh> donantes = _donanteService_380_jh.Listar_380_jh();
            _cmbDonante_380_jh.DataSource = null;
            _cmbDonante_380_jh.DisplayMember = "NombreCompleto_380_jh";
            _cmbDonante_380_jh.ValueMember = "Id_380_jh";
            _cmbDonante_380_jh.DataSource = donantes;
        }

        private void Registrar_Click_380_jh(object sender, EventArgs e)
        {
            Donante_380_jh donante = _cmbDonante_380_jh.SelectedItem as Donante_380_jh;
            Donacion_380_jh donacion = new Donacion_380_jh
            {
                IdDonante_380_jh = donante == null ? 0 : donante.Id_380_jh,
                FechaDonacion_380_jh = _dtpDonacion_380_jh.Value,
                CantidadUnidades_380_jh = Convert.ToInt32(_numCantidad_380_jh.Value),
                TipoComponente_380_jh = _txtComponente_380_jh.Text,
                FechaVencimiento_380_jh = _dtpVencimiento_380_jh.Value.Date,
                Observaciones_380_jh = _txtObservaciones_380_jh.Text
            };

            CodigoOperacionPN1_380_jh resultado = _donacionService_380_jh.Registrar_380_jh(donacion);
            MessageBox.Show(LanguageManager_380_jh.Instance_380_jh.Translate_380_jh(ObtenerClaveResultado_380_jh(resultado)));
            if (resultado == CodigoOperacionPN1_380_jh.Ok_380_jh)
            {
                CargarUnidades_380_jh(donacion.Unidades_380_jh);
                _numCantidad_380_jh.Value = 1;
                _txtObservaciones_380_jh.Clear();
            }
        }

        private void CargarUnidades_380_jh(List<Unidad_380_jh> unidades)
        {
            _unidades_380_jh.Clear();
            _unidades_380_jh.AddRange(unidades ?? new List<Unidad_380_jh>());
            PintarUnidades_380_jh();
        }

        private void PintarUnidades_380_jh()
        {
            if (_gridUnidades_380_jh == null)
            {
                return;
            }

            _gridUnidades_380_jh.Rows.Clear();
            foreach (Unidad_380_jh unidad in _unidades_380_jh)
            {
                _gridUnidades_380_jh.Rows.Add(
                    unidad.CodigoIdentificacion_380_jh,
                    unidad.TipoComponente_380_jh,
                    unidad.FechaVencimiento_380_jh.ToString("dd/MM/yyyy"),
                    TraducirEstado_380_jh(unidad.EstadoOperativo_380_jh));
            }

            _lblEstado_380_jh.Text = _unidades_380_jh.Count + " " + LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("MENU_UNITS");
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

        private static string ObtenerClaveResultado_380_jh(CodigoOperacionPN1_380_jh resultado)
        {
            switch (resultado)
            {
                case CodigoOperacionPN1_380_jh.Ok_380_jh: return "DONATION_REGISTERED";
                case CodigoOperacionPN1_380_jh.IntegridadInvalida_380_jh: return "INTEGRITY_BLOCKED";
                case CodigoOperacionPN1_380_jh.DatosInvalidos_380_jh: return "DONATION_INVALID";
                case CodigoOperacionPN1_380_jh.NoAutorizado_380_jh: return "OPERATION_NOT_AUTHORIZED";
                default: return "DONATION_ERROR";
            }
        }
    }
}
