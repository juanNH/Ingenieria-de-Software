using System;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using Application;
using DAL;
using Domain;
using Services;
using UI;

internal static class TipificacionDonanteUiTests
{
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 2 || !Regex.IsMatch(args[1], "^CodexDonanteTest_[a-f0-9]{12}$"))
                throw new InvalidOperationException("Solo se permite una base temporal de pruebas.");
            var config = XDocument.Load(args[0]);
            var connection = new SqlConnectionStringBuilder((string)config.Root.Element("connectionStrings").Element("add").Attribute("connectionString"));
            connection.InitialCatalog = args[1];
            typeof(DatabaseContext_380_jh).GetField("ConnectionString_380_jh", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, connection.ConnectionString);
            var user = new Usuario_380_jh { Id_380_jh = 1, Username_380_jh = "admin", Estado_380_jh = "ACTIVO" };
            user.ComponentesPermiso_380_jh = new PermisoDataMapper_380_jh().ListarAsignadosPorUsuario_380_jh(user.Id_380_jh);
            Sesion_380_jh.ObtenerInstancia_380_jh().IniciarSesion_380_jh(user);
            LanguageManager_380_jh.Instance_380_jh.Initialize_380_jh(user);
            var donors = new DonanteApplicationService_380_jh();
            var donor = new Donante_380_jh { Documento_380_jh = "UI-DONANTE", Nombre_380_jh = "Ana", Apellido_380_jh = "Prueba" };
            Check(donors.Registrar_380_jh(donor) == CodigoOperacionPN1_380_jh.Ok_380_jh, "Alta de donante mediante Application y DAL");
            var donations = new DonacionApplicationService_380_jh();
            var units = new UnidadApplicationService_380_jh();
            var first = Donate(donations, donor.Id_380_jh);
            first.GrupoSanguineo_380_jh = "A"; first.FactorRh_380_jh = "+";
            Check(units.Clasificar_380_jh(first) == CodigoOperacionPN1_380_jh.Ok_380_jh, "Clasificacion mediante Application y DAL");
            var saved = donors.Listar_380_jh().Single(d => d.Id_380_jh == donor.Id_380_jh);
            Check(saved.GrupoSanguineo_380_jh == null && saved.FactorRh_380_jh == null, "La clasificacion provisional no confirma la ficha");
            first.GrupoSanguineo_380_jh = "O"; first.FactorRh_380_jh = "-";
            Check(units.Clasificar_380_jh(first) == CodigoOperacionPN1_380_jh.Ok_380_jh, "El servicio admite corregir en revision");
            Check(units.CambiarEstado_380_jh(first, EstadoUnidadPN1_380_jh.Liberada_380_jh, null) == CodigoOperacionPN1_380_jh.Ok_380_jh,
                "La primera liberacion confirma el resultado corregido");
            saved = donors.Listar_380_jh().Single(d => d.Id_380_jh == donor.Id_380_jh);
            Check(saved.GrupoSanguineo_380_jh == "O" && saved.FactorRh_380_jh == "-", "El mapper recupera la tipificacion persistida");
            var next = Donate(donations, donor.Id_380_jh);
            Check(next.GrupoSanguineo_380_jh == "O" && next.FactorRh_380_jh == "-" && !next.EstaClasificada_380_jh,
                "Alta de segunda donacion devuelve grupo y Rh guardados pendientes de confirmar");
            next = units.Listar_380_jh().Single(u => u.Id_380_jh == next.Id_380_jh);
            Check(next.GrupoSanguineo_380_jh == "O" && next.FactorRh_380_jh == "-" && !next.EstaClasificada_380_jh,
                "La segunda unidad conserva la tipificacion y sigue pendiente al recargar");
            using (var view = new DonantesView_380_jh())
            {
                var row = Field<DataGridView>(view, "_grid_380_jh").Rows.Cast<DataGridViewRow>().Single(r => ((Donante_380_jh)r.Tag).Id_380_jh == donor.Id_380_jh);
                Check(Convert.ToString(row.Cells["grupo"].Value) == "O" && Convert.ToString(row.Cells["rh"].Value) == "-", "Pantalla de donantes muestra grupo y Rh");
            }
            using (var view = new UnidadesView_380_jh())
            {
                var grid = Field<DataGridView>(view, "_grid_380_jh");
                grid.ClearSelection();
                var row = grid.Rows.Cast<DataGridViewRow>().Single(r => ((Unidad_380_jh)r.Tag).Id_380_jh == next.Id_380_jh);
                Check(Convert.ToString(row.Cells["grupo"].Value) == "O" && Convert.ToString(row.Cells["rh"].Value) == "-",
                    "La grilla muestra el grupo y Rh de la segunda unidad");
                row.Selected = true;
                view.GetType().GetMethod("Grid_SelectionChanged_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(view, new object[] { grid, EventArgs.Empty });
                Check(((MaestroPN1_380_jh)Field<ComboBox>(view, "_cmbGrupo_380_jh").SelectedItem).Codigo_380_jh == "O" &&
                    ((MaestroPN1_380_jh)Field<ComboBox>(view, "_cmbRh_380_jh").SelectedItem).Codigo_380_jh == "-", "Formulario propone grupo y Rh del donante");
                Check(!next.EstaClasificada_380_jh, "Proponer datos en pantalla no clasifica la muestra");
            }
            next.GrupoSanguineo_380_jh = "A"; next.FactorRh_380_jh = "-";
            Check(units.Clasificar_380_jh(next) == CodigoOperacionPN1_380_jh.Ok_380_jh, "Se puede guardar una discrepancia provisional");
            Check(units.CambiarEstado_380_jh(next, EstadoUnidadPN1_380_jh.Liberada_380_jh, null) == CodigoOperacionPN1_380_jh.ConflictoTipificacion_380_jh,
                "El mapper informa la discrepancia al intentar liberar");
            string message = LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("UNIT_DONOR_BLOOD_CONFLICT");
            Check(!string.IsNullOrWhiteSpace(message) && message != "UNIT_DONOR_BLOOD_CONFLICT", "Mensaje de conflicto traducido");
            next.GrupoSanguineo_380_jh = "O";
            Check(units.Clasificar_380_jh(next) == CodigoOperacionPN1_380_jh.Ok_380_jh, "Confirmar nueva muestra coincidente");
            Check(next.EstaClasificada_380_jh, "El mapper reconoce la confirmacion");
            var masters = new MaestroPN1ApplicationService_380_jh();
            var group = masters.Listar_380_jh(MaestroPN1ApplicationService_380_jh.Grupo_380_jh).Single(g => g.Codigo_380_jh == "O");
            group.Activo_380_jh = false;
            Check(masters.Guardar_380_jh(MaestroPN1ApplicationService_380_jh.Grupo_380_jh, group) == "OK", "Desactivar un grupo ya clasificado");
            using (var view = new UnidadesView_380_jh())
            {
                var grid = Field<DataGridView>(view, "_grid_380_jh");
                grid.ClearSelection();
                var row = grid.Rows.Cast<DataGridViewRow>().Single(r => ((Unidad_380_jh)r.Tag).Id_380_jh == next.Id_380_jh);
                row.Selected = true;
                view.GetType().GetMethod("Grid_SelectionChanged_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(view, new object[] { grid, EventArgs.Empty });
                var groups = Field<ComboBox>(view, "_cmbGrupo_380_jh");
                var requiresSave = view.GetType().GetMethod("RequiereGuardarTipificacion_380_jh", BindingFlags.Instance | BindingFlags.NonPublic);
                Check(groups.SelectedIndex == -1 && !(bool)requiresSave.Invoke(view, new[] { row.Tag }),
                    "Un grupo guardado que ya no figura en el maestro no cuenta como edicion");
                groups.SelectedItem = groups.Items.Cast<MaestroPN1_380_jh>().Single(g => g.Codigo_380_jh == "A");
                Check((bool)requiresSave.Invoke(view, new[] { row.Tag }), "Una correccion sin guardar bloquea la liberacion");
            }
            Check(units.CambiarEstado_380_jh(next, EstadoUnidadPN1_380_jh.Liberada_380_jh, null) == CodigoOperacionPN1_380_jh.Ok_380_jh,
                "La clasificacion guardada permite liberar con el grupo desactivado");
            if (user.ComponentesPermiso_380_jh != null && new AutorizacionApplicationService_380_jh().TienePermiso_380_jh(PermisosSistema_380_jh.DonacionAjustarCantidad_380_jh))
                ComprobarAjusteCantidad(donations, donor.Id_380_jh, first.IdDonacion_380_jh);
            Check(new IntegridadApplicationService_380_jh().Verificar_380_jh().EsValida_380_jh, "Circuito completo conserva integridad");
            Console.WriteLine("Pruebas UI/Application/DAL de tipificacion completadas.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

    private static void ComprobarAjusteCantidad(DonacionApplicationService_380_jh service, int idDonante, int cerrada)
    {
        int idDonacion;
        var dialogType = typeof(DonacionesView_380_jh).Assembly.GetType("UI.CantidadDonacionDialog_380_jh", true);
        using (var vista = new DonacionesView_380_jh())
        {
            var nueva = new Donacion_380_jh { IdDonante_380_jh = idDonante, FechaDonacion_380_jh = DateTime.Now, CantidadUnidades_380_jh = 2,
                TipoComponente_380_jh = "Concentrado de eritrocitos", FechaVencimiento_380_jh = DateTime.Today.AddDays(30) };
            Check(service.Registrar_380_jh(nueva) == CodigoOperacionPN1_380_jh.Ok_380_jh, "Registrar donacion para el ajuste");
            idDonacion = nueva.Id_380_jh;
            var selector = Field<ComboBox>(vista, "_cmbDonacionRegistrada_380_jh");
            var grilla = Field<DataGridView>(vista, "_gridUnidades_380_jh");
            vista.GetType().GetMethod("MostrarDonacionRegistrada_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(vista, new object[] { nueva });
            vista.Size = new System.Drawing.Size(950, 550);
            vista.OnLanguageChanged_380_jh(LanguageManager_380_jh.Instance_380_jh.CurrentLanguage_380_jh);
            vista.PerformLayout();
            var historial = Field<Button>(vista, "_btnCantidadHistorial_380_jh");
            Check(((Donacion_380_jh)selector.SelectedItem).Id_380_jh == idDonacion && grilla.Rows.Count == 2 &&
                  historial.Enabled && historial.Parent.ClientRectangle.Contains(historial.Bounds),
                "Donaciones deja seleccionada la donacion recien registrada con acceso visible a su historial");
            var datos = service.ConsultarCantidad_380_jh(idDonacion);
            Check(datos.Editable_380_jh && datos.Ajustables_380_jh.Count == 2, "El formulario recibe la version y las unidades ajustables");
            using (var dialog = (Form)Activator.CreateInstance(dialogType, service, idDonacion, datos))
            {
                var unidades = Field<CheckedListBox>(dialog, "_unidades");
                Check(unidades.CheckedItems.Count == 2, "La lista marca las unidades vigentes al abrirse");
                unidades.SetItemChecked(0, false);
                Field<TextBox>(dialog, "_motivo").Text = "Excluir una unidad desde el formulario";
                dialogType.GetMethod("Guardar_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(dialog, new object[] { dialog, EventArgs.Empty });
                Check(dialog.DialogResult == DialogResult.OK, "El formulario ejecuta el ajuste a traves de Application y DAL");
            }
            vista.GetType().GetMethod("CargarDonaciones_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(vista, new object[] { idDonacion });
            Check(((Donacion_380_jh)selector.SelectedItem).CantidadUnidades_380_jh == 1 && grilla.Rows.Count == 2,
                "Donaciones muestra la cantidad ajustada y conserva las dos unidades");
        }
        var guardados = service.ConsultarCantidad_380_jh(idDonacion);
        Check(guardados.Cantidad_380_jh == 1 && guardados.Historial_380_jh.Count == 2 && guardados.Historial_380_jh[0].Anterior_380_jh == 2 &&
              guardados.Historial_380_jh[0].Codigos_380_jh == guardados.Ajustables_380_jh[0].CodigoIdentificacion_380_jh,
              "La consulta recupera el cambio y el codigo original");
        using (var dialog = (Form)Activator.CreateInstance(dialogType, service, idDonacion, guardados))
        {
            var unidades = Field<CheckedListBox>(dialog, "_unidades");
            Check(!unidades.GetItemChecked(0) && unidades.GetItemChecked(1), "La unidad anulada aparece desmarcada al volver a abrir el historial");
            unidades.SetItemChecked(0, true);
            unidades.SetItemChecked(1, false);
            Field<TextBox>(dialog, "_motivo").Text = "Cambiar la unidad vigente";
            dialogType.GetMethod("Guardar_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(dialog, new object[] { dialog, EventArgs.Empty });
            Check(dialog.DialogResult == DialogResult.OK, "La seleccion permite cambiar que unidad queda vigente sin cambiar la cantidad");
        }
        var intercambiadas = service.ConsultarCantidad_380_jh(idDonacion);
        Check(intercambiadas.Cantidad_380_jh == 1 && intercambiadas.Unidades_380_jh.Select(u => u.Id_380_jh).SequenceEqual(guardados.Unidades_380_jh.Select(u => u.Id_380_jh)) &&
              intercambiadas.Unidades_380_jh[0].EstadoOperativo_380_jh == EstadoUnidadPN1_380_jh.EnRevision_380_jh &&
              intercambiadas.Unidades_380_jh[1].EstadoOperativo_380_jh == EstadoUnidadPN1_380_jh.Anulada_380_jh,
              "El intercambio conserva los identificadores y cambia solamente que unidades quedan vigentes");
        using (var vista = new DonacionesView_380_jh())
        {
            var selector = Field<ComboBox>(vista, "_cmbDonacionRegistrada_380_jh");
            selector.SelectedItem = selector.Items.Cast<Donacion_380_jh>().Single(d => d.Id_380_jh == cerrada);
            Check(((Donacion_380_jh)selector.SelectedItem).Id_380_jh == cerrada, "Se puede volver a una donacion anterior al reabrir Donaciones");
        }
        var datosCerrados = service.ConsultarCantidad_380_jh(cerrada);
        using (var dialog = (Form)Activator.CreateInstance(dialogType, service, cerrada, datosCerrados))
            Check(!datosCerrados.Editable_380_jh && Field<TextBox>(dialog, "_motivo").Parent == null, "Una donacion liberada solo permite consultar el historial");
    }

    private static Unidad_380_jh Donate(DonacionApplicationService_380_jh service, int donor)
    {
        var donation = new Donacion_380_jh { IdDonante_380_jh = donor, FechaDonacion_380_jh = DateTime.Now, CantidadUnidades_380_jh = 1,
            TipoComponente_380_jh = "Concentrado de eritrocitos", FechaVencimiento_380_jh = DateTime.Today.AddDays(30) };
        Check(service.Registrar_380_jh(donation) == CodigoOperacionPN1_380_jh.Ok_380_jh, "Registrar donacion mediante Application y DAL");
        return donation.Unidades_380_jh.Single();
    }
    private static T Field<T>(object view, string name) { return (T)view.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view); }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        Console.WriteLine("OK " + message);
    }
}
