using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Application;
using DAL;
using Domain;
using Services;
using UI;

internal static class AbastecimientoUiTests
{
    private static readonly List<string> Passed = new List<string>();
    private static readonly List<Exception> UiExceptions = new List<Exception>();
    private static readonly List<string> Captures = new List<string>();
    private static bool CaptureImages = true;
    private static string Output;

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            GuardIsolatedRuntime();
            CaptureImages = !args.Contains("--no-captures");
            args = args.Where(x => x != "--no-captures").ToArray();
            System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            System.Windows.Forms.Application.ThreadException += delegate(object sender, System.Threading.ThreadExceptionEventArgs e) { UiExceptions.Add(e.Exception); };
            System.Windows.Forms.Application.EnableVisualStyles();
            if (args.Length == 2 && args[0] == "--unit") { Output = args[1]; Directory.CreateDirectory(Output); Unit(); }
            else if (args.Length == 5 && args[0] == "--sql")
            {
                Output = args[4]; Directory.CreateDirectory(Output); Integration(args[1], args[2], int.Parse(args[3]));
            }
            else throw new ArgumentException("Expected --unit output or --sql server database user output.");
            Pump();
            var data = new { status = "passed", checkCount = Passed.Count, checks = Passed, privateConfigurationRead = false,
                reportViews = 1, asynchronousUiErrors = UiExceptions.Count, captures = Captures };
            File.WriteAllText(Path.Combine(Output, args[0] == "--unit" ? "ui-unit-tests.json" : "ui-integration-tests.json"), new JavaScriptSerializer().Serialize(data));
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

    private static void GuardIsolatedRuntime()
    {
        string dir = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
        string temp = Path.GetFullPath(Path.GetTempPath());
        if (!dir.StartsWith(temp, StringComparison.OrdinalIgnoreCase) || !new DirectoryInfo(dir).Name.StartsWith("CodexAbastUi_"))
            throw new InvalidOperationException("Run only from the isolated temporary harness folder.");
        foreach (string path in new[] { Path.Combine(dir, "App.local.config"), Path.GetFullPath(Path.Combine(dir, @"..\..\..\DAL\App.local.config")), Path.GetFullPath(Path.Combine(dir, @"..\..\..\..\DAL\App.local.config")) })
            if (File.Exists(path)) throw new InvalidOperationException("A private configuration candidate exists; refusing to initialize DAL.");
    }

    private static void Unit()
    {
        Dictionary<string, Dictionary<string, string>> texts;
        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Abastecimiento.Traducciones.json"))
        using (var reader = new StreamReader(stream)) texts = new JavaScriptSerializer().Deserialize<Dictionary<string, Dictionary<string, string>>>(reader.ReadToEnd());
        string language = "es-AR";
        Func<string, string> translate = key => texts[language].ContainsKey(key) ? texts[language][key] : key;
        bool fail = false;
        int queries = 0;
        AbastecimientoFiltro_380_jh last = null;
        List<FilaAbastecimiento_380_jh> samples = Samples();
        Func<AbastecimientoFiltro_380_jh, List<FilaAbastecimiento_380_jh>> query = filter =>
        {
            queries++; last = filter;
            if (fail) throw new InvalidOperationException("Simulated query failure");
            return samples.Where(x => string.IsNullOrEmpty(filter.TipoComponente_380_jh) || x.TipoComponente_380_jh == filter.TipoComponente_380_jh)
                .Where(x => string.IsNullOrEmpty(filter.GrupoSanguineo_380_jh) || x.GrupoSanguineo_380_jh == filter.GrupoSanguineo_380_jh)
                .Where(x => string.IsNullOrEmpty(filter.FactorRh_380_jh) || x.FactorRh_380_jh == filter.FactorRh_380_jh).ToList();
        };
        using (var view = new AbastecimientoView_380_jh(query, translate))
        using (var form = Host(view))
        {
            form.Show(); Pump();
            var grid = Field<DataGridView>(view, "_grilla_380_jh");
            Check(grid.Rows.Count == 5, "Five fictional cohorts displayed");
            Check(Field<Label>(view, "_stock_380_jh").Text == "22" && Field<Label>(view, "_vence_380_jh").Text == "6" &&
                Field<Label>(view, "_deficit_380_jh").Text == "11" && Field<Label>(view, "_estudio_380_jh").Text == "6", "KPIs separate typed stock/shortage and unvalidated pending units");
            Check(new[] { "_stock_380_jh", "_vence_380_jh", "_deficit_380_jh", "_estudio_380_jh" }.All(name => NumberRenders(Field<Label>(view, name))), "KPI numbers have complete bounds and visible foreground pixels");
            var unknown = grid.Rows.Cast<DataGridViewRow>().Single(x => !((FilaAbastecimiento_380_jh)x.Tag).CohorteValidada_380_jh);
            Check((string)unknown.Cells["stock"].FormattedValue == "N/A" && (string)unknown.Cells["cobertura"].FormattedValue == "N/A", "Unvalidated study shows N/A instead of zero or false coverage");
            grid.CurrentCell = unknown.Cells[0]; unknown.Selected = true; Pump();
            Check(Field<Label>(view, "_detalle_380_jh").Text.Contains("6") && Field<Label>(view, "_detalle_380_jh").Text.Contains("sin estudio"), "Unvalidated selection explains pending study");
            Field<DateTimePicker>(view, "_desde_380_jh").Value = DateTime.Today.AddDays(-9);
            Field<NumericUpDown>(view, "_horizonte_380_jh").Value = 14;
            Field<ComboBox>(view, "_componente_380_jh").SelectedValue = "Concentrado de eritrocitos";
            Field<Button>(view, "_generar_380_jh").PerformClick(); Pump();
            Check(last.Desde_380_jh == DateTime.Today.AddDays(-9) && last.Hasta_380_jh == DateTime.Today && last.HorizonteVencimientoDias_380_jh == 14 && last.TipoComponente_380_jh == "Concentrado de eritrocitos" && grid.Rows.Count == 2, "Filters reach query delegate with inclusive dates and chosen horizon/component");
            int before = queries; Field<CheckBox>(view, "_soloAlertas_380_jh").Checked = true; Pump();
            Check(grid.Rows.Count == 1 && queries == before && Field<Label>(view, "_stock_380_jh").Text == "12", "Alerts filter updates grid and KPIs without another query");
            Field<CheckBox>(view, "_soloAlertas_380_jh").Checked = false;
            Field<ComboBox>(view, "_componente_380_jh").SelectedValue = string.Empty;
            Field<DateTimePicker>(view, "_desde_380_jh").Value = DateTime.Today.AddDays(-29);
            Field<NumericUpDown>(view, "_horizonte_380_jh").Value = 7;
            Field<Button>(view, "_generar_380_jh").PerformClick(); Pump();
            language = "en-US"; view.OnLanguageChanged_380_jh(new Idioma_380_jh { Codigo_380_jh = language }); Pump();
            Check(grid.Columns["prioridad"].HeaderText == "Priority" && Controls(view).OfType<Label>().Any(x => (string)x.Tag == "SUPPLY_TITLE" && x.Text == "Supply and expiry risk"), "Language observer refreshes heading and grid labels");
            Check(grid.Rows.Count == 5 && Field<NumericUpDown>(view, "_horizonte_380_jh").Value == 7 && Field<DateTimePicker>(view, "_desde_380_jh").Value.Date == DateTime.Today.AddDays(-29), "Language switch preserves data and filters");
            Snapshot(form, "abastecimiento-en-1280x920.png", new Size(1280, 920));
            language = "es-AR"; view.OnLanguageChanged_380_jh(new Idioma_380_jh { Codigo_380_jh = language }); Pump();
            Snapshot(form, "abastecimiento-detalle-1280x920.png", new Size(1280, 920));
            Check(!Controls(view).OfType<TabControl>().Any() && Controls(view).OfType<DataGridView>().Count() == 1,
                "A single report grid is presented directly without tabs");
            Snapshot(form, "abastecimiento-detalle-1000x620.png", new Size(1000, 620));
            Snapshot(form, "abastecimiento-detalle-1000x700.png", new Size(1000, 700));
            Check(grid.Controls.OfType<HScrollBar>().All(x => !x.Visible) && grid.Columns.Cast<DataGridViewColumn>().Sum(x => x.Width) <= grid.ClientSize.Width &&
                grid.Rows.Count == 5 && grid.Rows.Cast<DataGridViewRow>().All(x => x.Displayed), "Narrow 1000x700 viewport shows every column and all five cohorts");
            fail = true; Field<Button>(view, "_generar_380_jh").PerformClick(); Pump();
            Check(grid.Rows.Count == 0 && Field<Label>(view, "_stock_380_jh").Text == "0" && Field<Label>(view, "_deficit_380_jh").Text == "0", "Failed query clears previous grid and KPI data");
            Check(Field<Label>(view, "_estado_380_jh").Text == texts[language]["SUPPLY_QUERY_ERROR"], "Failed query displays a translated error");
            Snapshot(form, "abastecimiento-error-1000x620.png", new Size(1000, 620));
            language = "en-US"; view.OnLanguageChanged_380_jh(new Idioma_380_jh { Codigo_380_jh = language }); Pump();
            Check(Field<Label>(view, "_estado_380_jh").Text == texts[language]["SUPPLY_QUERY_ERROR"], "Error state survives language switch");
            fail = false; Field<Button>(view, "_generar_380_jh").PerformClick(); Pump();
            Check(grid.Rows.Count == 5, "A successful retry restores data");
            Field<DateTimePicker>(view, "_desde_380_jh").Value = DateTime.Today;
            Field<DateTimePicker>(view, "_hasta_380_jh").Value = DateTime.Today.AddDays(-1);
            before = queries; Field<Button>(view, "_generar_380_jh").PerformClick(); Pump();
            Check(queries == before && Field<Label>(view, "_estado_380_jh").Text == texts[language]["SUPPLY_INVALID_PERIOD"], "Invalid date order is rejected before querying");
            Check(grid.Rows.Count == 0 && Field<Label>(view, "_stock_380_jh").Text == "0", "Invalid date order does not leave stale result figures");
        }
        Lifecycle(texts);
    }

    private static void Lifecycle(Dictionary<string, Dictionary<string, string>> texts)
    {
        var manager = LanguageManager_380_jh.Instance_380_jh;
        var observers = (IList<IObserverLanguage_380_jh>)typeof(LanguageManager_380_jh)
            .GetField("_observers_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(manager);
        int initialObserverCount = observers.Count;
        int handlesCreated = 0, selections = 0, translated = 0, disposed = 0, detached = 0;
        for (int cycle = 0; cycle < 3; cycle++)
        {
            string language = "es-AR";
            Func<string, string> translate = key => texts[language].ContainsKey(key) ? texts[language][key] : key;
            using (var view = new LifecycleProbeView(filter => Samples(), translate))
            using (var form = Host(view))
            {
                form.Show(); Pump();
                var grid = Field<DataGridView>((AbastecimientoView_380_jh)view, "_grilla_380_jh");
                Require(form.IsHandleCreated && view.IsHandleCreated && grid.IsHandleCreated && observers.Contains(view),
                    "Lifecycle cycle " + cycle + " must load actual handles and attach its language observer.");
                handlesCreated++;
                foreach (DataGridViewRow row in grid.Rows)
                {
                    grid.ClearSelection(); grid.CurrentCell = row.Cells[0]; row.Selected = true; Pump();
                    Require(!string.IsNullOrWhiteSpace(Field<Label>((AbastecimientoView_380_jh)view, "_detalle_380_jh").Text), "Selected row must have an explanation.");
                    selections++;
                }
                language = "en-US";
                manager.ChangeLanguage_380_jh(new Idioma_380_jh { Id_380_jh = 2, Codigo_380_jh = language, Activo_380_jh = true }, null);
                Pump();
                Require(grid.Columns["prioridad"].HeaderText == "Priority" && grid.Rows.Count == 5, "Live observer notification must preserve and translate the grid.");
                language = "es-AR";
                manager.ChangeLanguage_380_jh(new Idioma_380_jh { Id_380_jh = 1, Codigo_380_jh = language, Activo_380_jh = true }, null);
                Pump();
                Require(grid.Columns["prioridad"].HeaderText == "Prioridad", "Second live language switch must restore Spanish.");
                translated++;
                Field<Button>((AbastecimientoView_380_jh)view, "_generar_380_jh").PerformClick(); Pump();
                grid.CurrentCell = grid.Rows[grid.Rows.Count - 1].Cells[0]; grid.Rows[grid.Rows.Count - 1].Selected = true; Pump();
                form.Close(); Pump();
                Require(form.IsDisposed && view.IsDisposed && grid.IsDisposed && !view.IsHandleCreated,
                    "Closing a loaded report with a selected row must dispose controls and destroy handles.");
                disposed++;
                int notificationsAtClose = view.LanguageNotifications;
                manager.Notify_380_jh(); Pump();
                Require(!observers.Contains(view) && observers.Count == initialObserverCount && view.LanguageNotifications == notificationsAtClose,
                    "A disposed report must detach before subsequent language notifications.");
                detached++;
            }
        }
        Check(handlesCreated == 3, "Report can be opened and reopened three times with real form/view/grid handles");
        Check(selections == 15, "Every cohort can be selected across repeated report lifecycles");
        Check(translated == 3, "Real language-manager notifications update a loaded report before closure");
        Check(disposed == 3, "Closing a selected and refreshed report disposes controls without UI exceptions");
        Check(detached == 3, "Closed reports detach their observer and receive no later language callbacks");
    }

    private sealed class LifecycleProbeView : AbastecimientoView_380_jh
    {
        internal int LanguageNotifications;
        internal LifecycleProbeView(Func<AbastecimientoFiltro_380_jh, List<FilaAbastecimiento_380_jh>> query, Func<string, string> translate) : base(query, translate) { }
        public override void OnLanguageChanged_380_jh(Idioma_380_jh language) { LanguageNotifications++; base.OnLanguageChanged_380_jh(language); }
    }

    private static void Integration(string server, string database, int userId)
    {
        if (!Regex.IsMatch(database, "^CodexAbast_[a-f0-9]{12}$")) throw new InvalidOperationException("Only the isolated test database is allowed.");
        string connection = new SqlConnectionStringBuilder { DataSource = server, InitialCatalog = database, IntegratedSecurity = true, TrustServerCertificate = true }.ConnectionString;
        typeof(DatabaseContext_380_jh).GetField("ConnectionString_380_jh", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, connection);
        var user = new Usuario_380_jh { Id_380_jh = userId, Username_380_jh = "abast_reporter", Estado_380_jh = "ACTIVO" };
        user.ComponentesPermiso_380_jh = new PermisoDataMapper_380_jh().ListarAsignadosPorUsuario_380_jh(userId);
        Sesion_380_jh.ObtenerInstancia_380_jh().IniciarSesion_380_jh(user);
        LanguageManager_380_jh.Instance_380_jh.Initialize_380_jh(user);
        var service = new PN2ApplicationService_380_jh();
        var filter = new AbastecimientoFiltro_380_jh { Desde_380_jh = DateTime.Today.AddDays(-10), Hasta_380_jh = DateTime.Today.AddDays(-1), HorizonteVencimientoDias_380_jh = 7 };
        var rows = service.ListarAbastecimiento_380_jh(filter);
        var a = rows.Single(x => x.TipoComponente_380_jh == "Concentrado de eritrocitos" && x.GrupoSanguineo_380_jh == "A");
        Check(rows.Count == 5 && a.StockLiberadoVigente_380_jh == 3 && a.DemandaPeriodo_380_jh == 10 && a.PendienteVigente_380_jh == 8 && a.CoberturaDias_380_jh == 3 && a.DeficitUnidades_380_jh == 5, "Real Application/Repository/DAL/SQL chain maps metrics and decimal coverage");
        Check(rows.Single(x => !x.CohorteValidada_380_jh).StockLiberadoVigente_380_jh == null, "Real DAL preserves nullable unvalidated metrics");
        using (var view = new AbastecimientoView_380_jh(service))
        using (var form = Host(view))
        {
            form.Show(); Pump();
            Check(Field<DataGridView>(view, "_grilla_380_jh").Rows.Count == 5, "Production service constructor loads real SQL fixture data");
            Check(!Controls(view).Any(x => x.Text.StartsWith("SUPPLY_")), "SQL translations resolve UI supply keys");
            Snapshot(form, "abastecimiento-integracion-sql-1000x700.png", new Size(1000, 700));
        }
        using (var main = new MainForm_380_jh())
        {
            main.WindowState = FormWindowState.Normal;
            main.StartPosition = FormStartPosition.Manual;
            main.Location = new Point(-30000, -30000);
            main.ClientSize = new Size(1280, 800);
            main.ShowInTaskbar = false;
            Exception lifecycleError = null;
            AbastecimientoView_380_jh loaded = null;
            DataGridView selectedGrid = null;
            main.Shown += delegate
            {
                main.BeginInvoke(new Action(delegate
                {
                    try
                    {
                        var consultations = Field<ToolStripMenuItem>(main, "_consultasToolStripMenuItem_380_jh");
                        var supply = Field<ToolStripMenuItem>(main, "_abastecimientoToolStripMenuItem_380_jh");
                        var menu = Field<MenuStrip>(main, "menuStrip1_380_jh");
                        Check(menu.Items.Contains(consultations) && consultations.DropDownItems.Count == 1 && consultations.DropDownItems.Contains(supply) && (string)supply.Tag == "SUPPLY_TITLE", "Consultations menu is attached and contains only the localized supply item");
                        typeof(MainForm_380_jh).GetMethod("ConfigurarMenuPorPermisos_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(main, null);
                        Check(consultations.Available && supply.Available, "Report-only role sees consultations and its single supply option");
                        var initial = Controls(main).OfType<AbastecimientoView_380_jh>().Single();
                        Check(main.IsHandleCreated && initial.IsHandleCreated && Field<DataGridView>(initial, "_grilla_380_jh").IsHandleCreated,
                            "A real MainForm load opens the supply view for a report-only role");
                        MenuSnapshot(main, menu, consultations);
                        supply.PerformClick(); Pump();
                        loaded = Controls(main).OfType<AbastecimientoView_380_jh>().Single();
                        Check(initial.IsDisposed && Field<DataGridView>(loaded, "_grilla_380_jh").Rows.Count == 5,
                            "Supply menu click disposes the previous view and opens real SQL data");
                        Sesion_380_jh.ObtenerInstancia_380_jh().IniciarSesion_380_jh(new Usuario_380_jh { Id_380_jh = userId, Username_380_jh = "without_permissions", Estado_380_jh = "ACTIVO" });
                        typeof(MainForm_380_jh).GetMethod("ConfigurarMenuPorPermisos_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(main, null);
                        Check(!consultations.Available && !supply.Available, "Without report and trace permissions both menu entries are hidden");
                        Sesion_380_jh.ObtenerInstancia_380_jh().IniciarSesion_380_jh(new Usuario_380_jh { Id_380_jh = userId, Username_380_jh = "trace_only", Estado_380_jh = "ACTIVO",
                            ComponentesPermiso_380_jh = new List<ComponentePermiso_380_jh> { new Permiso_380_jh { Codigo_380_jh = PermisosSistema_380_jh.TrazabilidadVer_380_jh } } });
                        typeof(MainForm_380_jh).GetMethod("ConfigurarMenuPorPermisos_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(main, null);
                        Check(!consultations.Available && !supply.Available, "Trace-only role cannot see consultations or supply without report permission");
                        Sesion_380_jh.ObtenerInstancia_380_jh().IniciarSesion_380_jh(user);
                        selectedGrid = Field<DataGridView>(loaded, "_grilla_380_jh");
                        selectedGrid.CurrentCell = selectedGrid.Rows[selectedGrid.Rows.Count - 1].Cells[0];
                        selectedGrid.Rows[selectedGrid.Rows.Count - 1].Selected = true; Pump();
                        main.Close();
                    }
                    catch (Exception ex) { lifecycleError = ex; main.Close(); }
                }));
            };
            System.Windows.Forms.Application.Run(main);
            if (lifecycleError != null) throw new InvalidOperationException("Real MainForm lifecycle failed.", lifecycleError);
            Pump();
            Check(main.IsDisposed && loaded.IsDisposed && selectedGrid.IsDisposed,
                "Closing the real MainForm disposes its selected report without UI exceptions");
        }
        Check(new IntegridadApplicationService_380_jh().Verificar_380_jh().EsValida_380_jh, "Real report leaves integrity valid");
    }

    private static void MenuSnapshot(MainForm_380_jh main, MenuStrip menu, ToolStripMenuItem consultations)
    {
        consultations.ShowDropDown();
        // Keep the native popup offscreen while rendering the real controls into the evidence image.
        consultations.DropDown.Location = new Point(-30000, -30000);
        Pump();
        Check(consultations.DropDown.Visible && consultations.DropDownItems.Count == 1 && consultations.DropDownItems[0].Available,
            "Opening consultations displays exactly one available supply entry");
        if (CaptureImages)
        {
            Point clientOffset = main.PointToScreen(Point.Empty);
            clientOffset.Offset(-main.Left, -main.Top);
            using (var bitmap = new Bitmap(main.Width, main.Height))
            using (var popup = new Bitmap(consultations.DropDown.Width, consultations.DropDown.Height))
            {
                main.DrawToBitmap(bitmap, new Rectangle(Point.Empty, main.Size));
                consultations.DropDown.DrawToBitmap(popup, new Rectangle(Point.Empty, popup.Size));
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.DrawImageUnscaled(popup, clientOffset.X + menu.Left + consultations.Bounds.Left, clientOffset.Y + menu.Top + consultations.Bounds.Bottom);
                    using (var background = new SolidBrush(Color.FromArgb(235, 239, 243)))
                        graphics.FillRectangle(background, 0, bitmap.Height - 26, bitmap.Width, 26);
                    TextRenderer.DrawText(graphics, "DATOS FICTICIOS DE PRUEBA · PERFIL REPORTE_VER", main.Font,
                        new Rectangle(0, bitmap.Height - 26, bitmap.Width, 26), Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
                const string filename = "mainform-consultas-unica-entrada.png";
                bitmap.Save(Path.Combine(Output, filename)); Captures.Add(filename);
            }
        }
        consultations.HideDropDown(); Pump();
    }

    private static Form Host(Control view)
    {
        var form = new Form { MaximumSize = new Size(4000, 4000), ClientSize = new Size(1280, 920), Location = new Point(-30000, -30000), StartPosition = FormStartPosition.Manual, ShowInTaskbar = false };
        var root = new Panel { Size = new Size(1280, 920), Location = Point.Empty, Name = "captureRoot" };
        var banner = new Label { Text = "DATOS FICTICIOS PARA PRUEBAS Y DOCUMENTACIÓN · FICTIONAL TEST DATA", Dock = DockStyle.Top, Height = 26, BackColor = Color.FromArgb(235, 239, 243), TextAlign = ContentAlignment.MiddleCenter };
        view.Dock = DockStyle.Fill; root.Controls.Add(view); root.Controls.Add(banner); form.Controls.Add(root); return form;
    }
    private static void Snapshot(Form form, string filename, Size size)
    {
        form.ClientSize = size; var root = form.Controls[0];
        // Render the requested control viewport independently of the desktop's maximum form height.
        root.Dock = DockStyle.None; root.Location = Point.Empty; root.Size = size;
        foreach (Control control in Controls(root).Reverse()) control.PerformLayout();
        root.PerformLayout(); Pump();
        if (root.Size != size) throw new InvalidOperationException("Capture size differs from requested size: " + root.Size);
        var metrics = Controls(root).OfType<Label>().Where(x => x.Font.Size >= 20).Select(x => new { text = x.Text, height = x.Height, fontHeight = x.Font.Height, preferredHeight = x.PreferredSize.Height, bounds = x.Bounds.ToString(), visible = x.Visible }).ToArray();
        File.WriteAllText(Path.Combine(Output, "layout-" + filename + ".json"), new JavaScriptSerializer().Serialize(metrics));
        if (!CaptureImages) return;
        using (var warmup = new Bitmap(root.Width, root.Height)) root.DrawToBitmap(warmup, new Rectangle(Point.Empty, root.Size));
        Pump();
        using (var bitmap = new Bitmap(root.Width, root.Height)) { root.DrawToBitmap(bitmap, new Rectangle(Point.Empty, root.Size)); bitmap.Save(Path.Combine(Output, filename)); }
        Captures.Add(filename);
    }
    private static List<FilaAbastecimiento_380_jh> Samples()
    {
        return new List<FilaAbastecimiento_380_jh> {
            Sample("Concentrado de eritrocitos", "A", "+", 12, 4, 30, 20, 1m, 12m, 8, "REPOSICION_Y_VENCIMIENTO"),
            Sample("Concentrado de eritrocitos", "O", "-", 6, 0, 12, 4, .4m, 15m, 0, "SIN_ALERTA"),
            Sample("Plasma fresco congelado", "B", "+", 4, 2, 15, 2, .5m, 8m, 0, "VENCIMIENTO"),
            Sample("Plasma fresco congelado", null, null, null, null, 6, 6, .2m, null, null, "VALIDAR_ESTUDIO"),
            Sample("Concentrado de plaquetas", "AB", "+", 0, 0, 9, 3, .3m, 0m, 3, "REPOSICION") };
    }
    private static FilaAbastecimiento_380_jh Sample(string component, string group, string rh, long? stock, long? expiry, long demand, long pending, decimal rate, decimal? coverage, long? deficit, string priority)
    {
        return new FilaAbastecimiento_380_jh { TipoComponente_380_jh = component, GrupoSanguineo_380_jh = group, FactorRh_380_jh = rh, CohorteValidada_380_jh = group != null,
            StockLiberadoVigente_380_jh = stock, StockPorVencer_380_jh = expiry, DemandaPeriodo_380_jh = demand, PendienteVigente_380_jh = pending,
            DemandaDiariaPromedio_380_jh = rate, CoberturaDias_380_jh = coverage, DeficitUnidades_380_jh = deficit, Prioridad_380_jh = priority, FechaCorte_380_jh = DateTime.Today };
    }
    private static T Field<T>(object instance, string name)
    {
        for (Type type = instance.GetType(); type != null; type = type.BaseType)
        {
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (field != null) return (T)field.GetValue(instance);
        }
        throw new MissingFieldException(instance.GetType().FullName, name);
    }
    private static bool NumberRenders(Label label)
    {
        if (label.Height < label.Font.Height || !label.Parent.ClientRectangle.Contains(label.Bounds)) return false;
        using (var bitmap = new Bitmap(label.Width, label.Height))
        {
            label.DrawToBitmap(bitmap, new Rectangle(Point.Empty, label.Size));
            int foreground = 0;
            for (int y = 0; y < bitmap.Height; y++) for (int x = 0; x < bitmap.Width; x++)
            {
                Color pixel = bitmap.GetPixel(x, y), expected = label.ForeColor;
                if (Math.Abs(pixel.R - expected.R) < 24 && Math.Abs(pixel.G - expected.G) < 24 && Math.Abs(pixel.B - expected.B) < 24) foreground++;
            }
            return foreground > 10;
        }
    }
    private static IEnumerable<Control> Controls(Control root) { foreach (Control child in root.Controls) { yield return child; foreach (var nested in Controls(child)) yield return nested; } }
    private static void Pump()
    {
        System.Windows.Forms.Application.DoEvents();
        if (UiExceptions.Count > 0) throw new InvalidOperationException("An asynchronous WinForms callback failed.", UiExceptions[0]);
    }
    private static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static void Check(bool condition, string name) { if (!condition) throw new Exception(name); Passed.Add(name); Console.WriteLine("OK " + name); }
}
