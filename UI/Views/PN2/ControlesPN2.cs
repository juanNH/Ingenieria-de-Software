using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Domain;
using Services;

namespace UI
{
    internal static class ControlesPN2_380_jh
    {
        internal static string Traducir_380_jh(string clave)
        {
            return LanguageManager_380_jh.Instance_380_jh.Translate_380_jh(clave);
        }

        internal static TableLayoutPanel CrearLayout_380_jh(Control vista, string titulo, string descripcion, int altoFiltro, int altoFormulario)
        {
            vista.BackColor = Color.White;
            vista.Padding = new Padding(24);
            vista.Font = new Font("Segoe UI", 9F);
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = altoFormulario > 0 ? 5 : 4 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, altoFiltro));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            if (altoFormulario > 0) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, altoFormulario));
            layout.Controls.Add(CrearLabel_380_jh(titulo, 18, true), 0, 0);
            layout.Controls.Add(CrearLabel_380_jh(descripcion, 9, false), 0, 1);
            vista.Controls.Add(layout);
            return layout;
        }

        internal static Label CrearLabel_380_jh(string clave, float tamano = 9, bool negrita = false)
        {
            return new Label { Tag = clave, Text = Traducir_380_jh(clave), Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", tamano, negrita ? FontStyle.Bold : FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
        }

        internal static Button CrearBoton_380_jh(string clave)
        {
            return new Button { Tag = clave, Text = Traducir_380_jh(clave), AutoSize = true, Padding = new Padding(8, 2, 8, 2) };
        }

        internal static FlowLayoutPanel CrearFiltros_380_jh()
        {
            return new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true, AutoScroll = true };
        }

        internal static void AgregarFiltro_380_jh(FlowLayoutPanel filtro, string clave, Control control)
        {
            Label label = CrearLabel_380_jh(clave);
            label.Dock = DockStyle.None;
            label.AutoSize = true;
            label.Padding = new Padding(0, 6, 4, 0);
            filtro.Controls.Add(label);
            filtro.Controls.Add(control);
        }

        internal static DataGridView CrearGrilla_380_jh()
        {
            return new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
                AllowUserToDeleteRows = false, AutoGenerateColumns = false, MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false, BackgroundColor = Color.White };
        }

        internal static void AgregarColumna_380_jh(DataGridView grilla, string nombre, string clave, float peso = 100)
        {
            grilla.Columns.Add(new DataGridViewTextBoxColumn { Name = nombre, Tag = clave, HeaderText = Traducir_380_jh(clave), FillWeight = peso, MinimumWidth = 65 });
        }

        internal static TableLayoutPanel CrearFormulario_380_jh(Control padre, string clave, int filas)
        {
            GroupBox detalle = new GroupBox { Tag = clave, Text = Traducir_380_jh(clave), Dock = DockStyle.Fill, Padding = new Padding(10) };
            padre.Controls.Add(detalle);
            TableLayoutPanel formulario = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = filas, AutoScroll = true };
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135));
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135));
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            for (int fila = 0; fila < filas; fila++) formulario.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            detalle.Controls.Add(formulario);
            return formulario;
        }

        internal static void AgregarCampo_380_jh(TableLayoutPanel formulario, string clave, Control control, int columna, int fila)
        {
            formulario.Controls.Add(CrearLabel_380_jh(clave), columna, fila);
            control.Dock = DockStyle.Fill;
            formulario.Controls.Add(control, columna + 1, fila);
        }

        internal static T Seleccionado_380_jh<T>(DataGridView grilla) where T : class
        {
            return !grilla.Enabled || grilla.SelectedRows.Count == 0 ? null : grilla.SelectedRows[0].Tag as T;
        }

        internal static string TraducirEstado_380_jh(string estado)
        {
            if (string.IsNullOrWhiteSpace(estado)) return string.Empty;
            string clave = "PN2_STATE_" + estado.ToUpperInvariant().Replace(" ", "_");
            string texto = Traducir_380_jh(clave);
            return texto == clave ? estado : texto;
        }

        internal static void CargarEstados_380_jh(ComboBox combo, params string[] estados)
        {
            string seleccionado = EstadoSeleccionado_380_jh(combo);
            List<KeyValuePair<string, string>> opciones = new List<KeyValuePair<string, string>>();
            foreach (string estado in estados)
                opciones.Add(new KeyValuePair<string, string>(estado ?? string.Empty, string.IsNullOrEmpty(estado) ? Traducir_380_jh("FILTER_ALL") : TraducirEstado_380_jh(estado)));
            combo.DisplayMember = "Value";
            combo.ValueMember = "Key";
            combo.DataSource = opciones;
            combo.SelectedValue = seleccionado ?? string.Empty;
            if (combo.SelectedIndex < 0 && opciones.Count > 0) combo.SelectedIndex = 0;
        }

        internal static string EstadoSeleccionado_380_jh(ComboBox combo)
        {
            return combo.SelectedItem is KeyValuePair<string, string>
                ? ((KeyValuePair<string, string>)combo.SelectedItem).Key : string.Empty;
        }

        internal static void MostrarErrorConsulta_380_jh(IWin32Window ventana, params DataGridView[] grillas)
        {
            foreach (DataGridView grilla in grillas)
            {
                grilla.Enabled = false;
                grilla.ClearSelection();
            }
            MessageBox.Show(ventana, Traducir_380_jh("PN2_QUERY_ERROR"), Traducir_380_jh("MENU_BLOOD_BANK"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        internal static void MostrarResultado_380_jh(IWin32Window ventana, CodigoOperacionPN2_380_jh resultado)
        {
            string clave;
            switch (resultado)
            {
                case CodigoOperacionPN2_380_jh.Ok_380_jh: clave = "PN2_SAVED"; break;
                case CodigoOperacionPN2_380_jh.DatosInvalidos_380_jh: clave = "PN2_INVALID"; break;
                case CodigoOperacionPN2_380_jh.NoAutorizado_380_jh: clave = "OPERATION_NOT_AUTHORIZED"; break;
                case CodigoOperacionPN2_380_jh.NoEncontrado_380_jh: clave = "PN2_NOT_FOUND"; break;
                case CodigoOperacionPN2_380_jh.ConflictoEstado_380_jh: clave = "PN2_CONFLICT"; break;
                case CodigoOperacionPN2_380_jh.SinDisponibilidad_380_jh: clave = "PN2_UNAVAILABLE"; break;
                case CodigoOperacionPN2_380_jh.CantidadCompleta_380_jh: clave = "PN2_QUANTITY_COMPLETE"; break;
                case CodigoOperacionPN2_380_jh.IntegridadInvalida_380_jh: clave = "INTEGRITY_BLOCKED"; break;
                default: clave = "PN2_SAVE_ERROR"; break;
            }
            MessageBox.Show(ventana, Traducir_380_jh(clave), Traducir_380_jh("MENU_BLOOD_BANK"), MessageBoxButtons.OK,
                resultado == CodigoOperacionPN2_380_jh.Ok_380_jh ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        internal static bool Confirmar_380_jh(IWin32Window ventana, string clave, string codigo)
        {
            return MessageBox.Show(ventana, string.Format(Traducir_380_jh(clave), codigo), Traducir_380_jh("MENU_BLOOD_BANK"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }
    }
}
