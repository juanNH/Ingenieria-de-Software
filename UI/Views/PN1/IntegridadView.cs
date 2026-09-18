using System;
using System.Drawing;
using System.Windows.Forms;
using Application;
using Domain;
using Services;

namespace UI
{
    public sealed class IntegridadView_380_jh : LocalizedUserControl_380_jh
    {
        private readonly IntegridadApplicationService_380_jh _servicio_380_jh = new IntegridadApplicationService_380_jh();
        private readonly AutorizacionApplicationService_380_jh _autorizacion_380_jh = new AutorizacionApplicationService_380_jh();
        private InformeIntegridad_380_jh _informe_380_jh;
        private readonly Label _estado_380_jh = new Label { Dock = DockStyle.Fill };
        private readonly DataGridView _grilla_380_jh = new DataGridView
        {
            Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            AutoGenerateColumns = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, RowHeadersVisible = false
        };
        private readonly TextBox _actual_380_jh = CrearTexto_380_jh(true);
        private readonly TextBox _confiable_380_jh = CrearTexto_380_jh(true);
        private readonly Button _inicializar_380_jh = CrearBoton_380_jh("INTEGRITY_INITIALIZE");
        private readonly Button _restaurar_380_jh = CrearBoton_380_jh("INTEGRITY_RESTORE");

        public IntegridadView_380_jh()
        {
            Padding = new Padding(16);
            BackColor = Color.White;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, AutoScroll = true };
            foreach (float alto in new[] { 48f, 54f, 44f }) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, alto));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            Controls.Add(layout);
            _estado_380_jh.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            layout.Controls.Add(_estado_380_jh, 0, 0);
            layout.Controls.Add(new Label { Tag = "INTEGRITY_DESCRIPTION", Dock = DockStyle.Fill }, 0, 1);
            var acciones = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, WrapContents = false };
            var verificar = CrearBoton_380_jh("INTEGRITY_VERIFY");
            verificar.Click += delegate { Cargar_380_jh(); };
            _inicializar_380_jh.Click += delegate { Recuperar_380_jh(true); };
            _restaurar_380_jh.Click += delegate { Recuperar_380_jh(false); };
            acciones.Controls.AddRange(new Control[] { verificar, _inicializar_380_jh, _restaurar_380_jh });
            layout.Controls.Add(acciones, 0, 2);
            foreach (string clave in new[] { "INTEGRITY_ENTITY", "INTEGRITY_ID", "INTEGRITY_TYPE", "INTEGRITY_DATE" })
                _grilla_380_jh.Columns.Add(new DataGridViewTextBoxColumn { Name = clave, Tag = clave });
            _grilla_380_jh.SelectionChanged += delegate { MostrarDetalle_380_jh(); };
            layout.Controls.Add(_grilla_380_jh, 0, 3);
            var titulos = CrearDosColumnas_380_jh();
            titulos.Controls.Add(new Label { Dock = DockStyle.Fill, Tag = "INTEGRITY_CURRENT" }, 0, 0);
            titulos.Controls.Add(new Label { Dock = DockStyle.Fill, Tag = "INTEGRITY_TRUSTED" }, 1, 0);
            layout.Controls.Add(titulos, 0, 4);
            var detalle = CrearDosColumnas_380_jh();
            detalle.Controls.Add(_actual_380_jh, 0, 0);
            detalle.Controls.Add(_confiable_380_jh, 1, 0);
            layout.Controls.Add(detalle, 0, 5);
            Load += delegate { Cargar_380_jh(); };
        }

        private void Cargar_380_jh()
        {
            if (!_autorizacion_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.IntegridadVer_380_jh))
            {
                Enabled = false;
                _estado_380_jh.Text = Traducir_380_jh("OPERATION_NOT_AUTHORIZED");
                return;
            }
            _informe_380_jh = _servicio_380_jh.Verificar_380_jh();
            Pintar_380_jh();
        }

        protected override void ApplyTranslations_380_jh()
        {
            base.ApplyTranslations_380_jh();
            Pintar_380_jh();
        }

        private void Pintar_380_jh()
        {
            if (_informe_380_jh == null) return;
            _estado_380_jh.Text = Traducir_380_jh(!_informe_380_jh.Disponible_380_jh ? "INTEGRITY_UNAVAILABLE" :
                _informe_380_jh.EsValida_380_jh ? "INTEGRITY_OK" : _informe_380_jh.PuedeInicializar_380_jh ?
                "INTEGRITY_PENDING" : _informe_380_jh.PuedeRestaurar_380_jh ? "INTEGRITY_BLOCKED" : "INTEGRITY_BACKUP_REQUIRED");
            _estado_380_jh.ForeColor = _informe_380_jh.EsValida_380_jh ? Color.DarkGreen : Color.DarkRed;
            bool permiso = _autorizacion_380_jh.TienePermiso_380_jh(PermisosSistema_380_jh.IntegridadRestaurar_380_jh);
            _inicializar_380_jh.Enabled = permiso && _informe_380_jh.PuedeInicializar_380_jh;
            _restaurar_380_jh.Enabled = permiso && _informe_380_jh.PuedeRestaurar_380_jh;
            _grilla_380_jh.Rows.Clear();
            _actual_380_jh.Clear();
            _confiable_380_jh.Clear();
            foreach (var incidencia in _informe_380_jh.Incidencias_380_jh)
            {
                int indice = _grilla_380_jh.Rows.Add(incidencia.Entidad_380_jh, incidencia.IdEntidad_380_jh,
                    Traducir_380_jh("INTEGRITY_TYPE_" + incidencia.Tipo_380_jh), incidencia.Fecha_380_jh);
                _grilla_380_jh.Rows[indice].Tag = incidencia;
            }
            MostrarDetalle_380_jh();
        }

        private void MostrarDetalle_380_jh()
        {
            var incidencia = _grilla_380_jh.CurrentRow?.Tag as IncidenciaIntegridad_380_jh;
            _actual_380_jh.Text = incidencia?.EstadoActual_380_jh ?? "";
            _confiable_380_jh.Text = incidencia?.EstadoConfiable_380_jh ?? "";
        }

        private void Recuperar_380_jh(bool inicializar)
        {
            if (_informe_380_jh == null) return;
            if (MessageBox.Show(this, Traducir_380_jh(inicializar ? "INTEGRITY_CONFIRM_BASELINE" : "INTEGRITY_CONFIRM_RESTORE"),
                Traducir_380_jh("MENU_INTEGRITY"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            string resultado = _servicio_380_jh.Recuperar_380_jh(_informe_380_jh.Revision_380_jh, inicializar);
            _informe_380_jh = IntegridadApplicationService_380_jh.Estado_380_jh;
            if (resultado == "INTEGRITY_RECOVERED" && !_informe_380_jh.EsValida_380_jh) resultado = "INTEGRITY_UNAVAILABLE";
            MessageBox.Show(Traducir_380_jh(resultado));
            Pintar_380_jh();
        }

        private static string Traducir_380_jh(string clave) => LanguageManager_380_jh.Instance_380_jh.Translate_380_jh(clave);
        private static Button CrearBoton_380_jh(string clave) => new Button { Tag = clave, AutoSize = true, Padding = new Padding(5) };
        private static TextBox CrearTexto_380_jh(bool lectura) => new TextBox
        { Dock = DockStyle.Fill, Multiline = true, ReadOnly = lectura, ScrollBars = ScrollBars.Both, WordWrap = false };
        private static TableLayoutPanel CrearDosColumnas_380_jh()
        {
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            return panel;
        }
    }
}
