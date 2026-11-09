using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        private readonly bool _puedeRegistrar_380_jh;
        private readonly bool _puedeConsultar_380_jh;
        private readonly List<Unidad_380_jh> _unidades_380_jh = new List<Unidad_380_jh>();
        private ComboBox _cmbDonante_380_jh;
        private DateTimePicker _dtpDonacion_380_jh;
        private NumericUpDown _numCantidad_380_jh;
        private ComboBox _cmbComponente_380_jh;
        private DateTimePicker _dtpVencimiento_380_jh;
        private TextBox _txtObservaciones_380_jh;
        private DataGridView _gridUnidades_380_jh;
        private Button _btnRegistrar_380_jh;
        private ComboBox _cmbDonacionRegistrada_380_jh;
        private Button _btnCantidadHistorial_380_jh;
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
            _puedeRegistrar_380_jh = autorizacionService.TienePermiso_380_jh(PermisosSistema_380_jh.DonacionCrear_380_jh);
            _puedeConsultar_380_jh = autorizacionService.TienePermiso_380_jh(PermisosSistema_380_jh.UnidadVer_380_jh);
            bool puedeConsultarDonantes = autorizacionService.TienePermiso_380_jh(PermisosSistema_380_jh.DonanteVer_380_jh) ||
                autorizacionService.TienePermiso_380_jh(PermisosSistema_380_jh.DonanteCrear_380_jh);
            ConstruirInterfaz_380_jh();
            new ProteccionIntegridad_380_jh(this, _btnRegistrar_380_jh);
            new IntegridadApplicationService_380_jh().Verificar_380_jh();
            if (_puedeRegistrar_380_jh && puedeConsultarDonantes)
                CargarDonantes_380_jh();
            CargarDonaciones_380_jh();
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

            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, AutoScroll = true };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, _puedeRegistrar_380_jh ? 195 : 0));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, _puedeConsultar_380_jh ? 44 : 0));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);
            layout.Controls.Add(CrearLabel_380_jh("DONATION_TITLE", 18, true), 0, 0);
            layout.Controls.Add(CrearLabel_380_jh("DONATION_DESCRIPTION", 9, false), 0, 1);

            GroupBox detalle = new GroupBox { Dock = DockStyle.Fill, Tag = "DONATION_NEW", Padding = new Padding(10), Visible = _puedeRegistrar_380_jh };
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
            _cmbComponente_380_jh = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            MaestrosCombo_380_jh.Cargar_380_jh(_cmbComponente_380_jh, MaestroPN1ApplicationService_380_jh.Componente_380_jh);
            _dtpVencimiento_380_jh = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddDays(35) };
            _txtObservaciones_380_jh = new TextBox { Dock = DockStyle.Fill, MaxLength = 500 };

            AgregarCampo_380_jh(formulario, "DONATION_DONOR", _cmbDonante_380_jh, 0, 0);
            AgregarCampo_380_jh(formulario, "DONATION_DATE", _dtpDonacion_380_jh, 2, 0);
            AgregarCampo_380_jh(formulario, "DONATION_QUANTITY", _numCantidad_380_jh, 0, 1);
            AgregarCampo_380_jh(formulario, "DONATION_COMPONENT", _cmbComponente_380_jh, 2, 1);
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

            var consulta = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, Visible = _puedeConsultar_380_jh };
            consulta.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            consulta.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            consulta.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            consulta.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            var etiqueta = new Label { AutoSize = true, Anchor = AnchorStyles.Left, Tag = "DONATION_REGISTERED_SELECTION" };
            _cmbDonacionRegistrada_380_jh = new ComboBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, DropDownStyle = ComboBoxStyle.DropDownList, FormattingEnabled = true };
            _cmbDonacionRegistrada_380_jh.Format += (sender, args) =>
            {
                if (args.ListItem is Donacion_380_jh donacion)
                {
                    string cantidad = string.Format(LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("DONATION_QUANTITY_CURRENT"), donacion.CantidadUnidades_380_jh);
                    args.Value = $"#{donacion.Id_380_jh} · {donacion.Donante_380_jh.NombreCompleto_380_jh} · {donacion.FechaDonacion_380_jh:d} · {cantidad}";
                }
            };
            _cmbDonacionRegistrada_380_jh.SelectedIndexChanged += delegate { CargarUnidadesSeleccionadas_380_jh(); };
            _btnCantidadHistorial_380_jh = CrearBoton_380_jh("DONATION_QUANTITY_HISTORY");
            _btnCantidadHistorial_380_jh.Enabled = false;
            _btnCantidadHistorial_380_jh.Click += CantidadDonacion_Click_380_jh;
            Button actualizarDonaciones = CrearBoton_380_jh("BTN_REFRESH");
            actualizarDonaciones.Click += delegate { CargarDonaciones_380_jh(); };
            consulta.Controls.Add(etiqueta, 0, 0);
            consulta.Controls.Add(_cmbDonacionRegistrada_380_jh, 1, 0);
            consulta.Controls.Add(_btnCantidadHistorial_380_jh, 2, 0);
            consulta.Controls.Add(actualizarDonaciones, 3, 0);
            layout.Controls.Add(consulta, 0, 3);

            Panel unidadesPanel = new Panel { Dock = DockStyle.Fill };
            layout.Controls.Add(unidadesPanel, 0, 4);
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

            _btnRegistrar_380_jh.Visible = _puedeRegistrar_380_jh;
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
                TipoComponente_380_jh = MaestrosCombo_380_jh.Seleccionado_380_jh(_cmbComponente_380_jh),
                FechaVencimiento_380_jh = _dtpVencimiento_380_jh.Value.Date,
                Observaciones_380_jh = _txtObservaciones_380_jh.Text
            };

            CodigoOperacionPN1_380_jh resultado = _donacionService_380_jh.Registrar_380_jh(donacion);
            MessageBox.Show(LanguageManager_380_jh.Instance_380_jh.Translate_380_jh(ObtenerClaveResultado_380_jh(resultado)));
            if (resultado == CodigoOperacionPN1_380_jh.Ok_380_jh)
            {
                MostrarDonacionRegistrada_380_jh(donacion);
                _numCantidad_380_jh.Value = 1;
                _txtObservaciones_380_jh.Clear();
            }
        }

        private void MostrarDonacionRegistrada_380_jh(Donacion_380_jh donacion)
        {
            CargarUnidades_380_jh(donacion.Unidades_380_jh);
            CargarDonaciones_380_jh(donacion.Id_380_jh);
        }

        private void CargarDonaciones_380_jh(int? idPreferido = null)
        {
            if (!_puedeConsultar_380_jh) return;
            idPreferido = idPreferido ?? (_cmbDonacionRegistrada_380_jh.SelectedItem as Donacion_380_jh)?.Id_380_jh;
            List<Donacion_380_jh> donaciones;
            try { donaciones = _donacionService_380_jh.Listar_380_jh(); }
            catch (Exception ex) when (ex is SqlException || ex is InvalidOperationException)
            {
                _lblEstado_380_jh.Text = LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("DONATION_QUANTITY_ERROR");
                return;
            }
            _cmbDonacionRegistrada_380_jh.Items.Clear();
            _cmbDonacionRegistrada_380_jh.Items.AddRange(donaciones.ToArray());
            if (donaciones.Count > 0)
            {
                int indice = donaciones.FindIndex(d => d.Id_380_jh == idPreferido);
                _cmbDonacionRegistrada_380_jh.SelectedIndex = Math.Max(0, indice);
            }
        }

        private void CargarUnidadesSeleccionadas_380_jh()
        {
            var donacion = _cmbDonacionRegistrada_380_jh.SelectedItem as Donacion_380_jh;
            _btnCantidadHistorial_380_jh.Enabled = donacion != null;
            if (donacion == null)
            {
                CargarUnidades_380_jh(null);
                return;
            }
            var datos = ConsultarCantidad_380_jh(donacion.Id_380_jh);
            CargarUnidades_380_jh(datos?.Unidades_380_jh);
            if (datos == null)
                _lblEstado_380_jh.Text = LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("DONATION_QUANTITY_ERROR");
        }

        private CantidadDonacion_380_jh ConsultarCantidad_380_jh(int idDonacion)
        {
            try { return _donacionService_380_jh.ConsultarCantidad_380_jh(idDonacion); }
            catch (Exception ex) when (ex is SqlException || ex is InvalidOperationException) { return null; }
        }

        private void CantidadDonacion_Click_380_jh(object sender, EventArgs e)
        {
            var donacion = _cmbDonacionRegistrada_380_jh.SelectedItem as Donacion_380_jh;
            if (donacion == null) return;
            var datos = ConsultarCantidad_380_jh(donacion.Id_380_jh);
            if (datos == null)
            {
                MessageBox.Show(this, LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("DONATION_QUANTITY_ERROR"));
                return;
            }
            using (var dialogo = new CantidadDonacionDialog_380_jh(_donacionService_380_jh, donacion.Id_380_jh, datos))
            {
                var resultado = dialogo.ShowDialog(this);
                if (resultado == DialogResult.OK || resultado == DialogResult.Retry) CargarDonaciones_380_jh(donacion.Id_380_jh);
                if (resultado == DialogResult.OK) ComprobantePdfUI_380_jh.ProcesarPendientes_380_jh(this, true, false);
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
                case EstadoUnidadPN1_380_jh.EnRevision_380_jh: clave = "UNIT_STATE_EN_REVISION"; break;
                default: clave = "PN2_STATE_" + estado; break;
            }

            string traducido = LanguageManager_380_jh.Instance_380_jh.Translate_380_jh(clave);
            return traducido == clave ? estado : traducido;
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
