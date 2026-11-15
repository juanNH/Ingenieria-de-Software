using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using DAL;
using Domain;
using Services;
using UI;

internal static class CicloSesionUiTests
{
    private static string Output;
    private static readonly List<object> Cases = new List<object>();
    private static readonly List<string> Checks = new List<string>();
    private static readonly List<object> Exceptions = new List<object>();
    private static int Unhandled, NullReferences;
    private static string CurrentCase = "setup";
    [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length != 5 || args[0] != "--sql") return 2;
        Output = args[4]; Directory.CreateDirectory(Output);
        File.WriteAllText(Path.Combine(Output, "ciclo-sesion.log"), string.Empty);
        try
        {
            GuardRuntime();
            if (!Regex.IsMatch(args[2], "^CodexAbast_[a-f0-9]{12}$")) throw new Exception("Only the isolated fixture database is allowed.");
            AppDomain.CurrentDomain.FirstChanceException += FirstChance;
            System.Windows.Forms.Application.ThreadException += delegate(object sender, System.Threading.ThreadExceptionEventArgs e)
            {
                Unhandled++; Log("THREAD " + e.Exception); System.Windows.Forms.Application.ExitThread();
            };
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            string connection = new SqlConnectionStringBuilder { DataSource = args[1], InitialCatalog = args[2], IntegratedSecurity = true, TrustServerCertificate = true }.ConnectionString;
            typeof(DatabaseContext_380_jh).GetField("ConnectionString_380_jh", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, connection);
            int userId = int.Parse(args[3]);
            // Add one traceable movement and the trace permission only in the disposable fixture.
            using (var db = new SqlConnection(connection))
            using (var cmd = db.CreateCommand())
            {
                db.Open(); cmd.CommandText = @"SET XACT_ABORT ON; BEGIN TRANSACTION;
INSERT dbo.ComponentePermiso(codigo,nombre,tipo) VALUES('TEST_CICLO_SESION',N'Ciclo de sesion (fixture)','FAMILIA');
DECLARE @familia INT=SCOPE_IDENTITY();
INSERT dbo.ComponentePermisoRelacion(id_padre,id_hijo)
SELECT @familia,id_componente FROM dbo.ComponentePermiso WHERE codigo IN ('REPORTE_VER','TRAZABILIDAD_VER');
UPDATE dbo.UsuarioComponentePermiso SET id_componente=@familia WHERE id_usuario=@u;
INSERT dbo.MovimientoUnidad(id_unidad,tipo_movimiento,estado_anterior,estado_nuevo,observacion,id_usuario)
SELECT TOP(1) id_unidad,'UBICACION',estado_operativo,estado_operativo,N'Fixture de cierre de sesion',@u FROM dbo.Unidad ORDER BY id_unidad;
EXEC dbo.sp_IntegridadPN1_Sellar @u,'BASELINE'; COMMIT;";
                cmd.Parameters.Add("@u", SqlDbType.Int).Value = userId; cmd.ExecuteNonQuery();
            }
            var user = new Usuario_380_jh { Id_380_jh = userId, Username_380_jh = "abast_reporter", Estado_380_jh = "ACTIVO" };
            user.ComponentesPermiso_380_jh = new PermisoDataMapper_380_jh().ListarAsignadosPorUsuario_380_jh(userId);
            foreach (string view in new[] { "Abastecimiento", "Comparacion", "Reportes", "Trazabilidad" })
                foreach (bool logout in new[] { false, true })
                    for (int repeat = 0; repeat < 2; repeat++) RunCase(user, view, logout, repeat);
            Check(Unhandled == 0 && NullReferences == 0, "No unhandled UI exception or first-chance NullReferenceException in the matrix");
            WriteResult("passed"); return 0;
        }
        catch (Exception ex) { Log("FATAL " + ex); WriteResult("failed"); return 1; }
    }

    private static void RunCase(Usuario_380_jh user, string viewName, bool logout, int repeat)
    {
        CurrentCase = viewName + "/" + (logout ? "Salir" : "X") + "/" + repeat;
        Log("BEGIN " + CurrentCase);
        int observersBefore = ObserverCount();
        // Exercise the same modal-login / Application.Run transition as Program, without entering credentials.
        using (var login = new UI.Login_380_jh())
        using (var loginTimer = new Timer { Interval = 20 })
        {
            Offscreen(login); loginTimer.Tick += delegate { loginTimer.Stop(); login.DialogResult = DialogResult.OK; login.Close(); };
            loginTimer.Start(); Check(login.ShowDialog() == DialogResult.OK, CurrentCase + ": modal login returns OK");
        }
        Sesion_380_jh.ObtenerInstancia_380_jh().IniciarSesion_380_jh(user);
        bool comparisonActive = false, hadGridFocus = false, dropdownOpen = false;
        UserControl view = null; int step = 0;
        using (var form = new MainForm_380_jh())
        using (var timer = new Timer { Interval = 40 })
        {
            Offscreen(form);
            timer.Tick += delegate
            {
                try
                {
                    if (step++ == 0)
                    {
                        view = viewName == "Reportes" ? (UserControl)new ReportesView_380_jh() : viewName == "Trazabilidad" ? new TrazabilidadView_380_jh() : new AbastecimientoView_380_jh();
                        typeof(MainForm_380_jh).GetMethod("ShowScreen_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(form, new object[] { view });
                        var grid = Descendants(view).OfType<DataGridView>().First();
                        Check(form.IsHandleCreated && grid.IsHandleCreated && grid.Rows.Count > 0, CurrentCase + ": real main/view/grid handles and SQL rows exist");
                        foreach (DataGridViewRow row in grid.Rows) { grid.CurrentCell = row.Cells[0]; row.Selected = true; }
                        grid.Focus(); hadGridFocus = grid.Focused || grid.ContainsFocus;
                        Check(hadGridFocus, CurrentCase + ": grid receives focus and selection");
                        var tabs = Descendants(view).OfType<TabControl>().FirstOrDefault();
                        if (viewName == "Comparacion" && tabs != null && tabs.TabPages.Count > 1) { tabs.SelectedIndex = 1; comparisonActive = true; }
                        return;
                    }
                    if (step == 2)
                    {
                        var manager = LanguageManager_380_jh.Instance_380_jh;
                        manager.ChangeLanguage_380_jh(manager.ListarIdiomasActivos_380_jh().First(x => x.Id_380_jh != manager.CurrentLanguage_380_jh.Id_380_jh), user);
                        var menu = (ToolStripMenuItem)Field(form, "_consultasToolStripMenuItem_380_jh");
                        typeof(MainForm_380_jh).GetMethod("MostrarDesplegableAlPasarMouse_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(form, new object[] { menu, EventArgs.Empty });
                        dropdownOpen = menu.DropDown.Visible;
                        Check(dropdownOpen, CurrentCase + ": production hover handler opens Consultas dropdown");
                        return;
                    }
                    timer.Stop();
                    if (logout) ((ToolStripMenuItem)Field(form, "salirToolStripMenuItem_380_jh")).PerformClick();
                    else SendMessage(form.Handle, 0x0112, new IntPtr(0xF060), IntPtr.Zero); // WM_SYSCOMMAND / SC_CLOSE
                }
                catch (Exception ex) { Unhandled++; Log("CASE " + ex); timer.Stop(); form.Close(); }
            };
            timer.Start(); System.Windows.Forms.Application.Run(form);
            DialogResult result = form.DialogResult;
            Check(Unhandled == 0 && form.IsDisposed && view != null && view.IsDisposed, CurrentCase + ": Run returns and disposes main and content");
            Check(logout ? result == DialogResult.Retry && !Sesion_380_jh.ObtenerInstancia_380_jh().HaySesionActiva_380_jh() : result != DialogResult.Retry, CurrentCase + ": logout/X result is preserved after disposal");
            Check(ObserverCount() == observersBefore, CurrentCase + ": main and content detach language observers");
            LanguageManager_380_jh.Instance_380_jh.Notify_380_jh();
            Cases.Add(new { name = CurrentCase, view = viewName, comparisonActive, hadGridFocus, dropdownOpen, result = result.ToString(), observersBefore, observersAfter = ObserverCount() });
        }
        Sesion_380_jh.ObtenerInstancia_380_jh().Logout_380_jh();
    }

    private static void GuardRuntime()
    {
        string dir = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
        if (!dir.StartsWith(Path.GetTempPath(), StringComparison.OrdinalIgnoreCase) || !new DirectoryInfo(dir).Name.StartsWith("CodexCicloSesion_")) throw new Exception("Run from the isolated temporary runner directory.");
        foreach (string path in new[] { Path.Combine(dir, "App.local.config"), Path.GetFullPath(Path.Combine(dir, @"..\..\..\DAL\App.local.config")), Path.GetFullPath(Path.Combine(dir, @"..\..\..\..\DAL\App.local.config")) })
            if (File.Exists(path)) throw new Exception("Private configuration candidate exists; refusing to initialize DAL.");
    }
    private static void FirstChance(object sender, FirstChanceExceptionEventArgs args)
    {
        Exception ex = args.Exception;
        if (!(ex is NullReferenceException) && !(ex is UnauthorizedAccessException) && !(ex is ObjectDisposedException) && !(ex is ArgumentException)) return;
        if (ex is NullReferenceException) NullReferences++;
        Exceptions.Add(new { test = CurrentCase, type = ex.GetType().FullName, message = ex.Message, stack = ex.StackTrace });
        Log("FIRST " + ex);
    }
    private static object Field(object item, string name) { return typeof(MainForm_380_jh).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(item); }
    private static int ObserverCount() { return ((ICollection)typeof(LanguageManager_380_jh).GetField("_observers_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(LanguageManager_380_jh.Instance_380_jh)).Count; }
    private static IEnumerable<Control> Descendants(Control control) { foreach (Control child in control.Controls) { yield return child; foreach (var nested in Descendants(child)) yield return nested; } }
    private static void Offscreen(Form form) { form.ShowInTaskbar = false; form.StartPosition = FormStartPosition.Manual; form.Location = new Point(-20000, -20000); }
    private static void Check(bool ok, string name) { if (!ok) throw new Exception(name); Checks.Add(name); }
    private static void Log(string message) { if (Output != null) File.AppendAllText(Path.Combine(Output, "ciclo-sesion.log"), CurrentCase + ": " + message + Environment.NewLine); }
    private static void WriteResult(string status)
    {
        var result = new { status, checkCount = Checks.Count, checks = Checks, cases = Cases, exceptions = Exceptions, unhandledExceptions = Unhandled, nullReferenceExceptions = NullReferences, privateConfigurationRead = false,
            scope = "Production MainForm/Application.Run with actual SQL views, modal-login transition, language change, grid focus, open dropdown and Salir/SC_CLOSE. Credentials are not entered in Login; session is seeded from the isolated fixture." };
        File.WriteAllText(Path.Combine(Output, "ciclo-sesion-tests.json"), new JavaScriptSerializer { MaxJsonLength = int.MaxValue }.Serialize(result));
    }
}
