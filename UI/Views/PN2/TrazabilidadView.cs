using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Application;
using Domain;

namespace UI
{
    public class TrazabilidadView_380_jh : LocalizedUserControl_380_jh
    {
        private readonly PN2ApplicationService_380_jh _pn2Service_380_jh;
        private readonly List<MovimientoTrazabilidad_380_jh> _movimientos_380_jh = new List<MovimientoTrazabilidad_380_jh>();
        private NumericUpDown _numUnidad_380_jh;
        private DateTimePicker _dtpDesde_380_jh;
        private DateTimePicker _dtpHasta_380_jh;
        private ComboBox _cmbEstado_380_jh;
        private ComboBox _cmbGrupo_380_jh;
        private TextBox _txtComponente_380_jh;
        private TextBox _txtObservacion_380_jh;
        private DataGridView _grid_380_jh;
        private Label _lblEstado_380_jh;

        public TrazabilidadView_380_jh() : this(new PN2ApplicationService_380_jh()) { }

        public TrazabilidadView_380_jh(PN2ApplicationService_380_jh pn2Service)
        {
            _pn2Service_380_jh = pn2Service;
            ConstruirInterfaz_380_jh();
            CargarTrazabilidad_380_jh();
        }

        private void ConstruirInterfaz_380_jh()
        {
            TableLayoutPanel layout = ControlesPN2_380_jh.CrearLayout_380_jh(this, "PN2_TRACE_TITLE", "PN2_TRACE_DESCRIPTION", 95, 88);
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
            Button buscar = ControlesPN2_380_jh.CrearBoton_380_jh("BTN_SEARCH");
            buscar.Click += delegate { CargarTrazabilidad_380_jh(); };
            filtros.Controls.Add(buscar);
            Button limpiar = ControlesPN2_380_jh.CrearBoton_380_jh("BTN_CLEAR_FILTERS");
            limpiar.Click += delegate
            {
                _numUnidad_380_jh.Value = 0; _dtpDesde_380_jh.Checked = false; _dtpHasta_380_jh.Checked = false;
                _cmbEstado_380_jh.SelectedIndex = 0; _cmbGrupo_380_jh.SelectedIndex = 0; _txtComponente_380_jh.Clear();
                CargarTrazabilidad_380_jh();
            };
            filtros.Controls.Add(limpiar);
            layout.Controls.Add(filtros, 0, 2);

            Panel grillaPanel = new Panel { Dock = DockStyle.Fill };
            _grid_380_jh = ControlesPN2_380_jh.CrearGrilla_380_jh();
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "fecha", "PN2_MOVEMENT_DATE", 130);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "unidadId", "PN2_UNIT_ID", 65);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "codigo", "UNIT_CODE", 110);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "movimiento", "PN2_MOVEMENT", 110);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "anterior", "PN2_PREVIOUS_STATE", 95);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "nuevo", "PN2_NEW_STATE", 95);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "usuario", "PN2_RESPONSIBLE", 110);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "solicitud", "PN2_REQUEST_ID", 65);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "reserva", "PN2_RESERVATION_ID", 65);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "entrega", "PN2_DELIVERY_ID", 65);
            _grid_380_jh.SelectionChanged += delegate { MostrarObservacion_380_jh(); };
            grillaPanel.Controls.Add(_grid_380_jh);
            _lblEstado_380_jh = new Label { Dock = DockStyle.Bottom, Height = 24 };
            grillaPanel.Controls.Add(_lblEstado_380_jh);
            layout.Controls.Add(grillaPanel, 0, 3);

            GroupBox detalle = new GroupBox { Dock = DockStyle.Fill, Tag = "UNIT_OBSERVATIONS", Padding = new Padding(8) };
            _txtObservacion_380_jh = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Vertical };
            detalle.Controls.Add(_txtObservacion_380_jh);
            layout.Controls.Add(detalle, 0, 4);
            CargarOpciones_380_jh();
        }

        protected override void ApplyTranslations_380_jh()
        {
            base.ApplyTranslations_380_jh();
            CargarOpciones_380_jh();
            PintarTrazabilidad_380_jh();
        }

        private void CargarOpciones_380_jh()
        {
            if (_cmbEstado_380_jh == null) return;
            ControlesPN2_380_jh.CargarEstados_380_jh(_cmbEstado_380_jh, "", "EN_REVISION", "LIBERADA", "BLOQUEADA", "RESERVADA", "ENTREGADA", "TRANSFUNDIDA", "DEVUELTA", "DESCARTADA");
            ControlesPN2_380_jh.CargarEstados_380_jh(_cmbGrupo_380_jh, "", "A", "B", "AB", "O");
        }

        private void CargarTrazabilidad_380_jh()
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
                List<MovimientoTrazabilidad_380_jh> movimientos = _pn2Service_380_jh.ListarTrazabilidad_380_jh(filtro);
                _movimientos_380_jh.Clear();
                _movimientos_380_jh.AddRange(movimientos);
                _grid_380_jh.Enabled = true;
                PintarTrazabilidad_380_jh();
            }
            catch { ControlesPN2_380_jh.MostrarErrorConsulta_380_jh(this, _grid_380_jh); }
        }

        private void PintarTrazabilidad_380_jh()
        {
            if (_grid_380_jh == null) return;
            _grid_380_jh.Rows.Clear();
            foreach (MovimientoTrazabilidad_380_jh movimiento in _movimientos_380_jh)
            {
                int fila = _grid_380_jh.Rows.Add(movimiento.FechaMovimiento_380_jh.ToString("dd/MM/yyyy HH:mm"), movimiento.IdUnidad_380_jh,
                    movimiento.CodigoIdentificacion_380_jh, ControlesPN2_380_jh.TraducirEstado_380_jh(movimiento.TipoMovimiento_380_jh),
                    ControlesPN2_380_jh.TraducirEstado_380_jh(movimiento.EstadoAnterior_380_jh), ControlesPN2_380_jh.TraducirEstado_380_jh(movimiento.EstadoNuevo_380_jh),
                    movimiento.Usuario_380_jh, movimiento.IdSolicitudTransfusional_380_jh, movimiento.IdReserva_380_jh, movimiento.IdEntrega_380_jh);
                _grid_380_jh.Rows[fila].Tag = movimiento;
            }
            _lblEstado_380_jh.Text = _grid_380_jh.Enabled ? string.Format(ControlesPN2_380_jh.Traducir_380_jh("PN2_RECORD_COUNT"), _movimientos_380_jh.Count)
                : ControlesPN2_380_jh.Traducir_380_jh("PN2_QUERY_ERROR");
            MostrarObservacion_380_jh();
        }

        private void MostrarObservacion_380_jh()
        {
            if (_txtObservacion_380_jh == null) return;
            MovimientoTrazabilidad_380_jh movimiento = ControlesPN2_380_jh.Seleccionado_380_jh<MovimientoTrazabilidad_380_jh>(_grid_380_jh);
            _txtObservacion_380_jh.Text = movimiento == null ? string.Empty : movimiento.Observacion_380_jh;
        }
    }
}
