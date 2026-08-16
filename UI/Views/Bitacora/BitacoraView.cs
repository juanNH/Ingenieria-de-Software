using System;
using System.Collections.Generic;
using Application;
using Domain;
using System.Windows.Forms;
using Services;

namespace UI
{
    public partial class BitacoraView_380_jh : LocalizedUserControl_380_jh
    {
        private readonly BitacoraApplicationService_380_jh _bitacoraService_380_jh;
        private int _cantidadRegistros_380_jh;

        public BitacoraView_380_jh()
            : this(new BitacoraApplicationService_380_jh())
        {
        }

        public BitacoraView_380_jh(BitacoraApplicationService_380_jh bitacoraService)
        {
            _bitacoraService_380_jh = bitacoraService;
            InitializeComponent_380_jh();
            ConfigurarTraducciones_380_jh();
            ConfigurarFiltros_380_jh();
            CargarBitacora_380_jh();
        }

        private void ConfigurarTraducciones_380_jh()
        {
            lblTitulo_380_jh.Tag = "AUDIT_TITLE";
            lblDescripcion_380_jh.Tag = "AUDIT_DESCRIPTION";
            grpFiltros_380_jh.Tag = "AUDIT_FILTERS";
            lblFechaDesde_380_jh.Tag = "AUDIT_FILTER_FROM";
            lblFechaHasta_380_jh.Tag = "AUDIT_FILTER_TO";
            lblUsuarioFiltro_380_jh.Tag = "AUDIT_FILTER_USER";
            lblModuloFiltro_380_jh.Tag = "AUDIT_FILTER_MODULE";
            lblAccionFiltro_380_jh.Tag = "AUDIT_FILTER_ACTION";
            lblNivelFiltro_380_jh.Tag = "AUDIT_FILTER_LEVEL";
            lblDescripcionFiltro_380_jh.Tag = "AUDIT_FILTER_DESCRIPTION";
            btnBuscar_380_jh.Tag = "BTN_SEARCH";
            btnLimpiarFiltros_380_jh.Tag = "BTN_CLEAR_FILTERS";
            btnActualizar_380_jh.Tag = "BTN_REFRESH";
            columnId_380_jh.Tag = "GRID_ID";
            columnFecha_380_jh.Tag = "GRID_DATE";
            columnIdUsuario_380_jh.Tag = "GRID_USER_ID";
            columnUsuario_380_jh.Tag = "GRID_USER";
            columnModulo_380_jh.Tag = "GRID_MODULE";
            columnAccion_380_jh.Tag = "GRID_ACTION";
            columnNivel_380_jh.Tag = "GRID_LEVEL";
            columnDescripcion_380_jh.Tag = "GRID_DESCRIPTION";
            columnEquipo_380_jh.Tag = "GRID_DEVICE";
        }

        protected override void ApplyTranslations_380_jh()
        {
            base.ApplyTranslations_380_jh();
            ActualizarOpcionesFiltros_380_jh();
            ActualizarEstado_380_jh();
        }

        private void btnActualizar_Click_380_jh(object sender, EventArgs e)
        {
            CargarBitacora_380_jh();
        }

        private void btnBuscar_Click_380_jh(object sender, EventArgs e)
        {
            CargarBitacora_380_jh();
        }

        private void btnLimpiarFiltros_Click_380_jh(object sender, EventArgs e)
        {
            dtpFechaDesde_380_jh.Checked = false;
            dtpFechaHasta_380_jh.Checked = false;
            txtUsuarioFiltro_380_jh.Clear();
            txtDescripcionFiltro_380_jh.Clear();
            cmbModuloFiltro_380_jh.SelectedIndex = 0;
            cmbAccionFiltro_380_jh.SelectedIndex = 0;
            cmbNivelFiltro_380_jh.SelectedIndex = 0;
            CargarBitacora_380_jh();
        }

        private void ConfigurarFiltros_380_jh()
        {
            cmbModuloFiltro_380_jh.Items.Add(new BitacoraFiltroOpcion_380_jh(null, "FILTER_ALL"));
            cmbModuloFiltro_380_jh.Items.Add(new BitacoraFiltroOpcion_380_jh("Seguridad_380_jh", "BITACORA_MODULE_SECURITY"));
            cmbModuloFiltro_380_jh.SelectedIndex = 0;

            cmbAccionFiltro_380_jh.Items.Add(new BitacoraFiltroOpcion_380_jh(null, "FILTER_ALL_ACTIONS"));
            cmbAccionFiltro_380_jh.Items.Add(new BitacoraFiltroOpcion_380_jh("LoginExitoso_380_jh", "BITACORA_ACTION_LOGIN_SUCCESS"));
            cmbAccionFiltro_380_jh.Items.Add(new BitacoraFiltroOpcion_380_jh("LoginFallido_380_jh", "BITACORA_ACTION_LOGIN_FAILURE"));
            cmbAccionFiltro_380_jh.Items.Add(new BitacoraFiltroOpcion_380_jh("RegistroFallido_380_jh", "BITACORA_ACTION_REGISTER_FAILURE"));
            cmbAccionFiltro_380_jh.SelectedIndex = 0;

            cmbNivelFiltro_380_jh.Items.Add(new BitacoraFiltroOpcion_380_jh(null, "FILTER_ALL"));
            cmbNivelFiltro_380_jh.Items.Add(new BitacoraFiltroOpcion_380_jh("Informacion_380_jh", "BITACORA_LEVEL_INFORMATION"));
            cmbNivelFiltro_380_jh.Items.Add(new BitacoraFiltroOpcion_380_jh("Advertencia_380_jh", "BITACORA_LEVEL_WARNING"));
            cmbNivelFiltro_380_jh.Items.Add(new BitacoraFiltroOpcion_380_jh("Error_380_jh", "BITACORA_LEVEL_ERROR"));
            cmbNivelFiltro_380_jh.SelectedIndex = 0;
            ActualizarOpcionesFiltros_380_jh();
        }

        private void ActualizarOpcionesFiltros_380_jh()
        {
            ActualizarOpcionesCombo_380_jh(cmbModuloFiltro_380_jh);
            ActualizarOpcionesCombo_380_jh(cmbAccionFiltro_380_jh);
            ActualizarOpcionesCombo_380_jh(cmbNivelFiltro_380_jh);
        }

        private static void ActualizarOpcionesCombo_380_jh(ComboBox combo)
        {
            if (combo == null)
            {
                return;
            }

            foreach (object item in combo.Items)
            {
                BitacoraFiltroOpcion_380_jh opcion = item as BitacoraFiltroOpcion_380_jh;
                if (opcion != null)
                {
                    opcion.Texto_380_jh = LanguageManager_380_jh.Instance_380_jh.Translate_380_jh(opcion.Clave_380_jh);
                }
            }

            combo.Refresh();
        }

        private BitacoraFiltro_380_jh ObtenerFiltro_380_jh()
        {
            return new BitacoraFiltro_380_jh
            {
                FechaDesde_380_jh = dtpFechaDesde_380_jh.Checked ? dtpFechaDesde_380_jh.Value.Date : (DateTime?)null,
                FechaHasta_380_jh = dtpFechaHasta_380_jh.Checked ? dtpFechaHasta_380_jh.Value.Date : (DateTime?)null,
                Usuario_380_jh = ObtenerTextoFiltro_380_jh(txtUsuarioFiltro_380_jh.Text),
                Modulo_380_jh = ObtenerSeleccionFiltro_380_jh(cmbModuloFiltro_380_jh),
                Accion_380_jh = ObtenerSeleccionFiltro_380_jh(cmbAccionFiltro_380_jh),
                Nivel_380_jh = ObtenerSeleccionFiltro_380_jh(cmbNivelFiltro_380_jh),
                Descripcion_380_jh = ObtenerTextoFiltro_380_jh(txtDescripcionFiltro_380_jh.Text)
            };
        }

        private static string ObtenerTextoFiltro_380_jh(string texto)
        {
            return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
        }

        private static string ObtenerSeleccionFiltro_380_jh(ComboBox combo)
        {
            BitacoraFiltroOpcion_380_jh opcion = combo == null ? null : combo.SelectedItem as BitacoraFiltroOpcion_380_jh;
            return opcion == null ? null : opcion.Valor_380_jh;
        }

        private void CargarBitacora_380_jh()
        {
            listViewBitacora_380_jh.BeginUpdate();
            listViewBitacora_380_jh.Items.Clear();

            List<BitacoraRegistro_380_jh> registros = _bitacoraService_380_jh.Listar_380_jh(ObtenerFiltro_380_jh());

            foreach (BitacoraRegistro_380_jh registro in registros)
            {
                ListViewItem item = new ListViewItem(registro.Id_380_jh.ToString());
                item.SubItems.Add(registro.Fecha_380_jh.ToString("dd/MM/yyyy HH:mm:ss"));
                item.SubItems.Add(registro.IdUsuario_380_jh.HasValue ? registro.IdUsuario_380_jh.Value.ToString() : string.Empty);
                item.SubItems.Add(registro.IdentificadorUsuario_380_jh ?? string.Empty);
                item.SubItems.Add(registro.Modulo_380_jh ?? string.Empty);
                item.SubItems.Add(registro.Accion_380_jh ?? string.Empty);
                item.SubItems.Add(registro.Nivel_380_jh ?? string.Empty);
                item.SubItems.Add(registro.Descripcion_380_jh ?? string.Empty);
                item.SubItems.Add(registro.Equipo_380_jh ?? string.Empty);

                listViewBitacora_380_jh.Items.Add(item);
            }

            listViewBitacora_380_jh.EndUpdate();
            _cantidadRegistros_380_jh = registros.Count;
            ActualizarEstado_380_jh();
        }

        private void ActualizarEstado_380_jh()
        {
            lblEstado_380_jh.Text = _cantidadRegistros_380_jh == 0
                ? LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("AUDIT_EMPTY")
                : string.Format(LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("AUDIT_COUNT"), _cantidadRegistros_380_jh);
        }

        private sealed class BitacoraFiltroOpcion_380_jh
        {
            public string Valor_380_jh { get; private set; }
            public string Clave_380_jh { get; private set; }
            public string Texto_380_jh { get; set; }

            public BitacoraFiltroOpcion_380_jh(string valor, string clave)
            {
                Valor_380_jh = valor;
                Clave_380_jh = clave;
                Texto_380_jh = clave;
            }

            public override string ToString()
            {
                return Texto_380_jh;
            }
        }
    }
}
