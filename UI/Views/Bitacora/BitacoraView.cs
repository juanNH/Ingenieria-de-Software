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
            CargarBitacora_380_jh();
        }

        private void ConfigurarTraducciones_380_jh()
        {
            lblTitulo_380_jh.Tag = "AUDIT_TITLE";
            lblDescripcion_380_jh.Tag = "AUDIT_DESCRIPTION";
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
            ActualizarEstado_380_jh();
        }

        private void btnActualizar_Click_380_jh(object sender, EventArgs e)
        {
            CargarBitacora_380_jh();
        }

        private void CargarBitacora_380_jh()
        {
            listViewBitacora_380_jh.BeginUpdate();
            listViewBitacora_380_jh.Items.Clear();

            List<BitacoraRegistro_380_jh> registros = _bitacoraService_380_jh.Listar_380_jh();

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
    }
}
