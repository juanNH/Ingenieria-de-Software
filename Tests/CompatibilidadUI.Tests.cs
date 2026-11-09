using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Application;
using DAL;
using Domain;
using Services;
using UI;

internal static class CompatibilidadUITests
{
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length < 2 || !Regex.IsMatch(args[1], "^HemovidaGest_Compat_Test_[a-f0-9]{32}$"))
                throw new InvalidOperationException("Only an isolated compatibility test database is allowed.");
            string conexion = new SqlConnectionStringBuilder { DataSource = args[0], InitialCatalog = args[1], IntegratedSecurity = true }.ConnectionString;
            typeof(DatabaseContext_380_jh).GetField("ConnectionString_380_jh", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, conexion);
            var usuario = new Usuario_380_jh { Id_380_jh = 1, Username_380_jh = "admin", Estado_380_jh = "ACTIVO" };
            usuario.ComponentesPermiso_380_jh = new PermisoDataMapper_380_jh().ListarAsignadosPorUsuario_380_jh(1);
            Sesion_380_jh.ObtenerInstancia_380_jh().IniciarSesion_380_jh(usuario);
            LanguageManager_380_jh.Instance_380_jh.Initialize_380_jh(usuario);
            Check(new IntegridadApplicationService_380_jh().Verificar_380_jh().EsValida_380_jh, "Integridad válida en Application");
            var servicio = new MaestroPN1ApplicationService_380_jh();
            Check(servicio.ListarCompatibilidades_380_jh().Count == 76, "Application y DAL leen las reglas");
            Check(servicio.GuardarCompatibilidad_380_jh(new ReglaCompatibilidad_380_jh()) == "DATOS_INVALIDOS", "Application rechaza reglas incompletas");
            var regla = servicio.ListarCompatibilidades_380_jh().Single(r => r.TipoComponente_380_jh == "COMP_TEST");
            Check(servicio.GuardarCompatibilidad_380_jh(regla) == "OK", "Application guarda sin limitar códigos nuevos");
            Check(new AuditoriaApplicationService_380_jh().ListarNegocio_380_jh("CompatibilidadTransfusional").Count > 0, "Historial accesible en auditoría");
            using (var formulario = new Form { ClientSize = new Size(1100, 740), StartPosition = FormStartPosition.Manual,
                Location = new Point(-30000, -30000), ShowInTaskbar = false, Font = new Font("Segoe UI", 9) })
            using (var maestros = new MaestrosView_380_jh { Dock = DockStyle.Fill })
            {
                formulario.Controls.Add(maestros); formulario.Show();
                var tabs = Controles(maestros).OfType<TabControl>().Single();
                var tab = tabs.TabPages.Cast<TabPage>().Single(t => (string)t.Tag == "COMPAT_TITLE");
                tabs.SelectedTab = tab; System.Windows.Forms.Application.DoEvents();
                var vista = Controles(tab).OfType<CompatibilidadesView_380_jh>().Single();
                var grilla = Controles(vista).OfType<DataGridView>().Single();
                Check(grilla.Rows.Count == 76, "La pestaña muestra reglas iniciales y nuevas");
                var combos = Controles(vista).OfType<ComboBox>().ToList();
                Check(combos.Single(c => c.Name == "grupoUnidad").Items.Cast<MaestroPN1_380_jh>().Any(m => m.Codigo_380_jh == "X"), "Selector incluye grupos nuevos");
                Check(combos.Single(c => c.Name == "rhReceptor").Items.Cast<MaestroPN1_380_jh>().Any(m => m.Codigo_380_jh == "~"), "Selector incluye factores nuevos");
                var buscar = Controles(vista).OfType<TextBox>().Single(); buscar.Text = "COMP_TEST";
                Check(grilla.Rows.Count == 1, "Filtro de reglas");
                grilla.Rows[0].Selected = true;
                Check(combos.All(c => !c.Enabled), "Una regla existente conserva su combinación");
                Controles(vista).OfType<Button>().Single(b => b.Name == "nuevaRegla").PerformClick();
                Check(combos.All(c => c.Enabled && c.SelectedIndex == -1), "Nuevo habilita las cinco opciones");
                buscar.Clear();
                var idioma = LanguageManager_380_jh.Instance_380_jh.CurrentLanguage_380_jh;
                var ingles = LanguageManager_380_jh.Instance_380_jh.ListarIdiomasActivos_380_jh().Single(i => i.Codigo_380_jh == "en-US");
                LanguageManager_380_jh.Instance_380_jh.ChangeLanguage_380_jh(ingles, null);
                Check(tab.Text == "Compatibility" && grilla.Columns[1].HeaderText == "Unit blood group", "Traducciones inglesas de pestaña y columnas");
                LanguageManager_380_jh.Instance_380_jh.ChangeLanguage_380_jh(idioma, null);
                if (args.Length > 2 && !string.IsNullOrWhiteSpace(args[2]))
                    using (var bitmap = new Bitmap(formulario.ClientSize.Width, formulario.ClientSize.Height))
                    { formulario.DrawToBitmap(bitmap, formulario.ClientRectangle); bitmap.Save(args[2]); }
                formulario.Close();
            }
            Sesion_380_jh.ObtenerInstancia_380_jh().IniciarSesion_380_jh(new Usuario_380_jh { Id_380_jh = -1, Estado_380_jh = "ACTIVO" });
            Check(servicio.GuardarCompatibilidad_380_jh(regla) == "NO_AUTORIZADO", "SQL rechaza guardar reglas sin permiso administrativo");
            bool denegado = false;
            try { servicio.ListarCompatibilidades_380_jh(); } catch (UnauthorizedAccessException) { denegado = true; }
            Check(denegado, "SQL restringe la consulta administrativa");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
    private static IEnumerable<Control> Controles(Control raiz)
    {
        foreach (Control hijo in raiz.Controls)
        { yield return hijo; foreach (Control descendiente in Controles(hijo)) yield return descendiente; }
    }
    private static void Check(bool condicion, string mensaje)
    { if (!condicion) throw new InvalidOperationException(mensaje); Console.WriteLine("OK " + mensaje); }
}
