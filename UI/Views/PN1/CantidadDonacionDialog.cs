using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Application;
using Domain;
using Services;

namespace UI
{
    internal sealed class CantidadDonacionDialog_380_jh : Form
    {
        private readonly DonacionApplicationService_380_jh _service;
        private readonly int _idDonacion;
        private readonly CantidadDonacion_380_jh _datos;
        private readonly DataGridView _historial = new DataGridView();
        private readonly TextBox _motivo = new TextBox { Dock = DockStyle.Fill, MaxLength = 500 };
        private readonly TextBox _codigos = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Vertical };
        private readonly CheckedListBox _unidades = new CheckedListBox { Dock = DockStyle.Fill, CheckOnClick = true, DisplayMember = nameof(Unidad_380_jh.CodigoIdentificacion_380_jh) };
        private readonly Label _seleccion = new Label { Dock = DockStyle.Fill };
        private readonly Label _cantidad = new Label { Dock = DockStyle.Fill };
        private readonly int _unidadesFijas;

        public CantidadDonacionDialog_380_jh(DonacionApplicationService_380_jh service, int idDonacion, CantidadDonacion_380_jh datos)
        {
            _service = service;
            _idDonacion = idDonacion;
            _datos = datos;
            _unidadesFijas = datos.Cantidad_380_jh - datos.Ajustables_380_jh.Count(EstaVigente_380_jh);
            Text = string.Format(T("DONATION_QUANTITY_TITLE"), idDonacion);
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(860, datos.Editable_380_jh ? 525 : 355);
            MinimumSize = new Size(700, datos.Editable_380_jh ? 525 : 320);
            ShowInTaskbar = false;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 1 };
            Controls.Add(layout);
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
            layout.Controls.Add(new Label { AutoSize = true, Text = string.Format(T("DONATION_QUANTITY_CURRENT"), datos.Cantidad_380_jh) +
                (datos.Editable_380_jh ? "" : " · " + T("DONATION_QUANTITY_READ_ONLY")) }, 0, 0);
            ConfigurarHistorial_380_jh();
            layout.Controls.Add(_historial, 0, 1);
            layout.Controls.Add(_codigos, 0, 2);
            _historial.SelectionChanged += delegate { _codigos.Text = Seleccion_380_jh()?.Codigos_380_jh ?? ""; };
            _codigos.Text = Seleccion_380_jh()?.Codigos_380_jh ?? "";
            if (datos.Editable_380_jh) ConfigurarEdicion_380_jh(layout);
        }

        private void ConfigurarHistorial_380_jh()
        {
            _historial.Dock = DockStyle.Fill;
            _historial.ReadOnly = true;
            _historial.AllowUserToAddRows = false;
            _historial.AllowUserToDeleteRows = false;
            _historial.MultiSelect = false;
            _historial.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _historial.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            foreach (string clave in new[] { "DONATION_QUANTITY_DATE", "DONATION_QUANTITY_ACTOR", "DONATION_QUANTITY_ACTION", "DONATION_QUANTITY_BEFORE", "DONATION_QUANTITY_AFTER", "DONATION_QUANTITY_REASON" })
                _historial.Columns.Add(clave, T(clave));

            foreach (var cambio in _datos.Historial_380_jh)
            {
                string clave = cambio.Accion_380_jh == "RESTAURA_CANTIDAD" ? "DONATION_QUANTITY_RESTORED" :
                    cambio.Accion_380_jh == "AJUSTE_CANTIDAD" ? "DONATION_QUANTITY_ADJUSTED" : "DONATION_QUANTITY_INITIAL";
                int fila = _historial.Rows.Add(cambio.Fecha_380_jh.ToString("g"), cambio.Responsable_380_jh, T(clave),
                    cambio.Anterior_380_jh, cambio.Nueva_380_jh, cambio.Motivo_380_jh);
                _historial.Rows[fila].Tag = cambio;
            }
        }

        private void ConfigurarEdicion_380_jh(TableLayoutPanel layout)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 31));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 31));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 23));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 37));
            layout.Controls.Add(_cantidad, 0, 3);
            _motivo.AccessibleName = T("DONATION_QUANTITY_REASON");
            var motivo = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            motivo.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            motivo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            motivo.Controls.Add(new Label { AutoSize = true, Text = T("DONATION_QUANTITY_REASON") }, 0, 0);
            motivo.Controls.Add(_motivo, 1, 0);
            layout.Controls.Add(motivo, 0, 4);
            layout.Controls.Add(_seleccion, 0, 5);
            layout.Controls.Add(_unidades, 0, 6);
            foreach (var unidad in _datos.Ajustables_380_jh)
                _unidades.Items.Add(unidad, EstaVigente_380_jh(unidad));
            _seleccion.Text = T("DONATION_QUANTITY_SELECT_EXISTING");
            ActualizarCantidad_380_jh(_unidades.CheckedItems.Count);
            _unidades.ItemCheck += (sender, args) =>
            {
                int antes = args.CurrentValue == CheckState.Checked ? 1 : 0;
                int despues = args.NewValue == CheckState.Checked ? 1 : 0;
                ActualizarCantidad_380_jh(_unidades.CheckedItems.Count - antes + despues);
            };

            var botones = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            var guardar = new Button { Text = T("MASTERS_SAVE"), AutoSize = true };
            guardar.Click += Guardar_380_jh;
            botones.Controls.Add(guardar);
            layout.Controls.Add(botones, 0, 7);
        }

        private static bool EstaVigente_380_jh(Unidad_380_jh unidad) => unidad.EstadoOperativo_380_jh != EstadoUnidadPN1_380_jh.Anulada_380_jh;

        private void ActualizarCantidad_380_jh(int seleccionadas)
        {
            _cantidad.Text = string.Format(T("DONATION_QUANTITY_NEXT_TOTAL"), _unidadesFijas + seleccionadas);
        }

        private void Guardar_380_jh(object sender, EventArgs e)
        {
            int cantidad = _unidadesFijas + _unidades.CheckedItems.Count;
            var afectadas = _unidades.Items.Cast<Unidad_380_jh>()
                .Where((unidad, indice) => _unidades.GetItemChecked(indice) != EstaVigente_380_jh(unidad))
                .Select(unidad => unidad.Id_380_jh).ToArray();
            if (cantidad < 1 || cantidad > 1000 || string.IsNullOrWhiteSpace(_motivo.Text) || afectadas.Length == 0)
            {
                MessageBox.Show(this, T("DONATION_QUANTITY_INVALID"));
                return;
            }

            CodigoOperacionPN1_380_jh resultado;
            try { resultado = _service.AjustarCantidad_380_jh(_idDonacion, _datos.Version_380_jh, cantidad, _motivo.Text, afectadas, null); }
            catch (InvalidOperationException) { resultado = CodigoOperacionPN1_380_jh.Error_380_jh; }
            switch (resultado)
            {
                case CodigoOperacionPN1_380_jh.Ok_380_jh: DialogResult = DialogResult.OK; break;
                case CodigoOperacionPN1_380_jh.ConflictoVersion_380_jh:
                case CodigoOperacionPN1_380_jh.UnidadesNoAjustables_380_jh:
                    MessageBox.Show(this, T("DONATION_QUANTITY_STALE")); DialogResult = DialogResult.Retry; break;
                case CodigoOperacionPN1_380_jh.DonacionCerrada_380_jh:
                    MessageBox.Show(this, T("DONATION_QUANTITY_READ_ONLY")); DialogResult = DialogResult.Retry; break;
                case CodigoOperacionPN1_380_jh.DatosInvalidos_380_jh:
                    MessageBox.Show(this, T("DONATION_QUANTITY_INVALID")); break;
                default:
                    MessageBox.Show(this, T("DONATION_QUANTITY_ERROR")); break;
            }
        }

        private CambioCantidadDonacion_380_jh Seleccion_380_jh() => _historial.CurrentRow?.Tag as CambioCantidadDonacion_380_jh;

        private static string T(string clave) => LanguageManager_380_jh.Instance_380_jh.Translate_380_jh(clave);
    }
}
