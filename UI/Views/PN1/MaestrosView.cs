using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Application;
using Domain;
using Services;

namespace UI
{
    public class MaestrosView_380_jh : LocalizedUserControl_380_jh
    {
        private readonly MaestroPN1ApplicationService_380_jh _service = new MaestroPN1ApplicationService_380_jh();
        private readonly List<Button> _acciones = new List<Button>();

        public MaestrosView_380_jh()
        {
            Padding = new Padding(24);
            BackColor = Color.White;
            var titulo = new Label { Dock = DockStyle.Top, Height = 42, Font = new Font("Segoe UI", 18, FontStyle.Bold), Tag = "MASTERS_TITLE" };
            var nota = new Label { Dock = DockStyle.Top, Height = 32, Tag = "MASTERS_HELP" };
            var tabs = new TabControl { Dock = DockStyle.Fill };
            Controls.Add(tabs);
            Controls.Add(nota);
            Controls.Add(titulo);
            CrearTab_380_jh(tabs, MaestroPN1ApplicationService_380_jh.Componente_380_jh, "MASTERS_COMPONENTS", 100);
            CrearTab_380_jh(tabs, MaestroPN1ApplicationService_380_jh.Grupo_380_jh, "MASTERS_GROUPS", 5);
            CrearTab_380_jh(tabs, MaestroPN1ApplicationService_380_jh.Rh_380_jh, "MASTERS_RH", 5);
            CrearTab_380_jh(tabs, MaestroPN1ApplicationService_380_jh.Destino_380_jh, "MASTERS_DESTINATIONS", 20);
            new ProteccionIntegridad_380_jh(this, _acciones.ToArray());
        }

        private void CrearTab_380_jh(TabControl tabs, string tipo, string titulo, int longitudCodigo)
        {
            var tab = new TabPage { Tag = titulo, Padding = new Padding(12) };
            tabs.TabPages.Add(tab);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            bool esDestino = tipo == MaestroPN1ApplicationService_380_jh.Destino_380_jh;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 135));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tab.Controls.Add(layout);
            layout.Controls.Add(new Label { Dock = DockStyle.Fill, AutoEllipsis = true,
                Tag = esDestino ? "MASTERS_DESTINATION_NOTE" : "MASTERS_COMPATIBILITY_NOTE" }, 0, 1);
            var formulario = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 3 };
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.Controls.Add(formulario, 0, 0);
            var codigo = new TextBox { Dock = DockStyle.Fill, MaxLength = longitudCodigo };
            var descripcion = new TextBox { Dock = DockStyle.Fill, MaxLength = 150 };
            var activo = new CheckBox { AutoSize = true, Checked = true, Tag = "MASTERS_ACTIVE" };
            CheckBox motivo = null;
            formulario.Controls.Add(new Label { Tag = esDestino ? "MASTERS_DESTINATION_STATE" : "MASTERS_CODE", AutoSize = true }, 0, 0);
            formulario.Controls.Add(codigo, 1, 0);
            formulario.Controls.Add(new Label { Tag = "MASTERS_DESCRIPTION", AutoSize = true }, 2, 0);
            formulario.Controls.Add(descripcion, 3, 0);
            formulario.Controls.Add(activo, 1, 1);
            if (esDestino)
            {
                motivo = new CheckBox { AutoSize = true, Tag = "MASTERS_REASON_REQUIRED" };
                formulario.Controls.Add(motivo, 3, 1);
                Action ajustarMotivo = () => {
                    bool motivoFijo = codigo.Text.Trim().Equals("DEVUELTA", StringComparison.OrdinalIgnoreCase) ||
                        codigo.Text.Trim().Equals("DESCARTADA", StringComparison.OrdinalIgnoreCase);
                    if (motivoFijo) motivo.Checked = true;
                    motivo.Enabled = !motivoFijo;
                };
                codigo.TextChanged += delegate { ajustarMotivo(); };
            }
            var acciones = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            formulario.Controls.Add(acciones, 1, 2);
            formulario.SetColumnSpan(acciones, 3);
            var nuevo = new Button { AutoSize = true, Tag = "BTN_NEW" };
            var guardar = new Button { AutoSize = true, Tag = "MASTERS_SAVE" };
            var baja = new Button { AutoSize = true, Tag = "MASTERS_DEACTIVATE" };
            acciones.Controls.Add(nuevo); acciones.Controls.Add(guardar); acciones.Controls.Add(baja);
            _acciones.Add(guardar); _acciones.Add(baja);
            var grilla = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
                MultiSelect = false, RowHeadersVisible = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToResizeRows = false, ColumnHeadersHeight = 32,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(248, 249, 250) } };
            grilla.Columns.Add(new DataGridViewTextBoxColumn { Name = "codigo", Tag = esDestino ? "MASTERS_DESTINATION_STATE" : "MASTERS_CODE" });
            grilla.Columns.Add(new DataGridViewTextBoxColumn { Name = "descripcion", Tag = "MASTERS_DESCRIPTION" });
            grilla.Columns.Add(new DataGridViewCheckBoxColumn { Name = "activo", Tag = "MASTERS_ACTIVE" });
            if (esDestino)
            {
                grilla.Columns.Add(new DataGridViewCheckBoxColumn { Name = "motivo", Tag = "MASTERS_REASON_REQUIRED" });
            }
            layout.Controls.Add(grilla, 0, 2);
            int id = 0;
            Action limpiar = () => { id = 0; codigo.ReadOnly = false; codigo.Clear(); descripcion.Clear(); activo.Checked = true; grilla.ClearSelection();
                if (esDestino) motivo.Checked = false; };
            Action cargar = () => {
                try {
                    grilla.Rows.Clear();
                    foreach (var item in _service.Listar_380_jh(tipo, false)) {
                        int fila = esDestino ? grilla.Rows.Add(item.Codigo_380_jh, item.Descripcion_380_jh, item.Activo_380_jh, item.MotivoObligatorio_380_jh)
                            : grilla.Rows.Add(item.Codigo_380_jh, item.Descripcion_380_jh, item.Activo_380_jh);
                        grilla.Rows[fila].Tag = item;
                    }
                    limpiar();
                } catch {
                    grilla.Rows.Clear();
                    grilla.Rows.Add(Traducir_380_jh("MASTERS_LOAD_ERROR"));
                    grilla.Enabled = false;
                }
            };
            grilla.SelectionChanged += delegate {
                if (grilla.SelectedRows.Count == 0) return;
                var item = grilla.SelectedRows[0].Tag as MaestroPN1_380_jh;
                if (item == null) return;
                id = item.Id_380_jh; codigo.Text = item.Codigo_380_jh; codigo.ReadOnly = true;
                descripcion.Text = item.Descripcion_380_jh; activo.Checked = item.Activo_380_jh;
                if (esDestino) motivo.Checked = item.MotivoObligatorio_380_jh;
            };
            nuevo.Click += delegate { limpiar(); codigo.Focus(); };
            Action<bool> guardarCambios = desactivar => {
                if (desactivar && id == 0) { MessageBox.Show(this, Traducir_380_jh("MASTERS_SELECT")); return; }
                try {
                    string resultado = _service.Guardar_380_jh(tipo, new MaestroPN1_380_jh {
                        Id_380_jh = id, Codigo_380_jh = codigo.Text, Descripcion_380_jh = descripcion.Text,
                        Activo_380_jh = desactivar ? false : activo.Checked,
                        MotivoObligatorio_380_jh = esDestino && motivo.Checked
                    });
                    MessageBox.Show(this, Traducir_380_jh("MASTERS_RESULT_" + resultado));
                    if (resultado == "OK") cargar();
                } catch { MessageBox.Show(this, Traducir_380_jh("MASTERS_RESULT_ERROR")); }
            };
            guardar.Click += delegate { guardarCambios(false); };
            baja.Click += delegate { guardarCambios(true); };
            cargar();
        }

        private static string Traducir_380_jh(string clave) => LanguageManager_380_jh.Instance_380_jh.Translate_380_jh(clave);
    }
}
