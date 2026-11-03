using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Application;
using Domain;
using Services;

namespace UI
{
    internal static class MaestrosCombo_380_jh
    {
        public static string[] Codigos_380_jh(string tipo, bool soloActivos = true)
        {
            return new MaestroPN1ApplicationService_380_jh().Listar_380_jh(tipo, soloActivos)
                .ConvertAll(item => item.Codigo_380_jh).ToArray();
        }

        public static void Cargar_380_jh(ComboBox combo, string tipo)
        {
            try {
                combo.Items.Clear();
                combo.Items.AddRange(new MaestroPN1ApplicationService_380_jh().Listar_380_jh(tipo).ToArray());
                combo.SelectedIndex = -1;
            } catch (Exception) {
                combo.Items.Clear();
                combo.Items.Add(LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("MASTERS_LOAD_ERROR"));
                combo.SelectedIndex = 0;
                combo.AccessibleDescription = LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("MASTERS_LOAD_ERROR");
                combo.Enabled = false;
            }
        }

        public static string Seleccionado_380_jh(ComboBox combo) => (combo.SelectedItem as MaestroPN1_380_jh)?.Codigo_380_jh;

        public static void Seleccionar_380_jh(ComboBox combo, string codigo)
        {
            combo.SelectedIndex = -1;
            for (int i = 0; i < combo.Items.Count; i++)
            {
                var item = combo.Items[i] as MaestroPN1_380_jh;
                if (item != null && string.Equals(item.Codigo_380_jh, codigo, StringComparison.OrdinalIgnoreCase))
                { combo.SelectedIndex = i; return; }
            }
        }

        public static string[] ConTodos_380_jh(string tipo)
        {
            try {
                var codigos = new List<string>(Codigos_380_jh(tipo, false));
                codigos.Insert(0, string.Empty);
                return codigos.ToArray();
            } catch (Exception) {
                return new[] { string.Empty };
            }
        }
    }
}
