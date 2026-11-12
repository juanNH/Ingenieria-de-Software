using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Application;
using Domain;

namespace UI
{
    public class DestinosView_380_jh : LocalizedUserControl_380_jh
    {
        private readonly PN2ApplicationService_380_jh _pn2Service_380_jh;
        private readonly AutorizacionApplicationService_380_jh _autorizacionService_380_jh;
        private readonly bool _puedeConsultarEntregas_380_jh;
        private readonly List<Entrega_380_jh> _entregas_380_jh = new List<Entrega_380_jh>();
        private readonly List<MaestroPN1_380_jh> _destinos_380_jh = new List<MaestroPN1_380_jh>();
        private TextBox _txtBuscar_380_jh;
        private ComboBox _cmbFiltroDestino_380_jh;
        private ComboBox _cmbDestino_380_jh;
        private TextBox _txtMotivo_380_jh;
        private DataGridView _grid_380_jh;
        private Button _btnRegistrar_380_jh;
        private Label _lblEstado_380_jh;
        private Label _lblEfecto_380_jh;

        public DestinosView_380_jh() : this(new PN2ApplicationService_380_jh(), new AutorizacionApplicationService_380_jh()) { }

        public DestinosView_380_jh(PN2ApplicationService_380_jh pn2Service, AutorizacionApplicationService_380_jh autorizacionService)
        {
            _pn2Service_380_jh = pn2Service;
            _autorizacionService_380_jh = autorizacionService;
            _puedeConsultarEntregas_380_jh = autorizacionService.TienePermiso_380_jh(PermisosSistema_380_jh.ReservaVer_380_jh);
            ConstruirInterfaz_380_jh();
            new ProteccionIntegridad_380_jh(this, _btnRegistrar_380_jh);
            CargarEntregas_380_jh();
        }

        private void ConstruirInterfaz_380_jh()
        {
            TableLayoutPanel layout = ControlesPN2_380_jh.CrearLayout_380_jh(this, "PN2_DESTINATIONS_TITLE", "PN2_DESTINATIONS_DESCRIPTION", 56, 150);
            FlowLayoutPanel filtros = ControlesPN2_380_jh.CrearFiltros_380_jh();
            _txtBuscar_380_jh = new TextBox { Width = 210, MaxLength = 150 };
            _cmbFiltroDestino_380_jh = new ComboBox { Width = 165, DropDownStyle = ComboBoxStyle.DropDownList };
            ControlesPN2_380_jh.AgregarFiltro_380_jh(filtros, "PN2_SEARCH", _txtBuscar_380_jh);
            ControlesPN2_380_jh.AgregarFiltro_380_jh(filtros, "PN2_DESTINATION", _cmbFiltroDestino_380_jh);
            Button buscar = ControlesPN2_380_jh.CrearBoton_380_jh("BTN_SEARCH");
            buscar.Click += delegate { PintarEntregas_380_jh(); };
            filtros.Controls.Add(buscar);
            _txtBuscar_380_jh.KeyDown += delegate(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) { PintarEntregas_380_jh(); e.SuppressKeyPress = true; } };
            Button actualizar = ControlesPN2_380_jh.CrearBoton_380_jh("BTN_REFRESH");
            actualizar.Click += delegate { CargarEntregas_380_jh(); };
            filtros.Controls.Add(actualizar);
            Button comprobante = ControlesPN2_380_jh.CrearBoton_380_jh("RECEIPT_COPY");
            bool puedeEmitirComprobante = _puedeConsultarEntregas_380_jh &&
                _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.SolicitudVer_380_jh);
            comprobante.Visible = puedeEmitirComprobante;
            comprobante.Click += delegate
            {
                Entrega_380_jh entrega = ControlesPN2_380_jh.Seleccionado_380_jh<Entrega_380_jh>(_grid_380_jh);
                if (entrega == null) { MessageBox.Show(this, ControlesPN2_380_jh.Traducir_380_jh("PN2_SELECT_DELIVERY")); return; }
                ComprobantePdfUI_380_jh.CopiarOriginal_380_jh(this, "PN2", entrega.Id_380_jh);
            };
            filtros.Controls.Add(comprobante);
            Button reporteActual = ControlesPN2_380_jh.CrearBoton_380_jh("RECEIPT_CURRENT_PDF");
            reporteActual.Visible = puedeEmitirComprobante;
            reporteActual.Click += delegate
            {
                Entrega_380_jh entrega = ControlesPN2_380_jh.Seleccionado_380_jh<Entrega_380_jh>(_grid_380_jh);
                if (entrega == null) { MessageBox.Show(this, ControlesPN2_380_jh.Traducir_380_jh("PN2_SELECT_DELIVERY")); return; }
                ComprobantePdfUI_380_jh.GuardarPN2_380_jh(this, _pn2Service_380_jh, entrega.IdSolicitudTransfusional_380_jh, entrega.Id_380_jh);
            };
            filtros.Controls.Add(reporteActual);
            Button reintentarComprobante = ControlesPN2_380_jh.CrearBoton_380_jh("RECEIPT_RETRY");
            reintentarComprobante.Click += delegate { ComprobantePdfUI_380_jh.ProcesarPendientes_380_jh(this, true); };
            filtros.Controls.Add(reintentarComprobante);
            layout.Controls.Add(filtros, 0, 2);

            Panel grillaPanel = new Panel { Dock = DockStyle.Fill };
            _grid_380_jh = ControlesPN2_380_jh.CrearGrilla_380_jh();
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "id", "PN2_DELIVERY_ID", 60);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "solicitud", "PN2_REQUEST_ID", 60);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "codigo", "UNIT_CODE", 110);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "paciente", "PN2_PATIENT", 140);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "fecha", "PN2_DELIVERY_DATE", 115);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "responsable", "PN2_RESPONSIBLE", 110);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "destino", "PN2_DESTINATION", 95);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "fechaDestino", "PN2_DESTINATION_DATE", 115);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "motivo", "PN2_REASON", 160);
            grillaPanel.Controls.Add(_grid_380_jh);
            _lblEstado_380_jh = new Label { Dock = DockStyle.Bottom, Height = 24 };
            grillaPanel.Controls.Add(_lblEstado_380_jh);
            layout.Controls.Add(grillaPanel, 0, 3);

            Panel formularioPanel = new Panel { Dock = DockStyle.Fill };
            TableLayoutPanel formulario = ControlesPN2_380_jh.CrearFormulario_380_jh(formularioPanel, "PN2_REGISTER_DESTINATION", 3);
            _cmbDestino_380_jh = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            _txtMotivo_380_jh = new TextBox { MaxLength = 500 };
            ControlesPN2_380_jh.AgregarCampo_380_jh(formulario, "PN2_DESTINATION", _cmbDestino_380_jh, 0, 0);
            ControlesPN2_380_jh.AgregarCampo_380_jh(formulario, "PN2_REASON_OBSERVATION", _txtMotivo_380_jh, 2, 0);
            _btnRegistrar_380_jh = ControlesPN2_380_jh.CrearBoton_380_jh("PN2_REGISTER_DESTINATION");
            _btnRegistrar_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.DestinoRegistrar_380_jh);
            _btnRegistrar_380_jh.Click += Registrar_Click_380_jh;
            _lblEfecto_380_jh = new Label { Dock = DockStyle.Fill, AutoEllipsis = true };
            formulario.Controls.Add(_lblEfecto_380_jh, 0, 1);
            formulario.SetColumnSpan(_lblEfecto_380_jh, 3);
            _cmbDestino_380_jh.SelectedIndexChanged += delegate { ActualizarEfecto_380_jh(); };
            formulario.Controls.Add(_btnRegistrar_380_jh, 3, 1);
            Label nota = ControlesPN2_380_jh.CrearLabel_380_jh("PN2_DESTINATION_NOTE");
            formulario.Controls.Add(nota, 0, 2);
            formulario.SetColumnSpan(nota, 4);
            layout.Controls.Add(formularioPanel, 0, 4);
        }

        protected override void ApplyTranslations_380_jh()
        {
            base.ApplyTranslations_380_jh();
            CargarOpciones_380_jh();
            PintarEntregas_380_jh();
        }

        private void CargarOpciones_380_jh()
        {
            if (_cmbDestino_380_jh == null) return;
            string filtroAnterior = ControlesPN2_380_jh.EstadoSeleccionado_380_jh(_cmbFiltroDestino_380_jh);
            string destinoAnterior = ControlesPN2_380_jh.EstadoSeleccionado_380_jh(_cmbDestino_380_jh);
            try
            {
                _destinos_380_jh.Clear();
                _destinos_380_jh.AddRange(new MaestroPN1ApplicationService_380_jh().Listar_380_jh(MaestroPN1ApplicationService_380_jh.Destino_380_jh, false));
                _cmbDestino_380_jh.Enabled = true; _cmbFiltroDestino_380_jh.Enabled = true;
                var filtros = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("", ControlesPN2_380_jh.Traducir_380_jh("FILTER_ALL")),
                    new KeyValuePair<string, string>("SIN_DESTINO", ControlesPN2_380_jh.TraducirEstado_380_jh("SIN_DESTINO"))
                };
                filtros.AddRange(_destinos_380_jh.Select(x => new KeyValuePair<string, string>(x.Codigo_380_jh, NombreDestino_380_jh(x))));
                _cmbFiltroDestino_380_jh.DisplayMember = "Value"; _cmbFiltroDestino_380_jh.ValueMember = "Key";
                _cmbFiltroDestino_380_jh.DataSource = filtros;
                _cmbFiltroDestino_380_jh.SelectedValue = filtroAnterior;
                if (_cmbFiltroDestino_380_jh.SelectedIndex < 0) _cmbFiltroDestino_380_jh.SelectedIndex = 0;
                var disponibles = _destinos_380_jh.Where(x => x.Activo_380_jh).Select(x => new KeyValuePair<string, string>(x.Codigo_380_jh, NombreDestino_380_jh(x))).ToList();
                _cmbDestino_380_jh.DisplayMember = "Value"; _cmbDestino_380_jh.ValueMember = "Key";
                _cmbDestino_380_jh.DataSource = disponibles;
                _cmbDestino_380_jh.SelectedValue = destinoAnterior;
                if (string.IsNullOrEmpty(destinoAnterior)) _cmbDestino_380_jh.SelectedIndex = -1;
                ActualizarEfecto_380_jh();
            }
            catch
            {
                _destinos_380_jh.Clear(); _cmbDestino_380_jh.DataSource = null; _cmbFiltroDestino_380_jh.DataSource = null;
                _cmbDestino_380_jh.Enabled = false; _cmbFiltroDestino_380_jh.Enabled = false;
                if (_lblEfecto_380_jh != null) _lblEfecto_380_jh.Text = ControlesPN2_380_jh.Traducir_380_jh("MASTERS_LOAD_ERROR");
            }
        }

        private static string NombreDestino_380_jh(MaestroPN1_380_jh destino)
        {
            if (destino == null) return string.Empty;
            if ((destino.Codigo_380_jh == "TRANSFUNDIDA" && destino.Descripcion_380_jh == "Transfundida") ||
                (destino.Codigo_380_jh == "DEVUELTA" && destino.Descripcion_380_jh == "Devuelta") ||
                (destino.Codigo_380_jh == "DESCARTADA" && destino.Descripcion_380_jh == "Descartada"))
                return ControlesPN2_380_jh.TraducirEstado_380_jh(destino.Codigo_380_jh);
            return destino.Descripcion_380_jh;
        }

        private void ActualizarEfecto_380_jh()
        {
            if (_lblEfecto_380_jh == null) return;
            string codigo = ControlesPN2_380_jh.EstadoSeleccionado_380_jh(_cmbDestino_380_jh);
            var destino = _destinos_380_jh.FirstOrDefault(x => x.Codigo_380_jh == codigo);
            _lblEfecto_380_jh.Text = destino == null ? ControlesPN2_380_jh.Traducir_380_jh("PN2_CHOOSE_DESTINATION") :
                string.Format(ControlesPN2_380_jh.Traducir_380_jh(destino.MotivoObligatorio_380_jh ? "PN2_DEST_EFFECT_REQUIRED" : "PN2_DEST_EFFECT_OPTIONAL"),
                    NombreDestino_380_jh(destino));
        }

        private void CargarEntregas_380_jh()
        {
            try
            {
                CargarOpciones_380_jh();
                new IntegridadApplicationService_380_jh().Verificar_380_jh();
                List<Entrega_380_jh> entregas = _puedeConsultarEntregas_380_jh
                    ? _pn2Service_380_jh.ListarEntregas_380_jh() : new List<Entrega_380_jh>();
                _entregas_380_jh.Clear();
                _entregas_380_jh.AddRange(entregas);
                _grid_380_jh.Enabled = true;
                PintarEntregas_380_jh();
            }
            catch { ControlesPN2_380_jh.MostrarErrorConsulta_380_jh(this, _grid_380_jh); }
        }

        private void PintarEntregas_380_jh()
        {
            if (_grid_380_jh == null) return;
            string buscar = _txtBuscar_380_jh.Text.Trim();
            string destino = ControlesPN2_380_jh.EstadoSeleccionado_380_jh(_cmbFiltroDestino_380_jh);
            _grid_380_jh.Rows.Clear();
            foreach (Entrega_380_jh entrega in _entregas_380_jh.Where(e =>
                (buscar.Length == 0 || (e.CodigoIdentificacion_380_jh ?? "").IndexOf(buscar, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (e.PacienteReceptor_380_jh ?? "").IndexOf(buscar, StringComparison.OrdinalIgnoreCase) >= 0 || e.IdSolicitudTransfusional_380_jh.ToString() == buscar) &&
                (destino.Length == 0 || (destino == "SIN_DESTINO" ? string.IsNullOrEmpty(e.Destino_380_jh) : e.Destino_380_jh == destino))))
            {
                int fila = _grid_380_jh.Rows.Add(entrega.Id_380_jh, entrega.IdSolicitudTransfusional_380_jh, entrega.CodigoIdentificacion_380_jh,
                    entrega.PacienteReceptor_380_jh, entrega.FechaEntrega_380_jh.ToString("dd/MM/yyyy HH:mm"), entrega.Responsable_380_jh,
                    string.IsNullOrEmpty(entrega.Destino_380_jh) ? ControlesPN2_380_jh.TraducirEstado_380_jh("SIN_DESTINO") :
                        (NombreDestino_380_jh(_destinos_380_jh.FirstOrDefault(x => x.Codigo_380_jh == entrega.Destino_380_jh)) is string nombre && nombre.Length > 0 ? nombre : entrega.Destino_380_jh),
                    entrega.FechaDestino_380_jh?.ToString("dd/MM/yyyy HH:mm"), entrega.MotivoDestino_380_jh);
                _grid_380_jh.Rows[fila].Tag = entrega;
            }
            _lblEstado_380_jh.Text = _grid_380_jh.Enabled ? string.Format(ControlesPN2_380_jh.Traducir_380_jh("PN2_RECORD_COUNT"), _grid_380_jh.Rows.Count)
                : ControlesPN2_380_jh.Traducir_380_jh("PN2_QUERY_ERROR");
        }

        private void Registrar_Click_380_jh(object sender, EventArgs e)
        {
            Entrega_380_jh entrega = ControlesPN2_380_jh.Seleccionado_380_jh<Entrega_380_jh>(_grid_380_jh);
            if (entrega == null) { MessageBox.Show(this, ControlesPN2_380_jh.Traducir_380_jh("PN2_SELECT_DELIVERY")); return; }
            if (!string.IsNullOrEmpty(entrega.Destino_380_jh)) { MessageBox.Show(this, ControlesPN2_380_jh.Traducir_380_jh("PN2_DESTINATION_ALREADY_SET")); return; }
            string destino = ControlesPN2_380_jh.EstadoSeleccionado_380_jh(_cmbDestino_380_jh);
            var opcion = _destinos_380_jh.FirstOrDefault(x => x.Activo_380_jh && x.Codigo_380_jh == destino);
            if (opcion == null) { MessageBox.Show(this, ControlesPN2_380_jh.Traducir_380_jh("PN2_CHOOSE_DESTINATION")); return; }
            if (opcion.MotivoObligatorio_380_jh && string.IsNullOrWhiteSpace(_txtMotivo_380_jh.Text))
            { MessageBox.Show(this, ControlesPN2_380_jh.Traducir_380_jh("PN2_REASON_REQUIRED")); return; }
            if (!ControlesPN2_380_jh.Confirmar_380_jh(this, "PN2_CONFIRM_DESTINATION", entrega.CodigoIdentificacion_380_jh + " · " + NombreDestino_380_jh(opcion))) return;
            CodigoOperacionPN2_380_jh resultado = _pn2Service_380_jh.RegistrarDestino_380_jh(entrega.Id_380_jh, destino, _txtMotivo_380_jh.Text);
            ControlesPN2_380_jh.MostrarResultado_380_jh(this, resultado);
            if (resultado != CodigoOperacionPN2_380_jh.Ok_380_jh) return;
            _txtMotivo_380_jh.Clear();
            CargarEntregas_380_jh();
            ComprobantePdfUI_380_jh.ProcesarPendientes_380_jh(this, true, false);
        }
    }
}
