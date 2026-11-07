using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Application;
using Domain;
using Services;

namespace UI
{
    public class CompatibilidadesView_380_jh : LocalizedUserControl_380_jh
    {
        private readonly MaestroPN1ApplicationService_380_jh _service = new MaestroPN1ApplicationService_380_jh();
        private readonly ComboBox _componente = Combo_380_jh("componente");
        private readonly ComboBox _grupoUnidad = Combo_380_jh("grupoUnidad");
        private readonly ComboBox _rhUnidad = Combo_380_jh("rhUnidad");
        private readonly ComboBox _grupoReceptor = Combo_380_jh("grupoReceptor");
        private readonly ComboBox _rhReceptor = Combo_380_jh("rhReceptor");
        private readonly CheckBox _activo = new CheckBox { AutoSize = true, Checked = true, Tag = "MASTERS_ACTIVE" };
        private readonly TextBox _buscar = new TextBox { Dock = DockStyle.Fill, MaxLength = 150 };
        private readonly Label _estado = new Label { Dock = DockStyle.Fill, AutoEllipsis = true };
        private readonly Button _guardar = new Button { AutoSize = true, Tag = "MASTERS_SAVE", Name = "guardarRegla" };
        private readonly Button _baja = new Button { AutoSize = true, Tag = "MASTERS_DEACTIVATE", Name = "desactivarRegla" };
        private readonly DataGridView _grilla = new DataGridView {
            Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            MultiSelect = false, RowHeadersVisible = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToResizeRows = false, ColumnHeadersHeight = 36, Name = "reglasCompatibilidad"
        };
        private List<MaestroPN1_380_jh> _componentes = new List<MaestroPN1_380_jh>();
        private List<MaestroPN1_380_jh> _grupos = new List<MaestroPN1_380_jh>();
        private List<MaestroPN1_380_jh> _factores = new List<MaestroPN1_380_jh>();
        private List<ReglaCompatibilidad_380_jh> _reglas = new List<ReglaCompatibilidad_380_jh>();
        private int _id;
        private bool _cargada;
        private bool _disponible;

        public CompatibilidadesView_380_jh()
        {
            BackColor = Color.White;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 190));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);
            var formulario = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 5 };
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135));
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135));
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            for (int i = 0; i < 5; i++) formulario.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
            Campo_380_jh(formulario, "MASTERS_COMPONENTS", _componente, 0, 0);
            formulario.SetColumnSpan(_componente, 3);
            Campo_380_jh(formulario, "COMPAT_GROUP_UNIT", _grupoUnidad, 0, 1);
            Campo_380_jh(formulario, "COMPAT_RH_UNIT", _rhUnidad, 2, 1);
            Campo_380_jh(formulario, "COMPAT_GROUP_RECIPIENT", _grupoReceptor, 0, 2);
            Campo_380_jh(formulario, "COMPAT_RH_RECIPIENT", _rhReceptor, 2, 2);
            formulario.Controls.Add(_activo, 1, 3);
            var acciones = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            var nuevo = new Button { AutoSize = true, Tag = "BTN_NEW", Name = "nuevaRegla" };
            var actualizar = new Button { AutoSize = true, Tag = "BTN_REFRESH", Name = "actualizarReglas" };
            acciones.Controls.AddRange(new Control[] { nuevo, _guardar, _baja, actualizar });
            formulario.Controls.Add(acciones, 1, 4); formulario.SetColumnSpan(acciones, 3);
            layout.Controls.Add(formulario, 0, 0);
            layout.Controls.Add(new Label { Dock = DockStyle.Fill, Tag = "COMPAT_HELP" }, 0, 1);
            var busqueda = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            busqueda.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135));
            busqueda.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            busqueda.Controls.Add(new Label { AutoSize = true, Tag = "COMPAT_FILTER" }, 0, 0);
            busqueda.Controls.Add(_buscar, 1, 0);
            layout.Controls.Add(busqueda, 0, 2);
            layout.Controls.Add(_estado, 0, 3);
            foreach (var columna in new[] {
                new[] { "componente", "MASTERS_COMPONENTS" }, new[] { "grupoUnidad", "COMPAT_GROUP_UNIT" },
                new[] { "rhUnidad", "COMPAT_RH_UNIT" }, new[] { "grupoReceptor", "COMPAT_GROUP_RECIPIENT" },
                new[] { "rhReceptor", "COMPAT_RH_RECIPIENT" } })
                _grilla.Columns.Add(new DataGridViewTextBoxColumn {
                    Name = columna[0], Tag = columna[1], FillWeight = columna[0] == "componente" ? 240 : 100
                });
            _grilla.Columns.Add(new DataGridViewCheckBoxColumn { Name = "activo", Tag = "MASTERS_ACTIVE", FillWeight = 65 });
            layout.Controls.Add(_grilla, 0, 4);
            nuevo.Click += delegate { Limpiar_380_jh(); };
            actualizar.Click += delegate { Cargar_380_jh(); };
            _guardar.Click += delegate { Guardar_380_jh(false); };
            _baja.Click += delegate { Guardar_380_jh(true); };
            _buscar.TextChanged += delegate { Pintar_380_jh(); };
            _grilla.SelectionChanged += delegate { Seleccionar_380_jh(); };
            VisibleChanged += delegate { if (Visible) Cargar_380_jh(); };
            Load += delegate { if (!_cargada) Cargar_380_jh(); };
            _guardar.Enabled = _baja.Enabled = false;
        }

        private static ComboBox Combo_380_jh(string nombre) => new ComboBox {
            Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Name = nombre, DisplayMember = "CodigoDescripcion_380_jh"
        };
        private static void Campo_380_jh(TableLayoutPanel panel, string clave, Control control, int columna, int fila)
        {
            panel.Controls.Add(new Label { AutoSize = true, Tag = clave }, columna, fila);
            panel.Controls.Add(control, columna + 1, fila);
        }
        private ComboBox[] Combos_380_jh() => new[] { _componente, _grupoUnidad, _rhUnidad, _grupoReceptor, _rhReceptor };
        private void Opciones_380_jh(bool soloActivos)
        {
            var listas = new[] { _componentes, _grupos, _factores, _grupos, _factores };
            var combos = Combos_380_jh();
            for (int i = 0; i < combos.Length; i++)
            {
                combos[i].Items.Clear();
                combos[i].Items.AddRange(listas[i].Where(x => !soloActivos || x.Activo_380_jh).ToArray());
                combos[i].SelectedIndex = -1;
            }
        }
        private void Limpiar_380_jh()
        {
            _id = 0; _activo.Checked = true;
            _grilla.ClearSelection();
            Opciones_380_jh(true);
            foreach (var combo in Combos_380_jh()) combo.Enabled = _disponible;
            _guardar.Enabled = _disponible; _baja.Enabled = false;
        }
        private void Cargar_380_jh()
        {
            _cargada = true;
            try
            {
                _componentes = _service.Listar_380_jh(MaestroPN1ApplicationService_380_jh.Componente_380_jh, false);
                _grupos = _service.Listar_380_jh(MaestroPN1ApplicationService_380_jh.Grupo_380_jh, false);
                _factores = _service.Listar_380_jh(MaestroPN1ApplicationService_380_jh.Rh_380_jh, false);
                _reglas = _service.ListarCompatibilidades_380_jh();
                _disponible = true; _estado.Tag = null; _estado.Text = "";
                Pintar_380_jh(); Limpiar_380_jh();
            }
            catch
            {
                _disponible = false; _reglas.Clear(); _grilla.Rows.Clear(); Limpiar_380_jh();
                _estado.Tag = "COMPAT_LOAD_ERROR"; _estado.Text = T_380_jh("COMPAT_LOAD_ERROR");
            }
        }
        private void Pintar_380_jh()
        {
            _grilla.Rows.Clear();
            string buscar = _buscar.Text.Trim();
            foreach (var regla in _reglas)
            {
                string texto = regla.TipoComponente_380_jh + " " + regla.GrupoUnidad_380_jh + regla.RhUnidad_380_jh + " " + regla.GrupoReceptor_380_jh + regla.RhReceptor_380_jh;
                if (texto.IndexOf(buscar, StringComparison.OrdinalIgnoreCase) < 0) continue;
                int fila = _grilla.Rows.Add(regla.TipoComponente_380_jh, regla.GrupoUnidad_380_jh, regla.RhUnidad_380_jh,
                    regla.GrupoReceptor_380_jh, regla.RhReceptor_380_jh, regla.Activo_380_jh);
                _grilla.Rows[fila].Tag = regla;
            }
            Limpiar_380_jh();
        }
        private void Seleccionar_380_jh()
        {
            if (_grilla.SelectedRows.Count == 0) return;
            var regla = _grilla.SelectedRows[0].Tag as ReglaCompatibilidad_380_jh;
            if (regla == null) return;
            _id = regla.Id_380_jh; _activo.Checked = regla.Activo_380_jh;
            Opciones_380_jh(false);
            string[] valores = { regla.TipoComponente_380_jh, regla.GrupoUnidad_380_jh, regla.RhUnidad_380_jh, regla.GrupoReceptor_380_jh, regla.RhReceptor_380_jh };
            var combos = Combos_380_jh();
            for (int i = 0; i < combos.Length; i++) { MaestrosCombo_380_jh.Seleccionar_380_jh(combos[i], valores[i]); combos[i].Enabled = false; }
            _guardar.Enabled = _baja.Enabled = _disponible;
        }
        private void Guardar_380_jh(bool desactivar)
        {
            if (!_disponible) return;
            if (desactivar && _id == 0) { MessageBox.Show(this, T_380_jh("MASTERS_SELECT")); return; }
            var regla = new ReglaCompatibilidad_380_jh {
                Id_380_jh = _id, TipoComponente_380_jh = MaestrosCombo_380_jh.Seleccionado_380_jh(_componente),
                GrupoUnidad_380_jh = MaestrosCombo_380_jh.Seleccionado_380_jh(_grupoUnidad), RhUnidad_380_jh = MaestrosCombo_380_jh.Seleccionado_380_jh(_rhUnidad),
                GrupoReceptor_380_jh = MaestrosCombo_380_jh.Seleccionado_380_jh(_grupoReceptor), RhReceptor_380_jh = MaestrosCombo_380_jh.Seleccionado_380_jh(_rhReceptor),
                Activo_380_jh = !desactivar && _activo.Checked
            };
            try
            {
                string resultado = _service.GuardarCompatibilidad_380_jh(regla);
                string clave = resultado == "DATOS_INVALIDOS" || resultado == "DUPLICADO" ? "COMPAT_RESULT_" + resultado : "MASTERS_RESULT_" + resultado;
                MessageBox.Show(this, T_380_jh(clave));
                if (resultado == "OK") Cargar_380_jh();
            }
            catch { Cargar_380_jh(); MessageBox.Show(this, T_380_jh("MASTERS_RESULT_ERROR")); }
        }
        private static string T_380_jh(string clave) => LanguageManager_380_jh.Instance_380_jh.Translate_380_jh(clave);
    }
}
