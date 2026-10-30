using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using Application;
using DAL;
using Domain;
using Repository;
using Services;
using UI;

internal static class PN1XmlTests
{
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length == 1 && args[0] == "--lectura-base") return VerificarDatosBase();
            var serializer = new XmlSerializerService_380_jh();
            var importer = new ProcesoPN1XmlImportService_380_jh(serializer);
            var now = DateTime.UtcNow;
            var proceso = new ProcesoPN1Xml_380_jh
            {
                VersionEsquema_380_jh = "1.0", ExportadoEnUtc_380_jh = now,
                Donacion_380_jh = new DonacionProcesoPN1Xml_380_jh
                {
                    Id_380_jh = 1, CantidadUnidades_380_jh = 1,
                    Donante_380_jh = new DonanteProcesoXml_380_jh { Id_380_jh = 1, Nombre_380_jh = "Donante de prueba" },
                    Unidades_380_jh = new List<UnidadProcesoXml_380_jh> { new UnidadProcesoXml_380_jh
                    {
                        Id_380_jh = 1, CodigoIdentificacion_380_jh = "UN-1", TipoComponente_380_jh = "Sangre",
                        FechaVencimiento_380_jh = now.AddDays(10), EstadoOperativo_380_jh = "EN_REVISION",
                        FechaAlta_380_jh = now, FechaUltimaModificacion_380_jh = now
                    } }
                }
            };
            string xml = serializer.Serialize_380_jh(proceso);
            Check(Importar(importer, xml).Donacion_380_jh.Unidades_380_jh.Count == 1, "Exportar y deserializar conserva la unidad");

            XDocument document = XDocument.Parse(xml);
            XElement unidades = document.Root.Element("Donacion").Element("Unidades");
            XElement extra = new XElement(unidades.Element("Unidad"));
            extra.SetAttributeValue("id", "2");
            extra.Element("CodigoIdentificacion").Value = "UN-FICTICIA";
            unidades.Add(extra);
            string modificado = document.ToString();
            var preview = Importar(importer, modificado);
            Check(preview.Donacion_380_jh.Unidades_380_jh.Count == 2 && preview.Donacion_380_jh.CantidadUnidades_380_jh == 1 &&
                preview.Donacion_380_jh.Unidades_380_jh[0].CodigoIdentificacion_380_jh == "UN-1" &&
                preview.Donacion_380_jh.Unidades_380_jh[1].CodigoIdentificacion_380_jh == "UN-FICTICIA",
                "Importa unidad agregada aunque la cantidad declarada no se haya actualizado");
            extra.SetAttributeValue("id", "1");
            Rechazado(importer, document.ToString(), "Rechaza identificadores repetidos");
            extra.SetAttributeValue("id", "2");
            extra.Element("CodigoIdentificacion").Value = "UN-1";
            Rechazado(importer, document.ToString(), "Rechaza códigos repetidos");
            extra.Element("CodigoIdentificacion").Value = "UN-2";
            extra.Element("EstadoOperativo").Value = "INVENTADO";
            Rechazado(importer, document.ToString(), "Rechaza estado inválido");
            extra.Element("EstadoOperativo").Value = "LIBERADA";
            Rechazado(importer, document.ToString(), "Rechaza unidad final sin clasificación");
            extra.Element("EstadoOperativo").Value = "EN_REVISION";
            extra.Element("FechaVencimiento").Remove();
            Rechazado(importer, document.ToString(), "Rechaza campo obligatorio ausente");
            Rechazado(importer, "<!DOCTYPE ProcesoPN1 [<!ENTITY x 'datos'>]><ProcesoPN1>&x;</ProcesoPN1>", "Rechaza DTD");
            Rechazado(importer, "<otro />", "Rechaza raíz incorrecta");
            XDocument versionInvalida = XDocument.Parse(xml);
            versionInvalida.Root.SetAttributeValue("version", "2.0");
            Rechazado(importer, versionInvalida.ToString(), "Rechaza versión no admitida");

            // Evita cualquier conexión a una base real durante la prueba de interfaz.
            typeof(DatabaseContext_380_jh).GetField("ConnectionString_380_jh", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, "");
            Sesion_380_jh.ObtenerInstancia_380_jh().Logout_380_jh();
            try
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
                    new UnidadApplicationService_380_jh().ImportarProcesoXml_380_jh(stream);
                throw new Exception("Se permitió importar sin permiso.");
            }
            catch (UnauthorizedAccessException) { Check(true, "El servicio exige permiso de consulta"); }
            try
            {
                new UnidadApplicationService_380_jh().SerializarTablaXml_380_jh(new List<FilaUnidadXml_380_jh>());
                throw new Exception("Se permitió serializar sin permiso.");
            }
            catch (UnauthorizedAccessException) { Check(true, "Serializar la tabla exige permiso de consulta"); }
            var usuario = new Usuario_380_jh { Id_380_jh = 1, Estado_380_jh = "ACTIVO" };
            usuario.ComponentesPermiso_380_jh.Add(new Permiso_380_jh { Codigo_380_jh = PermisosSistema_380_jh.UnidadVer_380_jh });
            Sesion_380_jh.ObtenerInstancia_380_jh().IniciarSesion_380_jh(usuario);
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(modificado)))
                Check(new UnidadApplicationService_380_jh().ImportarProcesoXml_380_jh(stream).Donacion_380_jh.Unidades_380_jh.Count == 2,
                    "El servicio autorizado importa la vista previa sin consultar ni escribir la base");
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(modificado)))
                Check(new UnidadApplicationService_380_jh().DeserializarTablaXml_380_jh(stream).Unidades_380_jh.Count == 2,
                    "El nuevo importador acepta archivos exportados anteriormente");
            XDocument archivoAnterior = XDocument.Parse(xml);
            XElement unidadAnterior = archivoAnterior.Root.Element("Donacion").Element("Unidades").Element("Unidad");
            unidadAnterior.Element("TipoComponente").AddAfterSelf(new XElement("GrupoSanguineo", "O"), new XElement("FactorRh", "+"));
            unidadAnterior.Element("EstadoOperativo").Value = "TRANSFUNDIDA";
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(archivoAnterior.ToString())))
                Check(new UnidadApplicationService_380_jh().DeserializarTablaXml_380_jh(stream).Unidades_380_jh[0].EstadoOperativo_380_jh == "TRANSFUNDIDA",
                    "El formato anterior admite unidades transfundidas");
            VerificarInterfaz();
            Console.WriteLine("Pruebas XML PN1 completadas.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

    private static int VerificarDatosBase()
    {
        var unidades = new UnidadRepository_380_jh().Listar_380_jh(null);
        if (unidades.Count == 0) throw new Exception("No hay unidades disponibles para el diagnóstico de lectura.");
        using (var grid = new DataGridView { AllowUserToAddRows = false })
        {
            foreach (string nombre in new[] { "codigo", "donante", "componente", "grupo", "rh", "vencimiento", "estado" })
                grid.Columns.Add(nombre, nombre);
            foreach (var unidad in unidades)
            {
                int i = grid.Rows.Add(unidad.CodigoIdentificacion_380_jh, unidad.Donante_380_jh,
                    unidad.TipoComponente_380_jh, unidad.GrupoSanguineo_380_jh, unidad.FactorRh_380_jh,
                    unidad.FechaVencimiento_380_jh.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), unidad.EstadoOperativo_380_jh);
                grid.Rows[i].Tag = unidad;
            }
            var vista = (UnidadesView_380_jh)FormatterServices.GetUninitializedObject(typeof(UnidadesView_380_jh));
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(UnidadesView_380_jh).GetField("_grid_380_jh", flags).SetValue(vista, grid);
            var filas = (List<FilaUnidadXml_380_jh>)typeof(UnidadesView_380_jh).GetMethod("ObtenerFilasVisibles_380_jh", flags).Invoke(vista, null);
            var serializer = new TablaUnidadesXmlService_380_jh(new XmlSerializerService_380_jh());
            string xml = serializer.Serializar_380_jh(filas);
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                var recuperadas = serializer.Deserializar_380_jh(stream);
                Check(recuperadas.Unidades_380_jh.Count == unidades.Count, "Serializa y recupera todas las unidades consultadas en la base");
                for (int i = 0; i < unidades.Count; i++)
                    Check(recuperadas.Unidades_380_jh[i].EstadoOperativo_380_jh == unidades[i].EstadoOperativo_380_jh &&
                        recuperadas.Unidades_380_jh[i].FechaVencimiento_380_jh.Date == unidades[i].FechaVencimiento_380_jh.Date &&
                        recuperadas.Unidades_380_jh[i].CodigoIdentificacion_380_jh == unidades[i].CodigoIdentificacion_380_jh &&
                        recuperadas.Unidades_380_jh[i].Donante_380_jh == unidades[i].Donante_380_jh &&
                        recuperadas.Unidades_380_jh[i].TipoComponente_380_jh == unidades[i].TipoComponente_380_jh &&
                        (recuperadas.Unidades_380_jh[i].GrupoSanguineo_380_jh ?? "") == (unidades[i].GrupoSanguineo_380_jh ?? "") &&
                        (recuperadas.Unidades_380_jh[i].FactorRh_380_jh ?? "") == (unidades[i].FactorRh_380_jh ?? ""),
                        "Conserva los datos visibles de la fila " + (i + 1));
            }
            XDocument documento = XDocument.Parse(xml);
            XElement ficticia = new XElement(documento.Root.Element("Unidad"));
            ficticia.Element("CodigoIdentificacion").Value = "PRUEBA-XML-" + Guid.NewGuid().ToString("N");
            ficticia.Element("Donante").Value = "Donante ficticio";
            documento.Root.Add(ficticia);
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(documento.ToString())))
                Check(serializer.Deserializar_380_jh(stream).Unidades_380_jh.Count == unidades.Count + 1,
                    "Recupera los datos de la base junto a una fila ficticia agregada al XML");
        }
        return 0;
    }

    private static void VerificarInterfaz()
    {
        using (var vista = new UnidadesView_380_jh())
        {
            vista.CreateControl();
            var tipo = vista.GetType();
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var grid = (DataGridView)tipo.GetField("_grid_380_jh", flags).GetValue(vista);
            var unidades = (List<Unidad_380_jh>)tipo.GetField("_unidades_380_jh", flags).GetValue(vista);
            unidades.Add(new Unidad_380_jh { Id_380_jh = 1, IdDonacion_380_jh = 1, CodigoIdentificacion_380_jh = "UN-1", Donante_380_jh = "Donante A", TipoComponente_380_jh = "Sangre", FechaVencimiento_380_jh = new DateTime(2030, 1, 2), EstadoOperativo_380_jh = "EN_REVISION" });
            unidades.Add(new Unidad_380_jh { Id_380_jh = 2, IdDonacion_380_jh = 2, CodigoIdentificacion_380_jh = "UN-2", Donante_380_jh = "Donante B", TipoComponente_380_jh = "Plasma", GrupoSanguineo_380_jh = "O", FactorRh_380_jh = "+", FechaVencimiento_380_jh = new DateTime(2031, 3, 4), EstadoOperativo_380_jh = "TRANSFUNDIDA" });
            tipo.GetMethod("PintarGrilla_380_jh", flags).Invoke(vista, null);
            Check(Convert.ToString(grid.Rows[1].Cells["estado"].Value) == LanguageManager_380_jh.Instance_380_jh.Translate_380_jh("PN2_STATE_TRANSFUNDIDA"),
                "La grilla identifica correctamente una unidad transfundida");
            CultureInfo culturaAnterior = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
                tipo.GetMethod("PintarGrilla_380_jh", flags).Invoke(vista, null);
                var fechas = (List<FilaUnidadXml_380_jh>)tipo.GetMethod("ObtenerFilasVisibles_380_jh", flags).Invoke(vista, null);
                Check(fechas[0].FechaVencimiento_380_jh == new DateTime(2030, 1, 2), "Serializa fechas con un separador regional diferente");
            }
            finally { Thread.CurrentThread.CurrentCulture = culturaAnterior; }
            grid.Rows[0].Cells["componente"].Value = "Visible & correcto";
            grid.ClearSelection();
            var filas = (List<FilaUnidadXml_380_jh>)tipo.GetMethod("ObtenerFilasVisibles_380_jh", flags).Invoke(vista, null);
            Check(filas.Count == 2 && filas[0].TipoComponente_380_jh == "Visible & correcto", "Captura todas las filas visibles de distintas donaciones sin selección");
            grid.Rows[1].Visible = false;
            Check(((List<FilaUnidadXml_380_jh>)tipo.GetMethod("ObtenerFilasVisibles_380_jh", flags).Invoke(vista, null)).Count == 1,
                "No serializa las filas ocultas");
            grid.Rows[1].Visible = true;
            var servicio = new UnidadApplicationService_380_jh();
            string xml = servicio.SerializarTablaXml_380_jh(filas);
            foreach (string estado in new[] { "RESERVADA", "ENTREGADA", "TRANSFUNDIDA", "DEVUELTA" })
            {
                var filaEstado = new FilaUnidadXml_380_jh
                {
                    CodigoIdentificacion_380_jh = "ESTADO", Donante_380_jh = "Donante", TipoComponente_380_jh = "Sangre",
                    GrupoSanguineo_380_jh = "O", FactorRh_380_jh = "+", FechaVencimiento_380_jh = new DateTime(2030, 1, 2),
                    EstadoOperativo_380_jh = estado
                };
                string xmlEstado = servicio.SerializarTablaXml_380_jh(new List<FilaUnidadXml_380_jh> { filaEstado });
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlEstado)))
                    Check(servicio.DeserializarTablaXml_380_jh(stream).Unidades_380_jh[0].EstadoOperativo_380_jh == estado,
                        "Serializa y recupera el estado " + estado);
            }
            XDocument documento = XDocument.Parse(xml);
            Check(documento.Root.Name.LocalName == "TablaUnidadesPN1" && documento.Root.Elements("Unidad").Count() == 2,
                "Serializa la tabla completa en XML");
            string archivoTemporal = Path.Combine(Path.GetTempPath(), "pn1-tabla-" + Guid.NewGuid().ToString("N") + ".xml");
            try
            {
                File.WriteAllText(archivoTemporal, xml, new UTF8Encoding(false));
                using (var archivo = File.OpenRead(archivoTemporal))
                    Check(servicio.DeserializarTablaXml_380_jh(archivo).Unidades_380_jh.Count == 2,
                        "El archivo UTF-8 guardado se puede abrir y deserializar");
            }
            finally { if (File.Exists(archivoTemporal)) File.Delete(archivoTemporal); }

            tipo.GetMethod("LimpiarXml_Click_380_jh", flags).Invoke(vista, new object[] { null, EventArgs.Empty });
            Check(grid.Rows.Count == 0 && unidades.Count == 0, "Limpiar vacía la grilla y los datos locales");
            var split = (SplitContainer)tipo.GetField("_split_380_jh", flags).GetValue(vista);
            Check(!split.Panel2.Enabled, "Limpiar desactiva las acciones operativas");

            XElement extra = new XElement(documento.Root.Element("Unidad"));
            extra.Element("CodigoIdentificacion").Value = "UN-FICTICIA";
            extra.Element("Donante").Value = "Donante ficticio";
            documento.Root.Add(extra);
            TablaUnidadesXml_380_jh tabla;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(documento.ToString())))
                tabla = servicio.DeserializarTablaXml_380_jh(stream);
            tipo.GetMethod("MostrarTablaXml_380_jh", flags).Invoke(vista, new object[] { tabla });
            Check(grid.Rows.Count == 3 && Convert.ToString(grid.Rows[2].Cells["codigo"].Value) == "UN-FICTICIA" &&
                Convert.ToString(grid.Rows[0].Cells["componente"].Value) == "Visible & correcto", "Deserializa filas originales y fila agregada en la misma grilla");
            Check(!split.Panel2.Enabled && tipo.GetMethod("ObtenerSeleccionada_380_jh", flags).Invoke(vista, null) == null,
                "Las filas importadas no habilitan operaciones de negocio");
            var filasImportadas = (List<FilaUnidadXml_380_jh>)tipo.GetMethod("ObtenerFilasVisibles_380_jh", flags).Invoke(vista, null);
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(servicio.SerializarTablaXml_380_jh(filasImportadas))))
                Check(servicio.DeserializarTablaXml_380_jh(stream).Unidades_380_jh.Count == 3, "La tabla importada también se puede volver a serializar");
            extra.Element("CodigoIdentificacion").Value = "UN-1";
            try
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(documento.ToString()))) servicio.DeserializarTablaXml_380_jh(stream);
                throw new Exception("Se permitió importar un código repetido.");
            }
            catch (InvalidDataException) { Check(grid.Rows.Count == 3, "Rechaza duplicados sin alterar la grilla mostrada"); }
            RechazadoTabla(servicio, "<!DOCTYPE TablaUnidadesPN1 [<!ENTITY x 'datos'>]><TablaUnidadesPN1>&x;</TablaUnidadesPN1>", "Rechaza DTD en formato de tabla");
            XDocument versionInvalida = XDocument.Parse(xml);
            versionInvalida.Root.SetAttributeValue("version", "2.0");
            RechazadoTabla(servicio, versionInvalida.ToString(), "Rechaza versión inválida de tabla");
            XDocument sinCampo = XDocument.Parse(xml);
            sinCampo.Root.Element("Unidad").Element("FechaVencimiento").Remove();
            RechazadoTabla(servicio, sinCampo.ToString(), "Rechaza fecha ausente en tabla");
            tipo.GetMethod("LimpiarXml_Click_380_jh", flags).Invoke(vista, new object[] { null, EventArgs.Empty });
            Check(grid.Rows.Count == 0, "Limpiar también vacía las filas importadas");
            tipo.GetMethod("CargarUnidades_380_jh", flags).Invoke(vista, null);
            Check(split.Panel2.Enabled, "Actualizar sale del modo temporal y recupera el panel operativo");
        }
    }

    private static void RechazadoTabla(UnidadApplicationService_380_jh servicio, string xml, string mensaje)
    {
        try { using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml))) servicio.DeserializarTablaXml_380_jh(stream); }
        catch (Exception ex) when (ex is InvalidOperationException || ex is InvalidDataException || ex is System.Xml.XmlException) { Check(true, mensaje); return; }
        throw new Exception("No se rechazó: " + mensaje);
    }

    private static ProcesoPN1Xml_380_jh Importar(ProcesoPN1XmlImportService_380_jh importer, string xml)
    {
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml))) return importer.Importar_380_jh(stream);
    }

    private static void Rechazado(ProcesoPN1XmlImportService_380_jh importer, string xml, string mensaje)
    {
        try { Importar(importer, xml); }
        catch (Exception ex) when (ex is InvalidOperationException || ex is InvalidDataException || ex is System.Xml.XmlException) { Check(true, mensaje); return; }
        throw new Exception("No se rechazó: " + mensaje);
    }

    private static void Check(bool condicion, string mensaje)
    {
        if (!condicion) throw new Exception("FALLÓ: " + mensaje);
        Console.WriteLine("OK: " + mensaje);
    }
}
