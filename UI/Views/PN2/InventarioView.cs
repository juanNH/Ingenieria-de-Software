using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Application;
using Domain;

namespace UI
{
    public class InventarioView_380_jh : LocalizedUserControl_380_jh
    {
        private readonly PN2ApplicationService_380_jh _pn2Service_380_jh;
        private readonly List<Unidad_380_jh> _unidades_380_jh = new List<Unidad_380_jh>();
        private DataGridView _grid_380_jh;
        private TextBox _txtBuscar_380_jh;
        private ComboBox _cmbEstado_380_jh;
        private Label _lblEstado_380_jh;

        public InventarioView_380_jh() : this(new PN2ApplicationService_380_jh()) { }

        public InventarioView_380_jh(PN2ApplicationService_380_jh pn2Service)
        {
            _pn2Service_380_jh = pn2Service;
            ConstruirInterfaz_380_jh();
            CargarInventario_380_jh();
        }

        private void ConstruirInterfaz_380_jh()
        {
            TableLayoutPanel layout = ControlesPN2_380_jh.CrearLayout_380_jh(this, "PN2_INVENTORY_TITLE", "PN2_INVENTORY_DESCRIPTION_VIEW", 52, 0);
            FlowLayoutPanel filtros = ControlesPN2_380_jh.CrearFiltros_380_jh();
            _txtBuscar_380_jh = new TextBox { Width = 210, MaxLength = 150 };
            _txtBuscar_380_jh.KeyDown += delegate(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) { CargarInventario_380_jh(); e.SuppressKeyPress = true; } };
            _cmbEstado_380_jh = new ComboBox { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            ControlesPN2_380_jh.AgregarFiltro_380_jh(filtros, "PN2_SEARCH", _txtBuscar_380_jh);
            ControlesPN2_380_jh.AgregarFiltro_380_jh(filtros, "UNIT_STATUS", _cmbEstado_380_jh);
            Button buscar = ControlesPN2_380_jh.CrearBoton_380_jh("BTN_SEARCH");
            buscar.Click += delegate { CargarInventario_380_jh(); };
            filtros.Controls.Add(buscar);
            Button actualizar = ControlesPN2_380_jh.CrearBoton_380_jh("BTN_REFRESH");
            actualizar.Click += delegate { CargarInventario_380_jh(); };
            filtros.Controls.Add(actualizar);
            layout.Controls.Add(filtros, 0, 2);

            Panel panelGrilla = new Panel { Dock = DockStyle.Fill };
            _grid_380_jh = ControlesPN2_380_jh.CrearGrilla_380_jh();
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "codigo", "UNIT_CODE", 115);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "componente", "UNIT_COMPONENT", 120);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "grupo", "UNIT_BLOOD_GROUP", 55);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "rh", "UNIT_RH", 45);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "estado", "UNIT_STATUS", 100);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "vencimiento", "UNIT_EXPIRATION", 90);
            panelGrilla.Controls.Add(_grid_380_jh);
            _lblEstado_380_jh = new Label { Dock = DockStyle.Bottom, Height = 24 };
            panelGrilla.Controls.Add(_lblEstado_380_jh);
            layout.Controls.Add(panelGrilla, 0, 3);
            CargarEstados_380_jh();
        }

        protected override void ApplyTranslations_380_jh()
        {
            base.ApplyTranslations_380_jh();
            CargarEstados_380_jh();
            PintarInventario_380_jh();
        }

        private void CargarEstados_380_jh()
        {
            if (_cmbEstado_380_jh != null)
                ControlesPN2_380_jh.CargarEstados_380_jh(_cmbEstado_380_jh, "", "EN_REVISION", "LIBERADA", "BLOQUEADA", "RESERVADA", "ENTREGADA", "TRANSFUNDIDA", "DEVUELTA", "DESCARTADA");
        }

        private void CargarInventario_380_jh()
        {
            try
            {
                new IntegridadApplicationService_380_jh().Verificar_380_jh();
                List<Unidad_380_jh> unidades = _pn2Service_380_jh.ListarInventario_380_jh(_txtBuscar_380_jh.Text, ControlesPN2_380_jh.EstadoSeleccionado_380_jh(_cmbEstado_380_jh));
                _unidades_380_jh.Clear();
                _unidades_380_jh.AddRange(unidades);
                _grid_380_jh.Enabled = true;
                PintarInventario_380_jh();
            }
            catch { ControlesPN2_380_jh.MostrarErrorConsulta_380_jh(this, _grid_380_jh); }
        }

        private void PintarInventario_380_jh()
        {
            if (_grid_380_jh == null) return;
            int idSeleccionado = ControlesPN2_380_jh.Seleccionado_380_jh<Unidad_380_jh>(_grid_380_jh)?.Id_380_jh ?? 0;
            _grid_380_jh.Rows.Clear();
            foreach (Unidad_380_jh unidad in _unidades_380_jh)
            {
                int fila = _grid_380_jh.Rows.Add(unidad.CodigoIdentificacion_380_jh, unidad.TipoComponente_380_jh,
                    unidad.GrupoSanguineo_380_jh, unidad.FactorRh_380_jh, ControlesPN2_380_jh.TraducirEstado_380_jh(unidad.EstadoOperativo_380_jh),
                    unidad.FechaVencimiento_380_jh.ToString("dd/MM/yyyy"));
                _grid_380_jh.Rows[fila].Tag = unidad;
                if (unidad.Id_380_jh == idSeleccionado) _grid_380_jh.Rows[fila].Selected = true;
            }
            _lblEstado_380_jh.Text = _grid_380_jh.Enabled ? string.Format(ControlesPN2_380_jh.Traducir_380_jh("PN2_RECORD_COUNT"), _unidades_380_jh.Count)
                : ControlesPN2_380_jh.Traducir_380_jh("PN2_QUERY_ERROR");
        }
    }
}
