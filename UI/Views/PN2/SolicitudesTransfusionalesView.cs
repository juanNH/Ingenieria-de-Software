using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Application;
using Domain;

namespace UI
{
    public class SolicitudesTransfusionalesView_380_jh : LocalizedUserControl_380_jh
    {
        private readonly PN2ApplicationService_380_jh _pn2Service_380_jh;
        private readonly AutorizacionApplicationService_380_jh _autorizacionService_380_jh;
        private readonly List<SolicitudTransfusional_380_jh> _solicitudes_380_jh = new List<SolicitudTransfusional_380_jh>();
        private TextBox _txtBuscar_380_jh;
        private TextBox _txtPaciente_380_jh;
        private TextBox _txtComponente_380_jh;
        private TextBox _txtDocumento_380_jh, _txtLaboratorio_380_jh, _txtReferencia_380_jh, _txtAnticuerpos_380_jh;
        private TextBox _txtSolicitante_380_jh;
        private ComboBox _cmbGrupo_380_jh, _cmbRh_380_jh;
        private DateTimePicker _fechaEstudio_380_jh;
        private NumericUpDown _numCantidad_380_jh;
        private DataGridView _grid_380_jh;
        private Button _btnRegistrar_380_jh;
        private Label _lblEstado_380_jh;

        public SolicitudesTransfusionalesView_380_jh() : this(new PN2ApplicationService_380_jh(), new AutorizacionApplicationService_380_jh()) { }

        public SolicitudesTransfusionalesView_380_jh(PN2ApplicationService_380_jh pn2Service, AutorizacionApplicationService_380_jh autorizacionService)
        {
            _pn2Service_380_jh = pn2Service;
            _autorizacionService_380_jh = autorizacionService;
            ConstruirInterfaz_380_jh();
            new ProteccionIntegridad_380_jh(this, _btnRegistrar_380_jh);
            CargarSolicitudes_380_jh();
        }

        private void ConstruirInterfaz_380_jh()
        {
            TableLayoutPanel layout = ControlesPN2_380_jh.CrearLayout_380_jh(this, "PN2_REQUESTS_TITLE", "PN2_REQUESTS_STUDY_DESCRIPTION", 50, 310);
            FlowLayoutPanel filtros = ControlesPN2_380_jh.CrearFiltros_380_jh();
            _txtBuscar_380_jh = new TextBox { Width = 280, MaxLength = 150 };
            _txtBuscar_380_jh.KeyDown += delegate(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) { CargarSolicitudes_380_jh(); e.SuppressKeyPress = true; } };
            ControlesPN2_380_jh.AgregarFiltro_380_jh(filtros, "PN2_SEARCH", _txtBuscar_380_jh);
            Button buscar = ControlesPN2_380_jh.CrearBoton_380_jh("BTN_SEARCH");
            buscar.Click += delegate { CargarSolicitudes_380_jh(); };
            filtros.Controls.Add(buscar);
            Button actualizar = ControlesPN2_380_jh.CrearBoton_380_jh("BTN_REFRESH");
            actualizar.Click += delegate { CargarSolicitudes_380_jh(); };
            filtros.Controls.Add(actualizar);
            Button comprobante = ControlesPN2_380_jh.CrearBoton_380_jh("RECEIPT_CURRENT_PDF");
            comprobante.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.SolicitudVer_380_jh) &&
                _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.ReservaVer_380_jh);
            comprobante.Click += delegate
            {
                SolicitudTransfusional_380_jh solicitud = ControlesPN2_380_jh.Seleccionado_380_jh<SolicitudTransfusional_380_jh>(_grid_380_jh);
                if (solicitud == null) { MessageBox.Show(this, ControlesPN2_380_jh.Traducir_380_jh("PN2_SELECT_REQUEST")); return; }
                ComprobantePdfUI_380_jh.GuardarPN2_380_jh(this, _pn2Service_380_jh, solicitud.Id_380_jh, null);
            };
            filtros.Controls.Add(comprobante);
            layout.Controls.Add(filtros, 0, 2);

            Panel grillaPanel = new Panel { Dock = DockStyle.Fill };
            _grid_380_jh = ControlesPN2_380_jh.CrearGrilla_380_jh();
            _grid_380_jh.SelectionChanged += SeleccionarSolicitud_380_jh;
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "id", "PN2_REQUEST_ID", 65);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "paciente", "PN2_PATIENT", 130);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "estudio", "PN2_STUDY_STATUS", 90);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "profesional", "PN2_REQUESTING_PROFESSIONAL", 130);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "fecha", "PN2_REQUEST_DATE", 100);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "componente", "UNIT_COMPONENT", 105);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "requerida", "PN2_REQUIRED", 70);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "reservada", "PN2_RESERVED", 70);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "entregada", "PN2_DELIVERED", 70);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "saldo", "PN2_PENDING", 70);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_grid_380_jh, "estado", "UNIT_STATUS", 100);
            grillaPanel.Controls.Add(_grid_380_jh);
            _lblEstado_380_jh = new Label { Dock = DockStyle.Bottom, Height = 24 };
            grillaPanel.Controls.Add(_lblEstado_380_jh);
            layout.Controls.Add(grillaPanel, 0, 3);

            Panel formularioPanel = new Panel { Dock = DockStyle.Fill };
            TableLayoutPanel formulario = ControlesPN2_380_jh.CrearFormulario_380_jh(formularioPanel, "PN2_NEW_REQUEST", 7);
            _txtPaciente_380_jh = new TextBox { MaxLength = 150 };
            _txtComponente_380_jh = new TextBox { MaxLength = 100 };
            _numCantidad_380_jh = new NumericUpDown { Minimum = 1, Maximum = int.MaxValue, Value = 1 };
            _txtDocumento_380_jh = new TextBox { MaxLength = 50 };
            _txtSolicitante_380_jh = new TextBox { MaxLength = 150 };
            _txtLaboratorio_380_jh = new TextBox { MaxLength = 150 };
            _txtReferencia_380_jh = new TextBox { MaxLength = 150 };
            _txtAnticuerpos_380_jh = new TextBox { MaxLength = 500 };
            _cmbGrupo_380_jh = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbGrupo_380_jh.Items.AddRange(new object[] { "O", "A", "B", "AB" });
            _cmbRh_380_jh = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbRh_380_jh.Items.AddRange(new object[] { "+", "-" });
            _fechaEstudio_380_jh = new DateTimePicker { Format = DateTimePickerFormat.Short, MaxDate = DateTime.Today, ShowCheckBox = true, Checked = false };
            ControlesPN2_380_jh.AgregarCampo_380_jh(formulario, "PN2_PATIENT", _txtPaciente_380_jh, 0, 0);
            ControlesPN2_380_jh.AgregarCampo_380_jh(formulario, "UNIT_COMPONENT", _txtComponente_380_jh, 2, 0);
            ControlesPN2_380_jh.AgregarCampo_380_jh(formulario, "PN2_REQUIRED", _numCantidad_380_jh, 0, 1);
            ControlesPN2_380_jh.AgregarCampo_380_jh(formulario, "PN2_PATIENT_DOCUMENT", _txtDocumento_380_jh, 2, 1);
            ControlesPN2_380_jh.AgregarCampo_380_jh(formulario, "UNIT_BLOOD_GROUP", _cmbGrupo_380_jh, 0, 2);
            ControlesPN2_380_jh.AgregarCampo_380_jh(formulario, "UNIT_RH", _cmbRh_380_jh, 2, 2);
            ControlesPN2_380_jh.AgregarCampo_380_jh(formulario, "PN2_LABORATORY", _txtLaboratorio_380_jh, 0, 3);
            ControlesPN2_380_jh.AgregarCampo_380_jh(formulario, "PN2_STUDY_REFERENCE", _txtReferencia_380_jh, 2, 3);
            ControlesPN2_380_jh.AgregarCampo_380_jh(formulario, "PN2_STUDY_DATE", _fechaEstudio_380_jh, 0, 4);
            ControlesPN2_380_jh.AgregarCampo_380_jh(formulario, "PN2_ANTIBODIES", _txtAnticuerpos_380_jh, 2, 4);
            ControlesPN2_380_jh.AgregarCampo_380_jh(formulario, "PN2_REQUESTING_PROFESSIONAL", _txtSolicitante_380_jh, 0, 5);
            _btnRegistrar_380_jh = ControlesPN2_380_jh.CrearBoton_380_jh("BTN_REGISTER");
            _btnRegistrar_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.SolicitudCrear_380_jh);
            _btnRegistrar_380_jh.Click += Registrar_Click_380_jh;
            Button actualizarEstudio = ControlesPN2_380_jh.CrearBoton_380_jh("PN2_UPDATE_STUDY");
            actualizarEstudio.Visible = _btnRegistrar_380_jh.Visible;
            actualizarEstudio.Click += ActualizarEstudio_Click_380_jh;
            formulario.Controls.Add(actualizarEstudio, 2, 5);
            formulario.Controls.Add(_btnRegistrar_380_jh, 3, 5);
            Label nota = ControlesPN2_380_jh.CrearLabel_380_jh("PN2_QUANTITY_NOTE");
            formulario.Controls.Add(nota, 0, 6);
            formulario.SetColumnSpan(nota, 4);
            layout.Controls.Add(formularioPanel, 0, 4);
        }

        protected override void ApplyTranslations_380_jh()
        {
            base.ApplyTranslations_380_jh();
            PintarSolicitudes_380_jh();
        }

        private void CargarSolicitudes_380_jh()
        {
            try
            {
                new IntegridadApplicationService_380_jh().Verificar_380_jh();
                List<SolicitudTransfusional_380_jh> solicitudes = _pn2Service_380_jh.ListarSolicitudes_380_jh(_txtBuscar_380_jh.Text);
                _solicitudes_380_jh.Clear();
                _solicitudes_380_jh.AddRange(solicitudes);
                _grid_380_jh.Enabled = true;
                PintarSolicitudes_380_jh();
            }
            catch { ControlesPN2_380_jh.MostrarErrorConsulta_380_jh(this, _grid_380_jh); }
        }

        private void PintarSolicitudes_380_jh()
        {
            if (_grid_380_jh == null) return;
            _grid_380_jh.Rows.Clear();
            foreach (SolicitudTransfusional_380_jh solicitud in _solicitudes_380_jh)
            {
                int fila = _grid_380_jh.Rows.Add(solicitud.Id_380_jh, solicitud.PacienteReceptor_380_jh,
                    ControlesPN2_380_jh.Traducir_380_jh(solicitud.IdUsuarioValidacion_380_jh.HasValue ? "PN2_STUDY_VALIDATED" : "PN2_STUDY_PENDING"),
                    solicitud.ProfesionalSolicitante_380_jh, solicitud.FechaSolicitud_380_jh.ToString("dd/MM/yyyy HH:mm"), solicitud.TipoComponente_380_jh,
                    solicitud.CantidadRequerida_380_jh, solicitud.CantidadReservada_380_jh, solicitud.CantidadEntregada_380_jh,
                    solicitud.SaldoPendiente_380_jh, ControlesPN2_380_jh.TraducirEstado_380_jh(solicitud.EstadoSolicitud_380_jh));
                _grid_380_jh.Rows[fila].Tag = solicitud;
            }
            SeleccionarSolicitud_380_jh(null, EventArgs.Empty);
            _lblEstado_380_jh.Text = _grid_380_jh.Enabled ? string.Format(ControlesPN2_380_jh.Traducir_380_jh("PN2_RECORD_COUNT"), _solicitudes_380_jh.Count)
                : ControlesPN2_380_jh.Traducir_380_jh("PN2_QUERY_ERROR");
        }

        private void Registrar_Click_380_jh(object sender, EventArgs e)
        {
            SolicitudTransfusional_380_jh solicitud = new SolicitudTransfusional_380_jh
            {
                PacienteReceptor_380_jh = _txtPaciente_380_jh.Text,
                DocumentoReceptor_380_jh = _txtDocumento_380_jh.Text,
                ProfesionalSolicitante_380_jh = _txtSolicitante_380_jh.Text,
                GrupoReceptor_380_jh = _cmbGrupo_380_jh.SelectedItem as string,
                RhReceptor_380_jh = _cmbRh_380_jh.SelectedItem as string,
                LaboratorioEstudio_380_jh = _txtLaboratorio_380_jh.Text,
                ReferenciaEstudio_380_jh = _txtReferencia_380_jh.Text,
                FechaEstudio_380_jh = _fechaEstudio_380_jh.Checked ? (DateTime?)_fechaEstudio_380_jh.Value.Date : null,
                ResultadoAnticuerpos_380_jh = _txtAnticuerpos_380_jh.Text,
                TipoComponente_380_jh = _txtComponente_380_jh.Text,
                CantidadRequerida_380_jh = Convert.ToInt32(_numCantidad_380_jh.Value)
            };
            CodigoOperacionPN2_380_jh resultado = _pn2Service_380_jh.RegistrarSolicitud_380_jh(solicitud);
            ControlesPN2_380_jh.MostrarResultado_380_jh(this, resultado);
            if (resultado != CodigoOperacionPN2_380_jh.Ok_380_jh) return;
            _txtPaciente_380_jh.Clear();
            _txtComponente_380_jh.Clear();
            _txtDocumento_380_jh.Clear(); _txtSolicitante_380_jh.Clear(); _txtLaboratorio_380_jh.Clear(); _txtReferencia_380_jh.Clear(); _txtAnticuerpos_380_jh.Clear();
            _cmbGrupo_380_jh.SelectedIndex = -1; _cmbRh_380_jh.SelectedIndex = -1;
            _fechaEstudio_380_jh.Checked = false;
            _numCantidad_380_jh.Value = 1;
            CargarSolicitudes_380_jh();
        }

        private void SeleccionarSolicitud_380_jh(object sender, EventArgs e)
        {
            if (_txtDocumento_380_jh == null) return;
            SolicitudTransfusional_380_jh s = ControlesPN2_380_jh.Seleccionado_380_jh<SolicitudTransfusional_380_jh>(_grid_380_jh);
            if (s == null) return;
            _txtPaciente_380_jh.Text = s.PacienteReceptor_380_jh;
            _txtComponente_380_jh.Text = s.TipoComponente_380_jh;
            _txtSolicitante_380_jh.Text = s.ProfesionalSolicitante_380_jh;
            _txtDocumento_380_jh.Text = s.DocumentoReceptor_380_jh;
            _cmbGrupo_380_jh.SelectedItem = s.GrupoReceptor_380_jh;
            _cmbRh_380_jh.SelectedItem = s.RhReceptor_380_jh;
            _txtLaboratorio_380_jh.Text = s.LaboratorioEstudio_380_jh;
            _txtReferencia_380_jh.Text = s.ReferenciaEstudio_380_jh;
            _txtAnticuerpos_380_jh.Text = s.ResultadoAnticuerpos_380_jh;
            _fechaEstudio_380_jh.Checked = s.FechaEstudio_380_jh.HasValue;
            if (s.FechaEstudio_380_jh.HasValue) _fechaEstudio_380_jh.Value = s.FechaEstudio_380_jh.Value;
        }

        private void ActualizarEstudio_Click_380_jh(object sender, EventArgs e)
        {
            SolicitudTransfusional_380_jh seleccion = ControlesPN2_380_jh.Seleccionado_380_jh<SolicitudTransfusional_380_jh>(_grid_380_jh);
            if (seleccion == null) { MessageBox.Show(this, ControlesPN2_380_jh.Traducir_380_jh("PN2_SELECT_REQUEST")); return; }
            var datos = new SolicitudTransfusional_380_jh {
                Id_380_jh = seleccion.Id_380_jh, DocumentoReceptor_380_jh = _txtDocumento_380_jh.Text,
                ProfesionalSolicitante_380_jh = _txtSolicitante_380_jh.Text,
                GrupoReceptor_380_jh = _cmbGrupo_380_jh.SelectedItem as string, RhReceptor_380_jh = _cmbRh_380_jh.SelectedItem as string,
                LaboratorioEstudio_380_jh = _txtLaboratorio_380_jh.Text, ReferenciaEstudio_380_jh = _txtReferencia_380_jh.Text,
                FechaEstudio_380_jh = _fechaEstudio_380_jh.Checked ? (DateTime?)_fechaEstudio_380_jh.Value.Date : null,
                ResultadoAnticuerpos_380_jh = _txtAnticuerpos_380_jh.Text
            };
            CodigoOperacionPN2_380_jh resultado = _pn2Service_380_jh.ActualizarEstudio_380_jh(datos);
            ControlesPN2_380_jh.MostrarResultado_380_jh(this, resultado);
            if (resultado == CodigoOperacionPN2_380_jh.Ok_380_jh) CargarSolicitudes_380_jh();
        }
    }
}
