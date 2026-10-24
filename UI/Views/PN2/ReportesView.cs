using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Application;
using Domain;

namespace UI
{
    public class ReportesView_380_jh : LocalizedUserControl_380_jh
    {
        private readonly PN2ApplicationService_380_jh _pn2Service_380_jh;
        private readonly List<FilaReportePN2_380_jh> _filas_380_jh = new List<FilaReportePN2_380_jh>();
        private NumericUpDown _numUnidad_380_jh;
        private DateTimePicker _dtpDesde_380_jh;
        private DateTimePicker _dtpHasta_380_jh;
        private ComboBox _cmbEstado_380_jh;
        private ComboBox _cmbGrupo_380_jh;
        private TextBox _txtComponente_380_jh;
        private DataGridView _grid_380_jh;
        private Label _lblEstado_380_jh;

        public ReportesView_380_jh() : this(new PN2ApplicationService_380_jh()) { }

        public ReportesView_380_jh(PN2ApplicationService_380_jh pn2Service)
        {
            _pn2Service_380_jh = pn2Service;
            ConstruirInterfaz_380_jh();
            CargarReporte_380_jh();
        }

        private void ConstruirInterfaz_380_jh()
        {
            TableLayoutPanel layout = ControlesPN2_380_jh.CrearLayout_380_jh(this, "PN2_REPORTS_TITLE", "PN2_REPORTS_DESCRIPTION", 95, 0);
            FlowLayoutPanel filtros = ControlesPN2_380_jh.CrearFiltros_380_jh();
            _numUnidad_380_jh = new NumericUpDown { Width = 90, Minimum = 0, Maximum = int.MaxValue };
            _dtpDesde_380_jh = new DateTimePicker { Width = 125, Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false };
            _dtpHasta_380_jh = new DateTimePicker { Width = 125, Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false };
            _cmbEstado_380_jh = new ComboBox { Width = 155, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbGrupo_380_jh = new ComboBox { Width = 90, DropDownStyle = ComboBoxStyle.DropDownList };
            _txtComponente_380_jh = new TextBox { Width = 160, MaxLength = 100 };
            ControlesPN2_380_jh.AgregarFiltro_380_jh(filtros, "PN2_UNIT_FILTER", _numUnidad_380_jh);
            ControlesPN2_380_jh.AgregarFiltro_380_jh(filtros, "AUDIT_FILTER_FROM", _dtpDesde_380_jh);
            ControlesPN2_380_jh.AgregarFiltro_380_jh(filtros, "AUDIT_FILTER_TO", _dtpHasta_380_jh);
            ControlesPN2_380_jh.AgregarFiltro_380_jh(filtros, "UNIT_STATUS", _cmbEstado_380_jh);
            ControlesPN2_380_jh.AgregarFiltro_380_jh(filtros, "UNIT_BLOOD_GROUP", _cmbGrupo_380_jh);
            ControlesPN2_380_jh.AgregarFiltro_380_jh(filtros, "UNIT_COMPONENT", _txtComponente_380_jh);
            Button buscar = ControlesPN2_380_jh.CrearBoton_380_jh("PN2_GENERATE_REPORT");
            buscar.Click += delegate { CargarReporte_380_jh(); };
            filtros.Controls.Add(buscar);
            Button limpiar = ControlesPN2_380_jh.CrearBoton_380_jh("BTN_CLEAR_FILTERS");
            limpiar.Click += delegate
            {
                _numUnidad_380_jh.Value = 0; _dtpDesde_380_jh.Checked = false; _dtpHasta_380_jh.Checked = false;
                _cmbEstado_380_jh.SelectedIndex = 0; _cmbGrupo_380_jh.SelectedIndex = 0; _txtComponente_380_jh.Clear();
                CargarReporte_380_jh();
            };
            filtros.Controls.Add(limpiar);
            layout.Controls.Add(filtros, 0, 2);

            Panel grillaPanel = new Panel { Dock = DockStyle.Fill };
            _grid_380_jh = ControlesPN2_380_jh.CrearGrilla_380_jh();
            _grid_380_jh.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            _grid_380_jh.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "seccion", "PN2_REPORT_SECTION", 100);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "concepto", "PN2_REPORT_CONCEPT", 180);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "cantidad", "PN2_REPORT_QUANTITY", 60);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "detalle", "PN2_REPORT_DETAIL", 250);
            grillaPanel.Controls.Add(_grid_380_jh);
            _lblEstado_380_jh = new Label { Dock = DockStyle.Bottom, Height = 24 };
            grillaPanel.Controls.Add(_lblEstado_380_jh);
            layout.Controls.Add(grillaPanel, 0, 3);
            CargarOpciones_380_jh();
        }

        protected override void ApplyTranslations_380_jh()
        {
            base.ApplyTranslations_380_jh();
            CargarOpciones_380_jh();
            PintarReporte_380_jh();
        }

        private void CargarOpciones_380_jh()
        {
            if (_cmbEstado_380_jh == null) return;
            ControlesPN2_380_jh.CargarEstados_380_jh(_cmbEstado_380_jh, "", "EN_REVISION", "LIBERADA", "BLOQUEADA", "RESERVADA", "ENTREGADA", "TRANSFUNDIDA", "DEVUELTA", "DESCARTADA");
            ControlesPN2_380_jh.CargarEstados_380_jh(_cmbGrupo_380_jh, "", "A", "B", "AB", "O");
        }

        private void CargarReporte_380_jh()
        {
            if (_dtpDesde_380_jh.Checked && _dtpHasta_380_jh.Checked && _dtpDesde_380_jh.Value.Date > _dtpHasta_380_jh.Value.Date)
            { MessageBox.Show(this, ControlesPN2_380_jh.Traducir_380_jh("PN2_INVALID_FILTER")); return; }
            try
            {
                new IntegridadApplicationService_380_jh().Verificar_380_jh();
                TrazabilidadFiltro_380_jh filtro = new TrazabilidadFiltro_380_jh
                {
                    IdUnidad_380_jh = _numUnidad_380_jh.Value == 0 ? (int?)null : Convert.ToInt32(_numUnidad_380_jh.Value),
                    Desde_380_jh = _dtpDesde_380_jh.Checked ? _dtpDesde_380_jh.Value.Date : (DateTime?)null,
                    Hasta_380_jh = _dtpHasta_380_jh.Checked ? _dtpHasta_380_jh.Value.Date.AddDays(1).AddMilliseconds(-3) : (DateTime?)null,
                    Estado_380_jh = ControlesPN2_380_jh.EstadoSeleccionado_380_jh(_cmbEstado_380_jh),
                    GrupoSanguineo_380_jh = ControlesPN2_380_jh.EstadoSeleccionado_380_jh(_cmbGrupo_380_jh),
                    TipoComponente_380_jh = _txtComponente_380_jh.Text
                };
                List<FilaReportePN2_380_jh> filas = _pn2Service_380_jh.ListarReporte_380_jh(filtro);
                _filas_380_jh.Clear();
                _filas_380_jh.AddRange(filas);
                _grid_380_jh.Enabled = true;
                PintarReporte_380_jh();
            }
            catch { ControlesPN2_380_jh.MostrarErrorConsulta_380_jh(this, _grid_380_jh); }
        }

        private void PintarReporte_380_jh()
        {
            if (_grid_380_jh == null) return;
            _grid_380_jh.Rows.Clear();
            foreach (FilaReportePN2_380_jh fila in _filas_380_jh)
            {
                int posicion = _grid_380_jh.Rows.Add(TraducirDatoReporte_380_jh(fila.Seccion_380_jh),
                    TraducirDatoReporte_380_jh(fila.Concepto_380_jh), fila.Cantidad_380_jh, TraducirDetalle_380_jh(fila.Detalle_380_jh));
                _grid_380_jh.Rows[posicion].Tag = fila;
            }
            _lblEstado_380_jh.Text = _grid_380_jh.Enabled ? string.Format(ControlesPN2_380_jh.Traducir_380_jh("PN2_RECORD_COUNT"), _filas_380_jh.Count)
                : ControlesPN2_380_jh.Traducir_380_jh("PN2_QUERY_ERROR");
        }

        private static string TraducirDatoReporte_380_jh(string valor)
        {
            if (string.IsNullOrEmpty(valor)) return string.Empty;
            string[] partes = valor.Split(new[] { " / " }, StringSplitOptions.None);
            if (partes.Length == 3)
            {
                if (partes[0] == "(sin clasificar)") partes[0] = ControlesPN2_380_jh.Traducir_380_jh("PN2_UNCLASSIFIED");
                partes[2] = ControlesPN2_380_jh.TraducirEstado_380_jh(partes[2]);
                return string.Join(" / ", partes);
            }
            string clave = "PN2_REPORT_" + valor.ToUpperInvariant().Replace(" ", "_");
            string texto = ControlesPN2_380_jh.Traducir_380_jh(clave);
            return texto == clave ? ControlesPN2_380_jh.TraducirEstado_380_jh(valor) : texto;
        }

        private static string TraducirDetalle_380_jh(string detalle)
        {
            string clave;
            switch (detalle)
            {
                case "Inventario actual; el período no reconstruye stock histórico.": clave = "PN2_REPORT_CURRENT_INVENTORY_NOTE"; break;
                case "Solicitudes registradas en el período seleccionado.": clave = "PN2_REPORT_REQUESTS_NOTE"; break;
                case "Unidades aún no cubiertas por reservas activas o entregas efectivas.": clave = "PN2_REPORT_BALANCE_NOTE"; break;
                case "Reservas vigentes registradas en el período seleccionado.": clave = "PN2_REPORT_RESERVATIONS_NOTE"; break;
                case "Movimientos registrados en el período; filtro de estado según resultado del movimiento.": clave = "PN2_REPORT_MOVEMENTS_NOTE"; break;
                case "Unidades vencidas cuyo recorrido aún no terminó en transfusión o descarte.": clave = "PN2_REPORT_EXPIRED_NOTE"; break;
                case "Reservas activas con unidad vencida, sin clasificación o con estado/componente incompatible.": clave = "PN2_REPORT_INVALID_RESERVATIONS_NOTE"; break;
                case "Solicitudes con saldo y sin unidad liberada, clasificada y vigente del componente solicitado.": clave = "PN2_REPORT_UNAVAILABLE_NOTE"; break;
                default: return detalle ?? string.Empty;
            }
            return ControlesPN2_380_jh.Traducir_380_jh(clave);
        }
    }
}
