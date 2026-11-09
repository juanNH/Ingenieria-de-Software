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

internal static class ReceptoresTests
{
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 3 || !Regex.IsMatch(args[1], "^CodexPN2Check_[a-f0-9]{12}$"))
                throw new InvalidOperationException("Only the isolated PN2 test database is allowed.");
            var connection = new SqlConnectionStringBuilder { DataSource = args[0], InitialCatalog = args[1], IntegratedSecurity = true };
            typeof(DatabaseContext_380_jh).GetField("ConnectionString_380_jh", BindingFlags.Static | BindingFlags.NonPublic)
                .SetValue(null, connection.ConnectionString);
            var user = new Usuario_380_jh { Id_380_jh = int.Parse(args[2]), Username_380_jh = "prueba_pn2", Estado_380_jh = "ACTIVO" };
            user.ComponentesPermiso_380_jh = new PermisoDataMapper_380_jh().ListarAsignadosPorUsuario_380_jh(user.Id_380_jh);
            Sesion_380_jh.ObtenerInstancia_380_jh().IniciarSesion_380_jh(user);
            LanguageManager_380_jh.Instance_380_jh.Initialize_380_jh(user);
            var service = new PN2ApplicationService_380_jh();
            var saved = service.ListarReceptores_380_jh("REUSE-1").Single();
            Check(saved.GrupoSanguineo_380_jh == "A" && saved.FactorRh_380_jh == "+", "Application/DAL retrieve the recipient within the user's scope");
            var request = new SolicitudTransfusional_380_jh {
                PacienteReceptor_380_jh = saved.Nombre_380_jh, DocumentoReceptor_380_jh = saved.Documento_380_jh,
                GrupoReceptor_380_jh = saved.GrupoSanguineo_380_jh, RhReceptor_380_jh = saved.FactorRh_380_jh,
                ProfesionalSolicitante_380_jh = "Profesional", TipoComponente_380_jh = "Plasma", CantidadRequerida_380_jh = 1
            };
            Check(service.RegistrarSolicitud_380_jh(request) == CodigoOperacionPN2_380_jh.Ok_380_jh,
                "Application registers another request with the saved group and Rh without an old study");
            Check(service.ListarReceptores_380_jh("REUSE-1").Count == 1, "A new service query still returns one recipient per document");
            request.LaboratorioEstudio_380_jh = "Incomplete study";
            Check(service.RegistrarSolicitud_380_jh(request) == CodigoOperacionPN2_380_jh.DatosInvalidos_380_jh,
                "Application continues to reject incomplete studies");

            using (var form = new Form { ClientSize = new Size(1100, 740), Location = new Point(-30000, -30000),
                StartPosition = FormStartPosition.Manual, ShowInTaskbar = false })
            using (var view = new SolicitudesTransfusionalesView_380_jh { Dock = DockStyle.Fill })
            {
                form.Controls.Add(view); form.Show(); System.Windows.Forms.Application.DoEvents();
                var combo = Field<ComboBox>(view, "_cmbReceptor_380_jh");
                var document = Field<TextBox>(view, "_txtDocumento_380_jh");
                var name = Field<TextBox>(view, "_txtPaciente_380_jh");
                var group = Field<ComboBox>(view, "_cmbGrupo_380_jh");
                var rh = Field<ComboBox>(view, "_cmbRh_380_jh");
                var grid = Field<DataGridView>(view, "_grid_380_jh");
                Check(combo.Items.Cast<Receptor_380_jh>().Count(r => r.Documento_380_jh == "REUSE-1") == 1, "UI includes the saved recipient once");
                Check(grid.SelectedRows.Count == 0 && document.Text == "", "Initial form is ready for a new request");
                document.Text = " REUSE-1 "; Leave(document);
                Check(name.Text == saved.Nombre_380_jh && Selected(group) == "A" && Selected(rh) == "+", "Entering a saved document retrieves name, group and Rh");

                var historical = grid.Rows.Cast<DataGridViewRow>().Single(r => ((SolicitudTransfusional_380_jh)r.Tag).DocumentoReceptor_380_jh == "DOC2");
                historical.Selected = true;
                Check(Field<TextBox>(view, "_txtLaboratorio_380_jh").Text == "Lab", "Selecting a historical request loads its own study");
                Leave(document);
                Check(grid.SelectedRows.Count == 1 && Field<TextBox>(view, "_txtLaboratorio_380_jh").Text == "Lab",
                    "Leaving the document of a historical request preserves the study being edited");
                combo.SelectedItem = combo.Items.Cast<Receptor_380_jh>().Single(r => r.Documento_380_jh == "REUSE-1");
                typeof(ComboBox).GetMethod("OnSelectionChangeCommitted", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(combo, new object[] { EventArgs.Empty });
                Check(document.Text == "REUSE-1" && Selected(group) == "A" && Selected(rh) == "+", "Selecting a recipient restores its saved identity and blood type");
                Check(grid.SelectedRows.Count == 0 && Field<TextBox>(view, "_txtLaboratorio_380_jh").Text == "" &&
                    Field<TextBox>(view, "_txtReferencia_380_jh").Text == "" && Field<TextBox>(view, "_txtAnticuerpos_380_jh").Text == "" &&
                    !Field<DateTimePicker>(view, "_fechaEstudio_380_jh").Checked, "Reusing a recipient starts a new request without copying study or validation");
                document.Text = "UNREGISTERED";
                Check(combo.SelectedIndex == -1 && name.Text == "" && group.SelectedIndex == -1 && rh.SelectedIndex == -1,
                    "Changing identity removes the previous recipient's blood type");
                document.Text = "REUSE-1"; Leave(document);
                Check(Selected(group) == "A" && Selected(rh) == "+", "The saved profile survives form changes");
                var english = LanguageManager_380_jh.Instance_380_jh.ListarIdiomasActivos_380_jh().Single(i => i.Codigo_380_jh == "en-US");
                LanguageManager_380_jh.Instance_380_jh.ChangeLanguage_380_jh(english, null);
                Check(Controls(view).OfType<Label>().Single(l => (string)l.Tag == "PN2_REGISTERED_RECIPIENT").Text == "Registered recipient",
                    "New recipient controls have English translations");
                Check(document.Text == "REUSE-1" && Selected(group) == "A" && grid.SelectedRows.Count == 0,
                    "Changing language preserves the new request and saved recipient");
                var spanish = LanguageManager_380_jh.Instance_380_jh.ListarIdiomasActivos_380_jh().Single(i => i.Codigo_380_jh == "es-AR");
                LanguageManager_380_jh.Instance_380_jh.ChangeLanguage_380_jh(spanish, null);
                var snapshot = Environment.GetEnvironmentVariable("PN2_RECIPIENT_SCREENSHOT");
                if (!string.IsNullOrWhiteSpace(snapshot))
                    using (var bitmap = new Bitmap(form.ClientSize.Width, form.ClientSize.Height))
                    { form.DrawToBitmap(bitmap, form.ClientRectangle); bitmap.Save(snapshot); }
                Controls(view).OfType<Button>().Single(b => (string)b.Tag == "PN2_NEW_RECIPIENT").PerformClick();
                Check(document.Text == "" && name.Text == "" && group.SelectedIndex == -1 && rh.SelectedIndex == -1 &&
                    combo.SelectedIndex == -1 && grid.SelectedRows.Count == 0, "New recipient clears the identity and request selection");
            }
            Check(new IntegridadApplicationService_380_jh().Verificar_380_jh().EsValida_380_jh, "Integrity remains valid after reusing recipients");

            // Conservar la cuenta de SQL permite comprobar que la vista decide qué datos cargar.
            var soloAlta = new Usuario_380_jh { Id_380_jh = user.Id_380_jh, Estado_380_jh = "ACTIVO" };
            soloAlta.ComponentesPermiso_380_jh.Add(new Permiso_380_jh { Codigo_380_jh = PermisosSistema_380_jh.SolicitudCrear_380_jh });
            Sesion_380_jh.ObtenerInstancia_380_jh().IniciarSesion_380_jh(soloAlta);
            using (var view = new SolicitudesTransfusionalesView_380_jh())
            {
                view.CreateControl();
                Check(Field<Button>(view, "_btnRegistrar_380_jh").Visible, "Registration remains available with create-only permission");
                var recipients = Field<ComboBox>(view, "_cmbReceptor_380_jh");
                Check(!recipients.Enabled && recipients.Items.Count == 0 && Field<DataGridView>(view, "_grid_380_jh").Rows.Count == 0,
                    "Create-only view does not display existing requests or recipients");
            }
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

    private static T Field<T>(object instance, string name) => (T)instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance);
    private static IEnumerable<Control> Controls(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            yield return child;
            foreach (Control nested in Controls(child)) yield return nested;
        }
    }
    private static string Selected(ComboBox combo) => (combo.SelectedItem as MaestroPN1_380_jh)?.Codigo_380_jh;
    private static void Leave(Control control) => typeof(Control).GetMethod("OnLeave", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(control, new object[] { EventArgs.Empty });
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        Console.WriteLine("OK " + message);
    }
}
