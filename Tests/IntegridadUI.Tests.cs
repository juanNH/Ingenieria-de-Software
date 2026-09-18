using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Application;
using DAL;
using Domain;
using Services;
using UI;

internal static class IntegridadUITests
{
    private static string conexion;
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 2 || !Regex.IsMatch(args[1], "^HemovidaGest_Integridad_Test_[a-f0-9]{32}$"))
                throw new InvalidOperationException("Only an isolated test database is allowed.");
            conexion = new SqlConnectionStringBuilder { DataSource = args[0], InitialCatalog = args[1], IntegratedSecurity = true }.ConnectionString;
            // Test-only seam for the existing static configuration. Never edits App.local.config.
            typeof(DatabaseContext_380_jh).GetField("ConnectionString_380_jh", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, conexion);
            var usuario = new Usuario_380_jh { Id_380_jh = 1, Username_380_jh = "admin", Estado_380_jh = "ACTIVO" };
            usuario.ComponentesPermiso_380_jh = new PermisoDataMapper_380_jh().ListarAsignadosPorUsuario_380_jh(1);
            Sesion_380_jh.ObtenerInstancia_380_jh().IniciarSesion_380_jh(usuario);
            LanguageManager_380_jh.Instance_380_jh.Initialize_380_jh(usuario);
            var servicio = new IntegridadApplicationService_380_jh();
            Check(servicio.Verificar_380_jh().EsValida_380_jh, "Application maps valid SQL report");
            Check(new AuditoriaApplicationService_380_jh().ListarNegocio_380_jh("Donante").Count > 0, "PN1 audit history is accessible to administrator");
            foreach (string idioma in new[] { "es-AR", "en-US" })
            {
                var traducciones = new TranslationService_380_jh();
                var lenguaje = new Idioma_380_jh { Id_380_jh = idioma == "es-AR" ? 1 : 2, Codigo_380_jh = idioma };
                Check(traducciones.Translate_380_jh("INTEGRITY_BLOCKED", lenguaje) != "INTEGRITY_BLOCKED", "Translations " + idioma);
                Check(traducciones.Translate_380_jh("MENU_INTEGRITY", lenguaje) == (idioma == "es-AR" ? "Integridad" : "Integrity"), "Generic integrity label " + idioma);
            }
            foreach (UserControl vista in new UserControl[] { new DonantesView_380_jh(), new DonacionesView_380_jh(), new UnidadesView_380_jh(), new IntegridadView_380_jh(), new AuditoriaCambiosView_380_jh() })
            {
                using (vista)
                {
                    vista.CreateControl();
                    foreach (Size tamano in new[] { new Size(1200, 700), new Size(900, 600), new Size(320, 480), new Size(1200, 700) })
                    {
                        vista.Size = tamano;
                        vista.PerformLayout();
                    }
                    ((LocalizedUserControl_380_jh)vista).OnLanguageChanged_380_jh(LanguageManager_380_jh.Instance_380_jh.CurrentLanguage_380_jh);
                    Check(true, "UI constructor, resizing and translation: " + vista.GetType().Name);
                }
            }
            using (var pantalla = new MainForm_380_jh())
            {
                pantalla.GetType().GetMethod("MainForm_Load_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(pantalla, new object[] { pantalla, EventArgs.Empty });
                pantalla.PerformLayout();
                Check(true, "Main form load and integrity status integration");
            }
            using (var unidades = new UnidadesView_380_jh())
            {
                Sql("UPDATE dbo.Unidad SET fecha_vencimiento='00010101' WHERE id_unidad=1");
                Check(!servicio.Verificar_380_jh().EsValida_380_jh, "Application maps corruption");
                var boton = (Button)unidades.GetType().GetField("_btnClasificar_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(unidades);
                Check(!boton.Enabled, "Integrity event disables an already open business screen");
                using (var corrupta = new UnidadesView_380_jh())
                {
                    corrupta.CreateControl();
                    corrupta.PerformLayout();
                    var grilla = (DataGridView)corrupta.GetType().GetField("_grid_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(corrupta);
                    foreach (DataGridViewRow fila in grilla.Rows)
                        if (((Unidad_380_jh)fila.Tag).Id_380_jh == 1) grilla.CurrentCell = fila.Cells[0];
                    Check(true, "A corrupted date outside WinForms range does not crash the UI");
                }
                Check(new DonanteApplicationService_380_jh().Registrar_380_jh(new Donante_380_jh { Documento_380_jh = "UI", Nombre_380_jh = "Test", Apellido_380_jh = "Test" }) == CodigoOperacionPN1_380_jh.IntegridadInvalida_380_jh, "Application rejects a write during corruption");
                var informe = servicio.Verificar_380_jh();
                Check(servicio.Recuperar_380_jh(informe.Revision_380_jh, false) == "INTEGRITY_RECOVERED", "Application recovery uses verified revision");
                Check(boton.Enabled, "Verified recovery enables business screen");
            }
            Sesion_380_jh.ObtenerInstancia_380_jh().IniciarSesion_380_jh(new Usuario_380_jh { Id_380_jh = 2 });
            Check(servicio.Recuperar_380_jh("", false) == "OPERATION_NOT_AUTHORIZED", "Application enforces recovery permissions");
            Check(new AuditoriaApplicationService_380_jh().ListarNegocio_380_jh("Donante").Count == 0, "Application protects PN1 audit history");
            // Missing migration must never produce a green report.
            Sql("EXEC sp_rename 'dbo.sp_IntegridadPN1_Verificar','sp_IntegridadPN1_Verificar_test'");
            try { Check(!servicio.Verificar_380_jh().Disponible_380_jh && !IntegridadApplicationService_380_jh.Estado_380_jh.EsValida_380_jh, "Missing migration fails closed"); }
            finally { Sql("EXEC sp_rename 'dbo.sp_IntegridadPN1_Verificar_test','sp_IntegridadPN1_Verificar'"); }
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
    private static void Sql(string sql)
    {
        using (var cn = new SqlConnection(conexion))
        using (var cmd = new SqlCommand(sql, cn)) { cn.Open(); cmd.ExecuteNonQuery(); }
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception("FAIL " + message);
        Console.WriteLine("OK " + message);
    }
}
