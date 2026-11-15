using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Application;
using Domain;

namespace UI
{
    public class AbastecimientoView_380_jh : LocalizedUserControl_380_jh
    {
        private readonly Func<AbastecimientoFiltro_380_jh, List<FilaAbastecimiento_380_jh>> _consultar_380_jh;
        private readonly Func<string, string> _traducir_380_jh;
        private readonly List<FilaAbastecimiento_380_jh> _filas_380_jh = new List<FilaAbastecimiento_380_jh>();
        private readonly Dictionary<string, SortedSet<string>> _opciones_380_jh = new Dictionary<string, SortedSet<string>>();
        private DateTimePicker _desde_380_jh, _hasta_380_jh;
        private NumericUpDown _horizonte_380_jh;
        private ComboBox _componente_380_jh, _grupo_380_jh, _rh_380_jh;
        private CheckBox _soloAlertas_380_jh;
        private Button _generar_380_jh;
        private DataGridView _grilla_380_jh;
        private Label _stock_380_jh, _vence_380_jh, _deficit_380_jh, _estudio_380_jh, _detalle_380_jh, _estado_380_jh;
        private ComparacionAbastecimientoPanel_380_jh _grafico_380_jh;
        private AbastecimientoFiltro_380_jh _filtroAplicado_380_jh;
        private string _errorConsulta_380_jh;
        private bool _cargando_380_jh;

        public AbastecimientoView_380_jh() : this(new PN2ApplicationService_380_jh()) { }

        public AbastecimientoView_380_jh(PN2ApplicationService_380_jh servicio)
            : this(servicio.ListarAbastecimiento_380_jh, ControlesPN2_380_jh.Traducir_380_jh) { }

        public AbastecimientoView_380_jh(
            Func<AbastecimientoFiltro_380_jh, List<FilaAbastecimiento_380_jh>> consultar,
            Func<string, string> traducir)
        {
            if (consultar == null) throw new ArgumentNullException("consultar");
            if (traducir == null) throw new ArgumentNullException("traducir");
            _consultar_380_jh = consultar;
            _traducir_380_jh = traducir;
            ConstruirInterfaz_380_jh();
            CargarReporte_380_jh();
        }

        private string T_380_jh(string clave) { return _traducir_380_jh(clave); }

        private Label Etiqueta_380_jh(string clave, float tamano = 9, bool negrita = false)
        {
            return new Label { Tag = clave, Text = T_380_jh(clave), Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", tamano, negrita ? FontStyle.Bold : FontStyle.Regular),
                AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft };
        }

        private void ConstruirInterfaz_380_jh()
        {
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F);
            Padding = new Padding(18, 12, 18, 12);
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7 };
            foreach (float alto in new[] { 36F, 34F, 78F, 88F }) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, alto));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            Controls.Add(layout);
            Label titulo = Etiqueta_380_jh("SUPPLY_TITLE", 20, true);
            titulo.ForeColor = Color.FromArgb(23, 50, 77);
            layout.Controls.Add(titulo, 0, 0);
            layout.Controls.Add(Etiqueta_380_jh("SUPPLY_DESCRIPTION"), 0, 1);

            FlowLayoutPanel filtros = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true, AutoScroll = true, Padding = new Padding(0, 3, 0, 0) };
            _desde_380_jh = new DateTimePicker { Width = 113, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-29), MaxDate = DateTime.Today };
            _hasta_380_jh = new DateTimePicker { Width = 113, Format = DateTimePickerFormat.Short, Value = DateTime.Today, MaxDate = DateTime.Today };
            _horizonte_380_jh = new NumericUpDown { Width = 58, Minimum = 1, Maximum = 365, Value = 7 };
            _componente_380_jh = Combo_380_jh(155);
            _grupo_380_jh = Combo_380_jh(73);
            _rh_380_jh = Combo_380_jh(67);
            AgregarFiltro_380_jh(filtros, "SUPPLY_FROM", _desde_380_jh);
            AgregarFiltro_380_jh(filtros, "SUPPLY_TO", _hasta_380_jh);
            AgregarFiltro_380_jh(filtros, "SUPPLY_HORIZON", _horizonte_380_jh);
            AgregarFiltro_380_jh(filtros, "SUPPLY_COMPONENT", _componente_380_jh);
            AgregarFiltro_380_jh(filtros, "SUPPLY_GROUP", _grupo_380_jh);
            AgregarFiltro_380_jh(filtros, "SUPPLY_RH", _rh_380_jh);
            _generar_380_jh = new Button { Tag = "SUPPLY_GENERATE", Text = T_380_jh("SUPPLY_GENERATE"), AutoSize = true,
                BackColor = Color.FromArgb(33, 82, 125), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Padding = new Padding(8, 2, 8, 2) };
            _generar_380_jh.Click += delegate { CargarReporte_380_jh(); };
            filtros.Controls.Add(_generar_380_jh);
            _soloAlertas_380_jh = new CheckBox { Tag = "SUPPLY_ALERTS_ONLY", Text = T_380_jh("SUPPLY_ALERTS_ONLY"), AutoSize = true, Padding = new Padding(7, 5, 0, 0) };
            _soloAlertas_380_jh.CheckedChanged += delegate { PintarReporte_380_jh(); };
            filtros.Controls.Add(_soloAlertas_380_jh);
            layout.Controls.Add(filtros, 0, 2);

            TableLayoutPanel tarjetas = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1 };
            for (int n = 0; n < 4; n++) tarjetas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            _stock_380_jh = Tarjeta_380_jh(tarjetas, 0, "SUPPLY_KPI_STOCK", Color.FromArgb(33, 82, 125));
            _vence_380_jh = Tarjeta_380_jh(tarjetas, 1, "SUPPLY_KPI_EXPIRY", Color.FromArgb(157, 100, 22));
            _deficit_380_jh = Tarjeta_380_jh(tarjetas, 2, "SUPPLY_KPI_DEFICIT", Color.FromArgb(153, 49, 45));
            _estudio_380_jh = Tarjeta_380_jh(tarjetas, 3, "SUPPLY_KPI_STUDY", Color.FromArgb(94, 82, 131));
            layout.Controls.Add(tarjetas, 0, 3);

            TabControl pestanas = new TabControl { Dock = DockStyle.Fill };
            TabPage detalle = new TabPage { Tag = "SUPPLY_TAB_DETAIL", Text = T_380_jh("SUPPLY_TAB_DETAIL"), BackColor = Color.White, Padding = new Padding(4) };
            TabPage comparacion = new TabPage { Tag = "SUPPLY_TAB_CHART", Text = T_380_jh("SUPPLY_TAB_CHART"), BackColor = Color.White };
            pestanas.TabPages.Add(detalle); pestanas.TabPages.Add(comparacion);
            _grilla_380_jh = ControlesPN2_380_jh.CrearGrilla_380_jh();
            _grilla_380_jh.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _grilla_380_jh.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            _grilla_380_jh.ColumnHeadersHeight = 42;
            _grilla_380_jh.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            _grilla_380_jh.RowTemplate.Height = 32;
            _grilla_380_jh.GridColor = Color.FromArgb(225, 231, 237);
            _grilla_380_jh.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(237, 243, 248);
            _grilla_380_jh.EnableHeadersVisualStyles = false;
            Columna_380_jh("componente", "SUPPLY_COMPONENT", 150);
            Columna_380_jh("grupo", "SUPPLY_GROUP", 64);
            Columna_380_jh("rh", "SUPPLY_RH", 48);
            Columna_380_jh("stock", "SUPPLY_COL_STOCK", 75, "N0");
            Columna_380_jh("vence", "SUPPLY_COL_EXPIRY", 75, "N0");
            Columna_380_jh("demanda", "SUPPLY_COL_DEMAND", 85, "N0");
            Columna_380_jh("promedio", "SUPPLY_COL_RATE", 88, "N2");
            Columna_380_jh("pendiente", "SUPPLY_COL_PENDING", 86, "N0");
            Columna_380_jh("cobertura", "SUPPLY_COL_COVERAGE", 85, "N1");
            Columna_380_jh("deficit", "SUPPLY_COL_DEFICIT", 70, "N0");
            Columna_380_jh("prioridad", "SUPPLY_COL_PRIORITY", 178);
            _grilla_380_jh.SelectionChanged += delegate { PintarDetalle_380_jh(); };
            detalle.Controls.Add(_grilla_380_jh);
            _grafico_380_jh = new ComparacionAbastecimientoPanel_380_jh(T_380_jh) { Dock = DockStyle.Fill };
            comparacion.Controls.Add(_grafico_380_jh);
            layout.Controls.Add(pestanas, 0, 4);
            _detalle_380_jh = new Label { Dock = DockStyle.Fill, Padding = new Padding(10, 7, 10, 2), BackColor = Color.FromArgb(242, 246, 250), AutoEllipsis = true };
            layout.Controls.Add(_detalle_380_jh, 0, 5);
            _estado_380_jh = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.DimGray, AutoEllipsis = true };
            layout.Controls.Add(_estado_380_jh, 0, 6);
            CargarOpciones_380_jh(_componente_380_jh, "componente", null);
            CargarOpciones_380_jh(_grupo_380_jh, "grupo", null);
            CargarOpciones_380_jh(_rh_380_jh, "rh", null);
        }

        private ComboBox Combo_380_jh(int ancho) { return new ComboBox { Width = ancho, DropDownStyle = ComboBoxStyle.DropDownList }; }

        private void AgregarFiltro_380_jh(FlowLayoutPanel panel, string clave, Control control)
        {
            Label etiqueta = Etiqueta_380_jh(clave); etiqueta.Dock = DockStyle.None; etiqueta.AutoSize = true; etiqueta.Padding = new Padding(0, 5, 2, 0);
            panel.Controls.Add(etiqueta); panel.Controls.Add(control);
        }

        private Label Tarjeta_380_jh(TableLayoutPanel padre, int columna, string clave, Color color)
        {
            Panel panel = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 2, 9, 10), Padding = new Padding(10, 4, 8, 4), BackColor = Color.FromArgb(242, 246, 250) };
            TableLayoutPanel contenido = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = Padding.Empty };
            contenido.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            contenido.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Label titulo = Etiqueta_380_jh(clave);
            Label valor = new Label { Dock = DockStyle.Fill, Margin = Padding.Empty, Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = color, Text = "0", TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
            contenido.Controls.Add(titulo, 0, 0); contenido.Controls.Add(valor, 0, 1); panel.Controls.Add(contenido);
            padre.Controls.Add(panel, columna, 0); return valor;
        }

        private void Columna_380_jh(string nombre, string clave, int ancho, string formato = null)
        {
            int minimo = nombre == "componente" ? 120 : nombre == "prioridad" ? 150 : nombre == "rh" ? 36 : nombre == "grupo" ? 42
                : nombre == "promedio" ? 74 : nombre == "demanda" || nombre == "pendiente" || nombre == "cobertura" ? 70 : 60;
            DataGridViewTextBoxColumn columna = new DataGridViewTextBoxColumn { Name = nombre, Tag = clave, HeaderText = T_380_jh(clave),
                FillWeight = ancho, MinimumWidth = minimo, Width = ancho, SortMode = DataGridViewColumnSortMode.Automatic };
            if (nombre == "componente" || nombre == "prioridad") columna.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            if (formato != null) { columna.DefaultCellStyle.Format = formato; columna.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight; }
            columna.DefaultCellStyle.NullValue = T_380_jh("SUPPLY_NA"); _grilla_380_jh.Columns.Add(columna);
        }

        private void CargarOpciones_380_jh(ComboBox combo, string nombre, IEnumerable<string> valores)
        {
            string seleccion = ControlesPN2_380_jh.EstadoSeleccionado_380_jh(combo);
            if (!_opciones_380_jh.ContainsKey(nombre)) _opciones_380_jh[nombre] = new SortedSet<string>(StringComparer.CurrentCultureIgnoreCase);
            if (valores != null) foreach (string valor in valores.Where(x => !string.IsNullOrWhiteSpace(x))) _opciones_380_jh[nombre].Add(valor);
            List<KeyValuePair<string, string>> opciones = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>(string.Empty, T_380_jh("FILTER_ALL")) };
            opciones.AddRange(_opciones_380_jh[nombre].Select(x => new KeyValuePair<string, string>(x, x)));
            combo.DisplayMember = "Value"; combo.ValueMember = "Key"; combo.DataSource = opciones; combo.SelectedValue = seleccion;
            if (combo.SelectedIndex < 0) combo.SelectedIndex = 0;
        }

        private void CargarReporte_380_jh()
        {
            if (_cargando_380_jh) return;
            if (_desde_380_jh.Value.Date > _hasta_380_jh.Value.Date)
            {
                _filas_380_jh.Clear(); _filtroAplicado_380_jh = null;
                _errorConsulta_380_jh = "SUPPLY_INVALID_PERIOD"; PintarReporte_380_jh(); return;
            }
            _cargando_380_jh = true; _generar_380_jh.Enabled = false; UseWaitCursor = true;
            try
            {
                AbastecimientoFiltro_380_jh filtro = new AbastecimientoFiltro_380_jh { Desde_380_jh = _desde_380_jh.Value.Date,
                    Hasta_380_jh = _hasta_380_jh.Value.Date, HorizonteVencimientoDias_380_jh = (int)_horizonte_380_jh.Value,
                    TipoComponente_380_jh = ControlesPN2_380_jh.EstadoSeleccionado_380_jh(_componente_380_jh),
                    GrupoSanguineo_380_jh = ControlesPN2_380_jh.EstadoSeleccionado_380_jh(_grupo_380_jh),
                    FactorRh_380_jh = ControlesPN2_380_jh.EstadoSeleccionado_380_jh(_rh_380_jh) };
                List<FilaAbastecimiento_380_jh> resultado = _consultar_380_jh(filtro);
                _filas_380_jh.Clear(); _filas_380_jh.AddRange(resultado); _filtroAplicado_380_jh = filtro; _errorConsulta_380_jh = null;
                CargarOpciones_380_jh(_componente_380_jh, "componente", _filas_380_jh.Select(x => x.TipoComponente_380_jh));
                CargarOpciones_380_jh(_grupo_380_jh, "grupo", _filas_380_jh.Select(x => x.GrupoSanguineo_380_jh));
                CargarOpciones_380_jh(_rh_380_jh, "rh", _filas_380_jh.Select(x => x.FactorRh_380_jh));
                PintarReporte_380_jh();
            }
            catch
            {
                _filas_380_jh.Clear(); _filtroAplicado_380_jh = null;
                _errorConsulta_380_jh = "SUPPLY_QUERY_ERROR"; PintarReporte_380_jh();
            }
            finally { _cargando_380_jh = false; _generar_380_jh.Enabled = true; UseWaitCursor = false; }
        }

        protected override void ApplyTranslations_380_jh()
        {
            if (_grilla_380_jh == null) return;
            foreach (Control control in Controls) TraducirControles_380_jh(control);
            foreach (DataGridViewColumn columna in _grilla_380_jh.Columns) { columna.HeaderText = T_380_jh((string)columna.Tag); columna.DefaultCellStyle.NullValue = T_380_jh("SUPPLY_NA"); }
            CargarOpciones_380_jh(_componente_380_jh, "componente", null); CargarOpciones_380_jh(_grupo_380_jh, "grupo", null); CargarOpciones_380_jh(_rh_380_jh, "rh", null);
            PintarReporte_380_jh();
        }

        private void TraducirControles_380_jh(Control control)
        {
            if (control.Tag is string) control.Text = T_380_jh((string)control.Tag);
            foreach (Control hijo in control.Controls) TraducirControles_380_jh(hijo);
        }

        private void PintarReporte_380_jh()
        {
            if (_grilla_380_jh == null) return;
            List<FilaAbastecimiento_380_jh> visibles = _filas_380_jh.Where(x => !_soloAlertas_380_jh.Checked || x.Prioridad_380_jh != "SIN_ALERTA")
                .OrderBy(x => OrdenPrioridad_380_jh(x.Prioridad_380_jh)).ThenBy(x => x.TipoComponente_380_jh).ThenBy(x => x.GrupoSanguineo_380_jh).ThenBy(x => x.FactorRh_380_jh).ToList();
            _grilla_380_jh.Rows.Clear();
            foreach (FilaAbastecimiento_380_jh fila in visibles)
            {
                int n = _grilla_380_jh.Rows.Add(fila.TipoComponente_380_jh, fila.GrupoSanguineo_380_jh, fila.FactorRh_380_jh,
                    fila.StockLiberadoVigente_380_jh, fila.StockPorVencer_380_jh, fila.DemandaPeriodo_380_jh, fila.DemandaDiariaPromedio_380_jh,
                    fila.PendienteVigente_380_jh, fila.CoberturaDias_380_jh, fila.DeficitUnidades_380_jh, T_380_jh("SUPPLY_PRIORITY_" + fila.Prioridad_380_jh));
                DataGridViewRow row = _grilla_380_jh.Rows[n]; row.Tag = fila;
                row.DefaultCellStyle.BackColor = fila.Prioridad_380_jh.Contains("REPOSICION") ? Color.FromArgb(255, 239, 236)
                    : fila.Prioridad_380_jh == "VENCIMIENTO" ? Color.FromArgb(255, 247, 224)
                    : !fila.CohorteValidada_380_jh ? Color.FromArgb(243, 239, 252) : Color.White;
                row.Cells["cobertura"].ToolTipText = T_380_jh(fila.CohorteValidada_380_jh ? "SUPPLY_COVERAGE_HELP" : "SUPPLY_STUDY_HELP");
            }
            _stock_380_jh.Text = visibles.Sum(x => x.StockLiberadoVigente_380_jh ?? 0).ToString("N0");
            _vence_380_jh.Text = visibles.Sum(x => x.StockPorVencer_380_jh ?? 0).ToString("N0");
            _deficit_380_jh.Text = visibles.Sum(x => x.DeficitUnidades_380_jh ?? 0).ToString("N0");
            _estudio_380_jh.Text = visibles.Where(x => !x.CohorteValidada_380_jh).Sum(x => x.PendienteVigente_380_jh).ToString("N0");
            _grafico_380_jh.Mostrar_380_jh(visibles);
            _estado_380_jh.ForeColor = _errorConsulta_380_jh == null ? Color.DimGray : Color.Firebrick;
            _estado_380_jh.Text = _errorConsulta_380_jh != null ? T_380_jh(_errorConsulta_380_jh) : _filtroAplicado_380_jh == null ? string.Empty : string.Format(T_380_jh("SUPPLY_STATUS"), visibles.Count,
                _filas_380_jh.Count == 0 ? DateTime.Today : _filas_380_jh[0].FechaCorte_380_jh,
                _filtroAplicado_380_jh.Desde_380_jh, _filtroAplicado_380_jh.Hasta_380_jh, _filtroAplicado_380_jh.HorizonteVencimientoDias_380_jh);
            PintarDetalle_380_jh();
        }

        private static int OrdenPrioridad_380_jh(string prioridad)
        {
            switch (prioridad) { case "REPOSICION_Y_VENCIMIENTO": return 0; case "REPOSICION": return 1; case "VENCIMIENTO": return 2; case "VALIDAR_ESTUDIO": return 3; default: return 4; }
        }

        private void PintarDetalle_380_jh()
        {
            if (_errorConsulta_380_jh != null) { _detalle_380_jh.Text = T_380_jh(_errorConsulta_380_jh); return; }
            FilaAbastecimiento_380_jh fila = ControlesPN2_380_jh.Seleccionado_380_jh<FilaAbastecimiento_380_jh>(_grilla_380_jh);
            if (fila == null) { _detalle_380_jh.Text = T_380_jh(_grilla_380_jh.Rows.Count == 0 ? "SUPPLY_EMPTY" : "SUPPLY_SELECT_ROW"); return; }
            if (!fila.CohorteValidada_380_jh) { _detalle_380_jh.Text = string.Format(T_380_jh("SUPPLY_STUDY_DETAIL"), fila.PendienteVigente_380_jh); return; }
            string recomendacion = fila.DeficitUnidades_380_jh > 0 ? string.Format(T_380_jh("SUPPLY_REPLENISH_DETAIL"), fila.DeficitUnidades_380_jh) : T_380_jh("SUPPLY_BALANCED_DETAIL");
            if (fila.StockPorVencer_380_jh > 0) recomendacion += " " + string.Format(T_380_jh("SUPPLY_EXPIRY_DETAIL"), fila.StockPorVencer_380_jh);
            _detalle_380_jh.Text = recomendacion + " " + T_380_jh("SUPPLY_EXACT_COHORT_HELP");
        }
    }

    internal class ComparacionAbastecimientoPanel_380_jh : Panel
    {
        private readonly Func<string, string> _traducir_380_jh;
        private List<FilaAbastecimiento_380_jh> _filas_380_jh = new List<FilaAbastecimiento_380_jh>();
        internal ComparacionAbastecimientoPanel_380_jh(Func<string, string> traducir) { _traducir_380_jh = traducir; DoubleBuffered = true; BackColor = Color.White; }
        internal void Mostrar_380_jh(List<FilaAbastecimiento_380_jh> filas) { _filas_380_jh = filas; Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var grupos = _filas_380_jh.Where(x => x.CohorteValidada_380_jh).GroupBy(x => x.TipoComponente_380_jh)
                .Select(x => new { Nombre = x.Key, Stock = x.Sum(f => f.StockLiberadoVigente_380_jh ?? 0), Saldo = x.Sum(f => f.PendienteVigente_380_jh) })
                .OrderByDescending(x => x.Saldo).ThenBy(x => x.Nombre).Take(6).ToList();
            using (Font font = new Font("Segoe UI", 10))
            using (Brush texto = new SolidBrush(Color.FromArgb(40, 55, 70)))
            using (Brush stock = new SolidBrush(Color.FromArgb(52, 108, 156)))
            using (Brush saldo = new SolidBrush(Color.FromArgb(207, 147, 57)))
            {
                e.Graphics.FillRectangle(stock, 18, 17, 14, 12); e.Graphics.DrawString(_traducir_380_jh("SUPPLY_CHART_STOCK"), font, texto, 38, 12);
                e.Graphics.FillRectangle(saldo, 250, 17, 14, 12); e.Graphics.DrawString(_traducir_380_jh("SUPPLY_CHART_PENDING"), font, texto, 270, 12);
                if (grupos.Count == 0) { e.Graphics.DrawString(_traducir_380_jh("SUPPLY_EMPTY"), font, texto, 18, 70); return; }
                long maximo = Math.Max(1, grupos.Max(x => Math.Max(x.Stock, x.Saldo)));
                float alto = Math.Max(31, Math.Min(66, (ClientSize.Height - 75F) / grupos.Count));
                float ancho = Math.Max(40, ClientSize.Width - 270F);
                for (int n = 0; n < grupos.Count; n++)
                {
                    float y = 52 + n * alto;
                    e.Graphics.DrawString(grupos[n].Nombre, font, texto, new RectangleF(18, y + 5, 185, alto - 2));
                    float wStock = ancho * grupos[n].Stock / maximo, wSaldo = ancho * grupos[n].Saldo / maximo;
                    e.Graphics.FillRectangle(stock, 210, y, wStock, 13); e.Graphics.FillRectangle(saldo, 210, y + 17, wSaldo, 13);
                    e.Graphics.DrawString(grupos[n].Stock.ToString("N0"), font, texto, 216 + wStock, y - 3);
                    e.Graphics.DrawString(grupos[n].Saldo.ToString("N0"), font, texto, 216 + wSaldo, y + 14);
                }
                e.Graphics.DrawString(_traducir_380_jh("SUPPLY_CHART_HELP"), font, texto, 18, ClientSize.Height - 24);
            }
        }
    }
}
