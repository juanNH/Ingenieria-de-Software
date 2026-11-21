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
            {
                ComprobarAjusteCantidad(donations, donor.Id_380_jh, first.IdDonacion_380_jh);
                ComprobarNavegacionDonaciones(donations, donor.Id_380_jh);
            }
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
        var unica = Donate(service, idDonante);
        var datosUnica = service.ConsultarCantidad_380_jh(unica.IdDonacion_380_jh);
        Check(datosUnica.Editable_380_jh && datosUnica.Ajustables_380_jh.Single().Id_380_jh == unica.Id_380_jh,
            "La donacion de una unidad sigue abierta y la unidad esta en revision");
        using (var dialog = (Form)Activator.CreateInstance(dialogType, service, unica.IdDonacion_380_jh, datosUnica))
        {
            var historial = Field<DataGridView>(dialog, "_historial");
            Check(historial.ReadOnly && historial.Rows.Count == 1 &&
                  historial.Rows[0].Tag is CambioCantidadDonacion_380_jh cambio && cambio.Anterior_380_jh == null && cambio.Nueva_380_jh == 1,
                "El historial de una sola unidad conserva el registro inicial");
            var nuevas = Field<NumericUpDown>(dialog, "_unidadesNuevas");
            Check(nuevas.Parent != null && nuevas.Value == 0 && Field<CheckedListBox>(dialog, "_unidades").CheckedItems.Count == 1,
                "La donacion abierta permite agregar unidades de manera explicita");
            nuevas.Value = 2;
            Field<TextBox>(dialog, "_motivo").Text = "Agregar dos unidades omitidas";
            dialogType.GetMethod("Guardar_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(dialog, new object[] { dialog, EventArgs.Empty });
            Check(dialog.DialogResult == DialogResult.OK, "La pantalla guarda una correccion de una a tres unidades");
        }
        var ampliada = service.ConsultarCantidad_380_jh(unica.IdDonacion_380_jh);
        Check(ampliada.Cantidad_380_jh == 3 && ampliada.Unidades_380_jh.Count == 3 &&
              ampliada.Unidades_380_jh[0].CodigoIdentificacion_380_jh == unica.CodigoIdentificacion_380_jh &&
              ampliada.Unidades_380_jh.All(u => u.EstadoOperativo_380_jh == EstadoUnidadPN1_380_jh.EnRevision_380_jh) &&
              ampliada.Historial_380_jh.Count == 2 && ampliada.Historial_380_jh[0].Anterior_380_jh == 1 &&
              ampliada.Historial_380_jh[0].Nueva_380_jh == 3,
            "La correccion conserva el codigo original, agrega dos unidades en revision y registra el cambio de una a tres");
        using (var dialog = (Form)Activator.CreateInstance(dialogType, service, unica.IdDonacion_380_jh, ampliada))
        {
            var historial = Field<DataGridView>(dialog, "_historial");
            var filaInicial = historial.Rows.Cast<DataGridViewRow>()
                .Single(fila => ((CambioCantidadDonacion_380_jh)fila.Tag).Version_380_jh == datosUnica.Version_380_jh);
            historial.CurrentCell = filaInicial.Cells[0];
            var restaurar = Field<Button>(dialog, "_restaurar");
            Check(restaurar.Enabled, "Seleccionar el registro inicial habilita restaurar la cantidad");
            dialogType.GetMethod("RestaurarSeleccion_380_jh", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(dialog, new object[] { restaurar, EventArgs.Empty });
            var nuevas = Field<NumericUpDown>(dialog, "_unidadesNuevas");
            var unidades = Field<CheckedListBox>(dialog, "_unidades");
            Check(!nuevas.Enabled && nuevas.Value == 0 &&
                  unidades.CheckedItems.Cast<Unidad_380_jh>().Single().Id_380_jh == unica.Id_380_jh,
                "La restauracion propone dejar vigente la unidad original y anular las unidades agregadas");
            Field<TextBox>(dialog, "_motivo").Text = "Restaurar la cantidad del registro inicial";
            dialogType.GetMethod("Guardar_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(dialog, new object[] { dialog, EventArgs.Empty });
            Check(dialog.DialogResult == DialogResult.OK, "La pantalla restaura la cantidad seleccionada en el historial");
        }
        var restaurada = service.ConsultarCantidad_380_jh(unica.IdDonacion_380_jh);
        Check(restaurada.Cantidad_380_jh == 1 && restaurada.Historial_380_jh.Count == 3 &&
              restaurada.Historial_380_jh[0].Accion_380_jh == "RESTAURA_CANTIDAD" && restaurada.Historial_380_jh[0].Anterior_380_jh == 3 &&
              restaurada.Historial_380_jh[0].Nueva_380_jh == 1 &&
              restaurada.Unidades_380_jh.Select(u => u.Id_380_jh).SequenceEqual(ampliada.Unidades_380_jh.Select(u => u.Id_380_jh)) &&
              restaurada.Unidades_380_jh.Single(u => u.EstadoOperativo_380_jh != EstadoUnidadPN1_380_jh.Anulada_380_jh).Id_380_jh == unica.Id_380_jh,
            "La restauracion registra el cambio de tres a una y conserva las tres unidades y sus identificadores");
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
        var unidadesService = new UnidadApplicationService_380_jh();
        var procesoXml = XDocument.Parse(unidadesService.ExportarProcesoXml_380_jh(intercambiadas.Unidades_380_jh[0]));
        var donacionXml = procesoXml.Root.Element("Donacion");
        Check((int)donacionXml.Attribute("cantidadUnidades") == 1 && donacionXml.Element("Unidades").Elements("Unidad").Count() == 2,
            "El XML de la donacion declara solo la cantidad vigente y conserva las dos unidades");
        using (var dialog = (Form)Activator.CreateInstance(dialogType, service, idDonacion, intercambiadas))
        {
            var unidades = Field<CheckedListBox>(dialog, "_unidades");
            unidades.SetItemChecked(1, true);
            Field<TextBox>(dialog, "_motivo").Text = "Reactivar la segunda unidad";
            dialogType.GetMethod("Guardar_380_jh", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(dialog, new object[] { dialog, EventArgs.Empty });
            Check(dialog.DialogResult == DialogResult.OK, "Se puede pasar de una a dos unidades reactivando la anulada");
        }
        var reactivadas = service.ConsultarCantidad_380_jh(idDonacion);
        Check(reactivadas.Cantidad_380_jh == 2 && reactivadas.Unidades_380_jh.Select(u => u.Id_380_jh).SequenceEqual(guardados.Unidades_380_jh.Select(u => u.Id_380_jh)) &&
              reactivadas.Historial_380_jh[0].Anterior_380_jh == 1 && reactivadas.Historial_380_jh[0].Nueva_380_jh == 2,
            "El historial registra la reactivacion conservando los identificadores originales");
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

    private static void ComprobarNavegacionDonaciones(DonacionApplicationService_380_jh service, int idDonante)
    {
        using (var formulario = new MainForm_380_jh())
        {
            var menuDonaciones = Field<ToolStripMenuItem>(formulario, "_donacionesToolStripMenuItem_380_jh");
            var menuDonantes = Field<ToolStripMenuItem>(formulario, "_donantesToolStripMenuItem_380_jh");
            var panel = Field<Panel>(formulario, "contentPanel_380_jh");
            menuDonaciones.PerformClick();
            var vistaInicial = panel.Controls.OfType<DonacionesView_380_jh>().Single();

            var donacion = new Donacion_380_jh
            {
                IdDonante_380_jh = idDonante,
                FechaDonacion_380_jh = DateTime.Now,
                CantidadUnidades_380_jh = 1,
                TipoComponente_380_jh = "Concentrado de eritrocitos",
                FechaVencimiento_380_jh = DateTime.Today.AddDays(30)
            };
            Check(service.Registrar_380_jh(donacion) == CodigoOperacionPN1_380_jh.Ok_380_jh,
                "El administrador registra una donacion antes de cambiar de pantalla");
            typeof(DonacionesView_380_jh).GetMethod("MostrarDonacionRegistrada_380_jh", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(vistaInicial, new object[] { donacion });
            var selectorInicial = Field<ComboBox>(vistaInicial, "_cmbDonacionRegistrada_380_jh");
            Check(((Donacion_380_jh)selectorInicial.SelectedItem).Id_380_jh == donacion.Id_380_jh,
                "El registro confirmado deja seleccionada la donacion persistida");

            menuDonantes.PerformClick();
            Check(vistaInicial.IsDisposed && panel.Controls.OfType<DonantesView_380_jh>().Count() == 1,
                "El menu Donantes reemplaza y dispone la vista de donaciones");
            var posterior = Donate(service, idDonante);
            menuDonaciones.PerformClick();
            var vistaRecargada = panel.Controls.OfType<DonacionesView_380_jh>().Single();
            var selector = Field<ComboBox>(vistaRecargada, "_cmbDonacionRegistrada_380_jh");
            var grilla = Field<DataGridView>(vistaRecargada, "_gridUnidades_380_jh");
            Check(((Donacion_380_jh)selector.SelectedItem).Id_380_jh == donacion.Id_380_jh &&
                  selector.Items.OfType<Donacion_380_jh>().Any(item => item.Id_380_jh == posterior.IdDonacion_380_jh),
                "Al volver se recarga la lista desde SQL y se conserva la seleccion aunque haya una donacion posterior");
            Check(grilla.Rows.Count == 1 && Convert.ToString(grilla.Rows[0].Cells["codigo_identificacion"].Value) ==
                  donacion.Unidades_380_jh.Single().CodigoIdentificacion_380_jh,
                "La grilla vuelve a mostrar la unidad persistida de la donacion seleccionada");
        }
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
