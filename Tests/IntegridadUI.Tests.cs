using System;
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
            var cifrado = new CifradoEmailService_380_jh();
            string emailPrueba = "donante@example.invalid";
            string emailCifrado = cifrado.Cifrar_380_jh(emailPrueba);
            Check(emailCifrado != emailPrueba && cifrado.EstaCifrado_380_jh(emailCifrado), "Donor email is stored in reversible protected form");
            Check(cifrado.Descifrar_380_jh(emailCifrado) == emailPrueba, "Protected donor email decrypts to the original value");
            Check(cifrado.Descifrar_380_jh("legacy@example.invalid") == "legacy@example.invalid", "Legacy clear-text emails remain readable during upgrade");
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
            var servicioDonantes = new DonanteApplicationService_380_jh();
            var donantePrueba = new Donante_380_jh
            {
                Documento_380_jh = "EMAIL-CIPHER-TEST",
                Nombre_380_jh = "Correo",
                Apellido_380_jh = "Cifrado",
                Email_380_jh = emailPrueba
            };
            Check(servicioDonantes.Registrar_380_jh(donantePrueba) == CodigoOperacionPN1_380_jh.Ok_380_jh, "Donor can be registered through the application service");
            string valorAlmacenado = SqlString("SELECT email FROM dbo.Donante WHERE id_donante=" + donantePrueba.Id_380_jh);
            Check(valorAlmacenado != emailPrueba && cifrado.EstaCifrado_380_jh(valorAlmacenado), "Database stores ciphertext instead of the donor email");
            Check(servicioDonantes.ObtenerEmail_380_jh(donantePrueba.Id_380_jh) == emailPrueba, "Application service decrypts a selected donor email");

            Sql("EXEC dbo.sp_Donante_Registrar @documento='LEGACY-EMAIL-TEST',@nombre='Correo',@apellido='Anterior',@email='anterior@example.invalid',@id_usuario=1");
            int idDonanteAnterior = Convert.ToInt32(SqlString("SELECT id_donante FROM dbo.Donante WHERE documento='LEGACY-EMAIL-TEST'"));
            var donanteListado = servicioDonantes.Listar_380_jh().Find(d => d.Id_380_jh == donantePrueba.Id_380_jh);
            Check(donanteListado != null && donanteListado.TieneEmail_380_jh && donanteListado.Email_380_jh == null, "Donor list indicates a saved email without loading its contents");
            string valorAnteriorMigrado = SqlString("SELECT email FROM dbo.Donante WHERE id_donante=" + idDonanteAnterior);
            Check(cifrado.EstaCifrado_380_jh(valorAnteriorMigrado), "Existing clear-text donor emails are encrypted during the first list load");
            Check(servicioDonantes.ObtenerEmail_380_jh(idDonanteAnterior) == "anterior@example.invalid", "Migrated donor emails remain reversible");
            Check(servicio.Verificar_380_jh().EsValida_380_jh, "Existing email migration preserves PN1 integrity");
            var donanteActualizado = new Donante_380_jh
            {
                Id_380_jh = donantePrueba.Id_380_jh,
                Documento_380_jh = "EMAIL-CIPHER-TEST",
                Nombre_380_jh = "Correo actualizado",
                Apellido_380_jh = "Cifrado",
                Email_380_jh = null
            };
            Check(servicioDonantes.Actualizar_380_jh(donanteActualizado, false) == CodigoOperacionPN1_380_jh.Ok_380_jh, "Donor details can be updated without replacing a hidden email");
            Check(SqlString("SELECT email FROM dbo.Donante WHERE id_donante=" + donantePrueba.Id_380_jh) == valorAlmacenado, "Updating other donor fields preserves the encrypted email");
            string emailNuevo = "actualizado@example.invalid";
            donanteActualizado.Email_380_jh = emailNuevo;
            Check(servicioDonantes.Actualizar_380_jh(donanteActualizado, true) == CodigoOperacionPN1_380_jh.Ok_380_jh, "Donor email can be changed through the application service");
            valorAlmacenado = SqlString("SELECT email FROM dbo.Donante WHERE id_donante=" + donantePrueba.Id_380_jh);
            Check(valorAlmacenado != emailNuevo && cifrado.EstaCifrado_380_jh(valorAlmacenado), "Updated email remains encrypted in the database");
            Check(servicioDonantes.ObtenerEmail_380_jh(donantePrueba.Id_380_jh) == emailNuevo, "Updated email decrypts correctly");
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

                    if (vista is DonantesView_380_jh)
                    {
                        var grilla = (DataGridView)vista.GetType().GetField("_grid_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(vista);
                        FlowLayoutPanel filtro = null;
                        foreach (Control control in grilla.Parent.Controls)
                            if (control is FlowLayoutPanel && control.Dock == DockStyle.Top) filtro = (FlowLayoutPanel)control;
                        bool encabezadosVisibles = grilla.ColumnHeadersVisible && !grilla.RowHeadersVisible;
                        foreach (DataGridViewColumn columna in grilla.Columns)
                            encabezadosVisibles = encabezadosVisibles && !string.IsNullOrWhiteSpace(columna.HeaderText);
                        Check(encabezadosVisibles, "Donor grid shows translated column headers without an empty row-header column");
                        Check(filtro != null && filtro.Bottom <= grilla.Top, "Donor grid starts below the search controls");
                        var filaDonante = grilla.Rows.Cast<DataGridViewRow>().FirstOrDefault(f => Convert.ToInt32(f.Cells[0].Value) == donantePrueba.Id_380_jh);
                        Check(filaDonante != null && Convert.ToString(filaDonante.Cells[4].Value) == "••••••••", "Donor grid masks the email by default");
                        vista.GetType().GetMethod("Grilla_CellClick_380_jh", BindingFlags.Instance | BindingFlags.NonPublic)
                            .Invoke(vista, new object[] { grilla, new DataGridViewCellEventArgs(0, filaDonante.Index) });
                        Check(((TextBox)vista.GetType().GetField("_txtNombre_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(vista)).Text == "Correo actualizado",
                            "Selecting a donor loads its data into the edit form");
                        var botonEmail = (Button)vista.GetType().GetField("_btnVerEmail_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(vista);
                        var domicilio = (TextBox)vista.GetType().GetField("_txtDomicilio_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(vista);
                        var botonRegistrar = (Button)vista.GetType().GetField("_btnRegistrar_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(vista);
                        var formulario = domicilio.Parent;
                        var acciones = (FlowLayoutPanel)botonRegistrar.Parent;
                        Check(botonEmail.Height <= 32 && domicilio.Bottom <= formulario.ClientSize.Height && acciones.Bottom <= formulario.ClientSize.Height,
                            "Donor email reveal button stays compact and address/actions remain inside the form");
                        var eventoEmail = vista.GetType().GetMethod("VerEmail_Click_380_jh", BindingFlags.Instance | BindingFlags.NonPublic);
                        var campoEmail = (TextBox)vista.GetType().GetField("_txtEmail_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(vista);
                        Check(campoEmail.Text == "••••••••" && botonEmail.Enabled, "Selected donor email stays masked beside the reveal button");
                        eventoEmail.Invoke(vista, new object[] { botonEmail, EventArgs.Empty });
                        Check(campoEmail.Text == emailNuevo, "Eye action reveals the selected donor email in the form");
                        eventoEmail.Invoke(vista, new object[] { botonEmail, EventArgs.Empty });
                        Check(campoEmail.Text == "••••••••", "Eye action hides the email again");
                    }
                }
            }
            using (var pantalla = new MainForm_380_jh())
            {
                pantalla.GetType().GetMethod("MainForm_Load_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(pantalla, new object[] { pantalla, EventArgs.Empty });
                pantalla.PerformLayout();
                Check(true, "Main form load without integrity status chrome");
                var menu = (MenuStrip)pantalla.GetType().GetField("menuStrip1_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(pantalla);
                bool integridadEnMenu = false;
                foreach (ToolStripItem item in menu.Items)
                    integridadEnMenu = integridadEnMenu || string.Equals(item.Tag as string, "MENU_INTEGRITY", StringComparison.Ordinal);
                bool barraEstadoVisible = false;
                foreach (Control control in pantalla.Controls)
                    barraEstadoVisible = barraEstadoVisible || control is StatusStrip;
                Check(!integridadEnMenu && !barraEstadoVisible, "Integrity tab and status strip are absent from main navigation");
            }
            using (var unidades = new UnidadesView_380_jh())
            {
                Sql("UPDATE dbo.Unidad SET fecha_vencimiento='00010101' WHERE id_unidad=1");
                Check(!servicio.Verificar_380_jh().EsValida_380_jh, "Application maps corruption");
                using (var pantalla = new MainForm_380_jh())
                {
                    pantalla.GetType().GetMethod("MainForm_Load_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(pantalla, new object[] { pantalla, EventArgs.Empty });
                    var panelContenido = (Panel)pantalla.GetType().GetField("contentPanel_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(pantalla);
                    Check(panelContenido.Controls.Count == 1 && panelContenido.Controls[0] is IntegridadView_380_jh,
                        "Administrator is routed to the repair screen when integrity is broken");
                }
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
    private static string SqlString(string sql)
    {
        using (var cn = new SqlConnection(conexion))
        using (var cmd = new SqlCommand(sql, cn))
        {
            cn.Open();
            object value = cmd.ExecuteScalar();
            return value == null || value == DBNull.Value ? null : value.ToString();
        }
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception("FAIL " + message);
        Console.WriteLine("OK " + message);
    }
}
