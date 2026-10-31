using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Application;
using Domain;

namespace UI
{
    public class ReservasView_380_jh : LocalizedUserControl_380_jh
    {
        private readonly PN2ApplicationService_380_jh _pn2Service_380_jh;
        private readonly AutorizacionApplicationService_380_jh _autorizacionService_380_jh;
        private readonly List<Unidad_380_jh> _unidades_380_jh = new List<Unidad_380_jh>();
        private readonly List<Reserva_380_jh> _reservas_380_jh = new List<Reserva_380_jh>();
        private ComboBox _cmbSolicitud_380_jh;
        private DataGridView _gridUnidades_380_jh;
        private DataGridView _gridReservas_380_jh;
        private TextBox _txtObservacion_380_jh;
        private Button _btnReservar_380_jh;
        private Button _btnEntregar_380_jh;
        private Button _btnCancelar_380_jh;
        private Label _lblCantidades_380_jh;
        private bool _cargando_380_jh;

        public ReservasView_380_jh() : this(new PN2ApplicationService_380_jh(), new AutorizacionApplicationService_380_jh()) { }

        public ReservasView_380_jh(PN2ApplicationService_380_jh pn2Service, AutorizacionApplicationService_380_jh autorizacionService)
        {
            _pn2Service_380_jh = pn2Service;
            _autorizacionService_380_jh = autorizacionService;
            ConstruirInterfaz_380_jh();
            new ProteccionIntegridad_380_jh(this, _btnReservar_380_jh, _btnEntregar_380_jh, _btnCancelar_380_jh);
            CargarDatos_380_jh();
        }

        private void ConstruirInterfaz_380_jh()
        {
            TableLayoutPanel layout = ControlesPN2_380_jh.CrearLayout_380_jh(this, "PN2_RESERVATIONS_TITLE", "PN2_RESERVATIONS_STUDY_DESCRIPTION", 62, 180);
            FlowLayoutPanel filtros = ControlesPN2_380_jh.CrearFiltros_380_jh();
            _cmbSolicitud_380_jh = new ComboBox { Width = 620, DropDownStyle = ComboBoxStyle.DropDownList, FormattingEnabled = true,
                DropDownWidth = 800, DisplayMember = "PacienteReceptor_380_jh", ValueMember = "Id_380_jh" };
            _cmbSolicitud_380_jh.Format += delegate(object sender, ListControlConvertEventArgs e)
            {
                SolicitudTransfusional_380_jh solicitud = e.ListItem as SolicitudTransfusional_380_jh;
                if (solicitud != null) e.Value = string.Format("#{0} · {1} · {2} · {3}{4} · {5}", solicitud.Id_380_jh, solicitud.PacienteReceptor_380_jh,
                    solicitud.TipoComponente_380_jh, solicitud.GrupoReceptor_380_jh, solicitud.RhReceptor_380_jh,
                    ControlesPN2_380_jh.Traducir_380_jh(solicitud.IdUsuarioValidacion_380_jh.HasValue ? "PN2_STUDY_VALIDATED" : "PN2_STUDY_PENDING"));
            };
            _cmbSolicitud_380_jh.SelectedIndexChanged += delegate { if (!_cargando_380_jh) CargarReservas_380_jh(); };
            ControlesPN2_380_jh.AgregarFiltro_380_jh(filtros, "PN2_REQUEST_ID", _cmbSolicitud_380_jh);
            Button actualizar = ControlesPN2_380_jh.CrearBoton_380_jh("BTN_REFRESH");
            actualizar.Click += delegate { CargarDatos_380_jh(); };
            filtros.Controls.Add(actualizar);
            Button validar = ControlesPN2_380_jh.CrearBoton_380_jh("PN2_VALIDATE_STUDY");
            validar.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.ReservaCrear_380_jh);
            validar.Click += ValidarEstudio_Click_380_jh;
            filtros.Controls.Add(validar);
            layout.Controls.Add(filtros, 0, 2);

            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal,
                SplitterDistance = 50, Panel1MinSize = 25, Panel2MinSize = 25 };
            split.SizeChanged += delegate
            {
                int disponible = split.Height - split.SplitterWidth;
                if (disponible >= 60) split.SplitterDistance = Math.Max(25, Math.Min(disponible - 25, disponible / 2));
            };
            GroupBox unidades = new GroupBox { Dock = DockStyle.Fill, Tag = "PN2_CANDIDATE_UNITS" };
            _gridUnidades_380_jh = ControlesPN2_380_jh.CrearGrilla_380_jh();
            ControlesPN2_380_jh.AgregarColumna_380_jh(_gridUnidades_380_jh, "codigo", "UNIT_CODE");
            ControlesPN2_380_jh.AgregarColumna_380_jh(_gridUnidades_380_jh, "componente", "UNIT_COMPONENT", 130);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_gridUnidades_380_jh, "grupo", "UNIT_BLOOD_GROUP", 60);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_gridUnidades_380_jh, "rh", "UNIT_RH", 50);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_gridUnidades_380_jh, "vencimiento", "UNIT_EXPIRATION");
            ControlesPN2_380_jh.AgregarColumna_380_jh(_gridUnidades_380_jh, "ubicacion", "PN2_LOCATION");
            unidades.Controls.Add(_gridUnidades_380_jh);
            split.Panel1.Controls.Add(unidades);

            GroupBox reservas = new GroupBox { Dock = DockStyle.Fill, Tag = "MENU_RESERVATIONS" };
            _gridReservas_380_jh = ControlesPN2_380_jh.CrearGrilla_380_jh();
            ControlesPN2_380_jh.AgregarColumna_380_jh(_gridReservas_380_jh, "id", "PN2_RESERVATION_ID", 65);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_gridReservas_380_jh, "codigo", "UNIT_CODE", 110);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_gridReservas_380_jh, "fecha", "PN2_RESERVATION_DATE", 120);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_gridReservas_380_jh, "estado", "UNIT_STATUS", 90);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_gridReservas_380_jh, "responsable", "PN2_RESPONSIBLE", 125);
            ControlesPN2_380_jh.AgregarColumna_380_jh(_gridReservas_380_jh, "motivo", "PN2_CANCELLATION_REASON", 160);
            reservas.Controls.Add(_gridReservas_380_jh);
            split.Panel2.Controls.Add(reservas);
            layout.Controls.Add(split, 0, 3);

            Panel formularioPanel = new Panel { Dock = DockStyle.Fill };
            TableLayoutPanel formulario = ControlesPN2_380_jh.CrearFormulario_380_jh(formularioPanel, "PN2_RESERVATION_ACTIONS", 3);
            _lblCantidades_380_jh = new Label { Dock = DockStyle.Fill, AutoEllipsis = true };
            formulario.Controls.Add(_lblCantidades_380_jh, 0, 0);
            formulario.SetColumnSpan(_lblCantidades_380_jh, 4);
            _txtObservacion_380_jh = new TextBox { MaxLength = 450 };
            ControlesPN2_380_jh.AgregarCampo_380_jh(formulario, "PN2_COMPATIBILITY_OR_REASON", _txtObservacion_380_jh, 0, 1);
            formulario.SetColumnSpan(_txtObservacion_380_jh, 3);
            FlowLayoutPanel acciones = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, AutoScroll = true };
            _btnReservar_380_jh = ControlesPN2_380_jh.CrearBoton_380_jh("PN2_RESERVE");
            _btnReservar_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.ReservaCrear_380_jh);
            _btnReservar_380_jh.Click += Reservar_Click_380_jh;
            _btnEntregar_380_jh = ControlesPN2_380_jh.CrearBoton_380_jh("PN2_DELIVER");
            _btnEntregar_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.EntregaRegistrar_380_jh);
            _btnEntregar_380_jh.Click += Entregar_Click_380_jh;
            _btnCancelar_380_jh = ControlesPN2_380_jh.CrearBoton_380_jh("PN2_CANCEL_RESERVATION");
            _btnCancelar_380_jh.Visible = _autorizacionService_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.ReservaCancelar_380_jh);
            _btnCancelar_380_jh.Click += Cancelar_Click_380_jh;
            acciones.Controls.AddRange(new Control[] { _btnReservar_380_jh, _btnEntregar_380_jh, _btnCancelar_380_jh });
            formulario.Controls.Add(acciones, 0, 2);
            formulario.SetColumnSpan(acciones, 4);
            layout.Controls.Add(formularioPanel, 0, 4);
        }

        protected override void ApplyTranslations_380_jh()
        {
            base.ApplyTranslations_380_jh();
            PintarDatos_380_jh();
        }

        private void CargarDatos_380_jh()
        {
            int idPrevio = (_cmbSolicitud_380_jh.SelectedItem as SolicitudTransfusional_380_jh)?.Id_380_jh ?? 0;
            try
            {
                new IntegridadApplicationService_380_jh().Verificar_380_jh();
                List<SolicitudTransfusional_380_jh> solicitudes = _pn2Service_380_jh.ListarSolicitudes_380_jh();
                _cargando_380_jh = true;
                _cmbSolicitud_380_jh.DataSource = solicitudes;
                _cmbSolicitud_380_jh.SelectedItem = solicitudes.FirstOrDefault(s => s.Id_380_jh == idPrevio) ?? solicitudes.FirstOrDefault();
                _cargando_380_jh = false;
                CargarReservas_380_jh();
            }
            catch { _cargando_380_jh = false; ControlesPN2_380_jh.MostrarErrorConsulta_380_jh(this, _gridUnidades_380_jh, _gridReservas_380_jh); }
        }

        private void CargarReservas_380_jh()
        {
            SolicitudTransfusional_380_jh solicitud = _cmbSolicitud_380_jh.SelectedItem as SolicitudTransfusional_380_jh;
            try
            {
                List<Reserva_380_jh> reservas = solicitud == null ? new List<Reserva_380_jh>() : _pn2Service_380_jh.ListarReservas_380_jh(solicitud.Id_380_jh);
                List<Unidad_380_jh> unidades = solicitud == null ? new List<Unidad_380_jh>() : _pn2Service_380_jh.ListarCandidatas_380_jh(solicitud.Id_380_jh);
                _reservas_380_jh.Clear();
                _reservas_380_jh.AddRange(reservas);
                _unidades_380_jh.Clear();
                _unidades_380_jh.AddRange(unidades);
                _gridReservas_380_jh.Enabled = true;
                _gridUnidades_380_jh.Enabled = true;
                PintarDatos_380_jh();
            }
            catch { ControlesPN2_380_jh.MostrarErrorConsulta_380_jh(this, _gridUnidades_380_jh, _gridReservas_380_jh); }
        }

        private void PintarDatos_380_jh()
        {
            if (_gridUnidades_380_jh == null) return;
            SolicitudTransfusional_380_jh solicitud = _cmbSolicitud_380_jh.SelectedItem as SolicitudTransfusional_380_jh;
            _gridUnidades_380_jh.Rows.Clear();
            foreach (Unidad_380_jh unidad in _unidades_380_jh)
            {
                int fila = _gridUnidades_380_jh.Rows.Add(unidad.CodigoIdentificacion_380_jh, unidad.TipoComponente_380_jh, unidad.GrupoSanguineo_380_jh,
                    unidad.FactorRh_380_jh, unidad.FechaVencimiento_380_jh.ToString("dd/MM/yyyy"), unidad.Ubicacion_380_jh);
                _gridUnidades_380_jh.Rows[fila].Tag = unidad;
            }
            _gridReservas_380_jh.Rows.Clear();
            foreach (Reserva_380_jh reserva in _reservas_380_jh)
            {
                int fila = _gridReservas_380_jh.Rows.Add(reserva.Id_380_jh, reserva.CodigoIdentificacion_380_jh,
                    reserva.FechaReserva_380_jh.ToString("dd/MM/yyyy HH:mm"), ControlesPN2_380_jh.TraducirEstado_380_jh(reserva.EstadoReserva_380_jh),
                    reserva.Responsable_380_jh, reserva.MotivoCancelacion_380_jh);
                _gridReservas_380_jh.Rows[fila].Tag = reserva;
            }
            _lblCantidades_380_jh.Text = solicitud == null ? ControlesPN2_380_jh.Traducir_380_jh("PN2_SELECT_REQUEST") : string.Format(
                ControlesPN2_380_jh.Traducir_380_jh("PN2_REQUEST_COUNTS"), solicitud.CantidadRequerida_380_jh, solicitud.CantidadReservada_380_jh,
                solicitud.CantidadEntregada_380_jh, solicitud.SaldoPendiente_380_jh);
        }

        private void Reservar_Click_380_jh(object sender, EventArgs e)
        {
            SolicitudTransfusional_380_jh solicitud = _cmbSolicitud_380_jh.SelectedItem as SolicitudTransfusional_380_jh;
            Unidad_380_jh unidad = ControlesPN2_380_jh.Seleccionado_380_jh<Unidad_380_jh>(_gridUnidades_380_jh);
            if (solicitud == null || unidad == null) { MessageBox.Show(this, ControlesPN2_380_jh.Traducir_380_jh("PN2_SELECT_REQUEST_UNIT")); return; }
            if (string.IsNullOrWhiteSpace(_txtObservacion_380_jh.Text)) { MessageBox.Show(this, ControlesPN2_380_jh.Traducir_380_jh("PN2_COMPATIBILITY_REFERENCE_REQUIRED")); return; }
            if (MessageBox.Show(this, string.Format(ControlesPN2_380_jh.Traducir_380_jh("PN2_CONFIRM_COMPATIBILITY"), unidad.CodigoIdentificacion_380_jh, solicitud.PacienteReceptor_380_jh),
                ControlesPN2_380_jh.Traducir_380_jh("MENU_RESERVATIONS"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            Reserva_380_jh reserva = new Reserva_380_jh { IdSolicitudTransfusional_380_jh = solicitud.Id_380_jh, IdUnidad_380_jh = unidad.Id_380_jh, Observacion_380_jh = _txtObservacion_380_jh.Text };
            FinalizarOperacion_380_jh(_pn2Service_380_jh.Reservar_380_jh(reserva));
        }

        private void ValidarEstudio_Click_380_jh(object sender, EventArgs e)
        {
            SolicitudTransfusional_380_jh s = _cmbSolicitud_380_jh.SelectedItem as SolicitudTransfusional_380_jh;
            if (s == null) { MessageBox.Show(this, ControlesPN2_380_jh.Traducir_380_jh("PN2_SELECT_REQUEST")); return; }
            string detalle = string.Format(ControlesPN2_380_jh.Traducir_380_jh("PN2_STUDY_CONFIRM_DETAILS").Replace("\\n", Environment.NewLine), s.PacienteReceptor_380_jh,
                s.DocumentoReceptor_380_jh, s.GrupoReceptor_380_jh, s.RhReceptor_380_jh, s.LaboratorioEstudio_380_jh,
                s.ReferenciaEstudio_380_jh, s.FechaEstudio_380_jh.HasValue ? s.FechaEstudio_380_jh.Value.ToString("dd/MM/yyyy") : "-",
                s.ResultadoAnticuerpos_380_jh);
            if (s.IdUsuarioValidacion_380_jh.HasValue) { MessageBox.Show(this, detalle, ControlesPN2_380_jh.Traducir_380_jh("PN2_STUDY_VALIDATED")); return; }
            if (MessageBox.Show(this, detalle, ControlesPN2_380_jh.Traducir_380_jh("PN2_VALIDATE_STUDY"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            FinalizarOperacion_380_jh(_pn2Service_380_jh.ValidarEstudio_380_jh(s.Id_380_jh));
        }

        private void Entregar_Click_380_jh(object sender, EventArgs e)
        {
            Reserva_380_jh reserva = ControlesPN2_380_jh.Seleccionado_380_jh<Reserva_380_jh>(_gridReservas_380_jh);
            if (reserva == null) { MessageBox.Show(this, ControlesPN2_380_jh.Traducir_380_jh("PN2_SELECT_RESERVATION")); return; }
            if (!ControlesPN2_380_jh.Confirmar_380_jh(this, "PN2_CONFIRM_DELIVERY", reserva.CodigoIdentificacion_380_jh)) return;
            FinalizarOperacion_380_jh(_pn2Service_380_jh.Entregar_380_jh(reserva.Id_380_jh, _txtObservacion_380_jh.Text));
        }

        private void Cancelar_Click_380_jh(object sender, EventArgs e)
        {
            Reserva_380_jh reserva = ControlesPN2_380_jh.Seleccionado_380_jh<Reserva_380_jh>(_gridReservas_380_jh);
            if (reserva == null) { MessageBox.Show(this, ControlesPN2_380_jh.Traducir_380_jh("PN2_SELECT_RESERVATION")); return; }
            if (string.IsNullOrWhiteSpace(_txtObservacion_380_jh.Text)) { MessageBox.Show(this, ControlesPN2_380_jh.Traducir_380_jh("PN2_REASON_REQUIRED")); return; }
            if (!ControlesPN2_380_jh.Confirmar_380_jh(this, "PN2_CONFIRM_CANCEL", reserva.CodigoIdentificacion_380_jh)) return;
            FinalizarOperacion_380_jh(_pn2Service_380_jh.CancelarReserva_380_jh(reserva.Id_380_jh, _txtObservacion_380_jh.Text));
        }

        private void FinalizarOperacion_380_jh(CodigoOperacionPN2_380_jh resultado)
        {
            ControlesPN2_380_jh.MostrarResultado_380_jh(this, resultado);
            if (resultado != CodigoOperacionPN2_380_jh.Ok_380_jh) return;
            _txtObservacion_380_jh.Clear();
            CargarDatos_380_jh();
        }
    }
}
