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
        private readonly NumericUpDown _unidadesNuevas = new NumericUpDown { Width = 80, Maximum = 1000 };
        private readonly Button _restaurar = new Button { AutoSize = true };
        private readonly Label _estadoRestauracion = new Label { AutoSize = true, Anchor = AnchorStyles.Left };
        private readonly Label _seleccion = new Label { Dock = DockStyle.Fill };
        private readonly Label _cantidad = new Label { AutoSize = true, Anchor = AnchorStyles.Left };
        private readonly int _unidadesFijas;
        private CambioCantidadDonacion_380_jh _versionRestaurar;
        private bool[] _seleccionAntesDeRestaurar;
        private decimal _nuevasAntesDeRestaurar;

        public CantidadDonacionDialog_380_jh(DonacionApplicationService_380_jh service, int idDonacion, CantidadDonacion_380_jh datos)
        {
            _service = service;
            _idDonacion = idDonacion;
            _datos = datos;
            _unidadesFijas = datos.Cantidad_380_jh - datos.Ajustables_380_jh.Count(EstaVigente_380_jh);
            bool puedeAjustar = datos.Editable_380_jh &&
                new AutorizacionApplicationService_380_jh().TienePermiso_380_jh(PermisosSistema_380_jh.DonacionAjustarCantidad_380_jh);
            bool hayAjustePosible = datos.Cantidad_380_jh < 1000 ||
                (_unidadesFijas <= 1000 && datos.Ajustables_380_jh.Any(EstaVigente_380_jh));
            bool editable = puedeAjustar && hayAjustePosible;
            Text = string.Format(T("DONATION_QUANTITY_TITLE"), idDonacion);
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(860, editable ? 575 : 355);
            MinimumSize = new Size(700, editable ? 560 : 320);
            ShowInTaskbar = false;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 1 };
            Controls.Add(layout);
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
            layout.Controls.Add(new Label { AutoSize = true, Text = string.Format(T("DONATION_QUANTITY_CURRENT"), datos.Cantidad_380_jh) }, 0, 0);
            ConfigurarHistorial_380_jh();
            layout.Controls.Add(_historial, 0, 1);
            layout.Controls.Add(_codigos, 0, 2);
            _historial.CurrentCellChanged += delegate { ActualizarSeleccionHistorial_380_jh(); };
            if (editable) ConfigurarEdicion_380_jh(layout);
            else
            {
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
                layout.Controls.Add(new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft,
                    Text = T(puedeAjustar ? "DONATION_QUANTITY_NO_ADJUSTMENT" : "DONATION_QUANTITY_READ_ONLY") }, 0, 3);
            }
            ActualizarSeleccionHistorial_380_jh();
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
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 31));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 37));

            var accionesHistorial = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            _restaurar.Click += RestaurarSeleccion_380_jh;
            accionesHistorial.Controls.Add(_restaurar);
            accionesHistorial.Controls.Add(_estadoRestauracion);
            layout.Controls.Add(accionesHistorial, 0, 3);

            _seleccion.Text = T("DONATION_QUANTITY_SELECT_EXISTING");
            layout.Controls.Add(_seleccion, 0, 4);
            layout.Controls.Add(_unidades, 0, 5);
            foreach (var unidad in _datos.Ajustables_380_jh.OrderBy(unidad => unidad.Id_380_jh))
                _unidades.Items.Add(unidad, EstaVigente_380_jh(unidad));

            var cantidades = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            string unidadesNuevas = T("DONATION_QUANTITY_ADD_UNITS");
            _unidadesNuevas.AccessibleName = unidadesNuevas;
            cantidades.Controls.Add(new Label { AutoSize = true, Anchor = AnchorStyles.Left, Text = unidadesNuevas });
            cantidades.Controls.Add(_unidadesNuevas);
            cantidades.Controls.Add(_cantidad);
            layout.Controls.Add(cantidades, 0, 6);

            _motivo.AccessibleName = T("DONATION_QUANTITY_REASON");
            var motivo = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            motivo.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            motivo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            motivo.Controls.Add(new Label { AutoSize = true, Text = T("DONATION_QUANTITY_REASON") }, 0, 0);
            motivo.Controls.Add(_motivo, 1, 0);
            layout.Controls.Add(motivo, 0, 7);
            string formatoCantidad = T("DONATION_QUANTITY_NEXT_TOTAL");
            ActualizarCantidad_380_jh(formatoCantidad, _unidades.CheckedItems.Count);
            _unidadesNuevas.ValueChanged += delegate { ActualizarCantidad_380_jh(formatoCantidad, _unidades.CheckedItems.Count); };
            _unidades.ItemCheck += (sender, args) =>
            {
                int antes = args.CurrentValue == CheckState.Checked ? 1 : 0;
                int despues = args.NewValue == CheckState.Checked ? 1 : 0;
                ActualizarCantidad_380_jh(formatoCantidad, _unidades.CheckedItems.Count - antes + despues);
            };

            var botones = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            var guardar = new Button { Text = T("MASTERS_SAVE"), AutoSize = true };
            guardar.Click += Guardar_380_jh;
            botones.Controls.Add(guardar);
            layout.Controls.Add(botones, 0, 8);
        }

        private static bool EstaVigente_380_jh(Unidad_380_jh unidad) => unidad.EstadoOperativo_380_jh != EstadoUnidadPN1_380_jh.Anulada_380_jh;

        private void ActualizarCantidad_380_jh(string formato, int seleccionadas)
        {
            _cantidad.Text = string.Format(formato, _unidadesFijas + seleccionadas + (int)_unidadesNuevas.Value);
        }

        private void ActualizarSeleccionHistorial_380_jh()
        {
            var seleccion = Seleccion_380_jh();
            _codigos.Text = seleccion?.Codigos_380_jh ?? "";
            _restaurar.Text = T(_versionRestaurar == null ? "DONATION_QUANTITY_RESTORE" : "DONATION_QUANTITY_CANCEL_RESTORE");
            _restaurar.Enabled = _versionRestaurar != null ||
                (seleccion != null && seleccion.Version_380_jh < _datos.Version_380_jh && seleccion.Nueva_380_jh != _datos.Cantidad_380_jh);
        }

        private void RestaurarSeleccion_380_jh(object sender, EventArgs e)
        {
            if (_versionRestaurar != null)
            {
                _versionRestaurar = null;
                for (int i = 0; i < _seleccionAntesDeRestaurar.Length; i++)
                    _unidades.SetItemChecked(i, _seleccionAntesDeRestaurar[i]);
                _seleccionAntesDeRestaurar = null;
                _unidadesNuevas.Value = _nuevasAntesDeRestaurar;
                _unidadesNuevas.Enabled = true;
                _estadoRestauracion.Text = "";
                ActualizarSeleccionHistorial_380_jh();
                return;
            }

            var seleccion = Seleccion_380_jh();
            if (seleccion == null || seleccion.Version_380_jh >= _datos.Version_380_jh || seleccion.Nueva_380_jh == _datos.Cantidad_380_jh) return;
            int objetivo = seleccion.Nueva_380_jh;
            if (objetivo < Math.Max(1, _unidadesFijas) || objetivo > Math.Min(1000, _unidadesFijas + _unidades.Items.Count))
            {
                MessageBox.Show(this, T("DONATION_QUANTITY_RESTORE_UNAVAILABLE"));
                return;
            }

            var indices = Enumerable.Range(0, _unidades.Items.Count);
            _seleccionAntesDeRestaurar = indices.Select(_unidades.GetItemChecked).ToArray();
            _nuevasAntesDeRestaurar = _unidadesNuevas.Value;
            _versionRestaurar = seleccion;
            _unidadesNuevas.Value = 0;
            _unidadesNuevas.Enabled = false;
            foreach (int indice in indices)
                _unidades.SetItemChecked(indice, EstaVigente_380_jh((Unidad_380_jh)_unidades.Items[indice]));
            bool reducir = objetivo < _datos.Cantidad_380_jh;
            var candidatas = indices.Where(indice => _unidades.GetItemChecked(indice) == reducir);
            // Al reducir se proponen las unidades agregadas más recientemente.
            if (reducir) candidatas = candidatas.Reverse();
            foreach (int indice in candidatas.Take(Math.Abs(objetivo - _datos.Cantidad_380_jh)))
                _unidades.SetItemChecked(indice, !reducir);
            _estadoRestauracion.Text = string.Format(T("DONATION_QUANTITY_RESTORE_TARGET"), objetivo, seleccion.Fecha_380_jh);
            ActualizarSeleccionHistorial_380_jh();
        }

        private void Guardar_380_jh(object sender, EventArgs e)
        {
            int nuevas = (int)_unidadesNuevas.Value;
            int cantidad = _unidadesFijas + _unidades.CheckedItems.Count + nuevas;
            var afectadas = _unidades.Items.Cast<Unidad_380_jh>()
                .Where((unidad, indice) => _unidades.GetItemChecked(indice) != EstaVigente_380_jh(unidad))
                .Select(unidad => unidad.Id_380_jh).ToArray();
            if (_versionRestaurar != null && (nuevas != 0 || cantidad != _versionRestaurar.Nueva_380_jh))
            {
                MessageBox.Show(this, string.Format(T("DONATION_QUANTITY_RESTORE_MISMATCH"), _versionRestaurar.Nueva_380_jh));
                return;
            }
            if (cantidad < 1 || cantidad > 1000 || string.IsNullOrWhiteSpace(_motivo.Text) || (afectadas.Length == 0 && nuevas == 0))
            {
                MessageBox.Show(this, T("DONATION_QUANTITY_INVALID"));
                return;
            }

            CodigoOperacionPN1_380_jh resultado;
            try
            {
                resultado = _service.AjustarCantidad_380_jh(_idDonacion, _datos.Version_380_jh,
                    cantidad, _motivo.Text, afectadas, _versionRestaurar?.Version_380_jh, nuevas);
            }
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
