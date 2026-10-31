using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Domain;
using Repository;
using Services;

namespace Application
{
    public class ResultadoComprobantes_380_jh
    {
        public List<string> Archivos_380_jh { get; } = new List<string>();
        public int Pendientes_380_jh { get; set; }
        public bool ErrorConsulta_380_jh { get; set; }
    }

    public class ComprobanteAutomaticoApplicationService_380_jh
    {
        private readonly ComprobanteRepository_380_jh _repositorio = new ComprobanteRepository_380_jh();
        private readonly string _directorioRaiz;

        public ComprobanteAutomaticoApplicationService_380_jh() { }
        public ComprobanteAutomaticoApplicationService_380_jh(string directorioRaiz) { _directorioRaiz = directorioRaiz; }

        public bool Disponible_380_jh()
        {
            try { return _repositorio.Disponible_380_jh(); }
            catch { return false; }
        }

        public string ObtenerOriginal_380_jh(string proceso, int idReferencia)
        {
            Usuario_380_jh usuario = Sesion_380_jh.ObtenerInstancia_380_jh().ObtenerUsuario_380_jh();
            if (usuario == null || usuario.Id_380_jh <= 0 || idReferencia <= 0) throw new InvalidOperationException();
            return _repositorio.ObtenerOriginal_380_jh(proceso, idReferencia, usuario.Id_380_jh);
        }

        public ResultadoComprobantes_380_jh ProcesarPendientes_380_jh()
        {
            ResultadoComprobantes_380_jh resultado = new ResultadoComprobantes_380_jh();
            Usuario_380_jh usuario = Sesion_380_jh.ObtenerInstancia_380_jh().ObtenerUsuario_380_jh();
            if (usuario == null || usuario.Id_380_jh <= 0) { resultado.ErrorConsulta_380_jh = true; return resultado; }
            InformeIntegridad_380_jh integridad = new IntegridadApplicationService_380_jh().Verificar_380_jh();
            if (!integridad.Disponible_380_jh || !integridad.EsValida_380_jh) { resultado.ErrorConsulta_380_jh = true; return resultado; }
            List<ComprobantePendiente_380_jh> pendientes;
            try { pendientes = _repositorio.ListarPendientes_380_jh(usuario.Id_380_jh); }
            catch { resultado.ErrorConsulta_380_jh = true; return resultado; }
            foreach (ComprobantePendiente_380_jh pendiente in pendientes)
            {
                string temporal = null;
                try
                {
                    DatosComprobanteAutomatico_380_jh datos;
                    XmlReaderSettings ajustes = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
                    using (StringReader texto = new StringReader(pendiente.DatosXml_380_jh))
                    using (XmlReader lector = XmlReader.Create(texto, ajustes))
                        datos = (DatosComprobanteAutomatico_380_jh)new XmlSerializer(typeof(DatosComprobanteAutomatico_380_jh)).Deserialize(lector);
                    if (datos == null || datos.Tipo != pendiente.Proceso_380_jh) throw new InvalidDataException();
                    string carpeta = Path.Combine(string.IsNullOrWhiteSpace(_directorioRaiz) ? DirectorioRaiz_380_jh() : Path.GetFullPath(_directorioRaiz), pendiente.Proceso_380_jh);
                    Directory.CreateDirectory(carpeta);
                    string ruta = Path.Combine(carpeta, pendiente.Proceso_380_jh + "_" + pendiente.IdReferencia_380_jh + "_" + pendiente.Identificador_380_jh.ToString("N") + ".pdf");
                    if (!File.Exists(ruta))
                    {
                        temporal = Path.Combine(carpeta, "." + Guid.NewGuid().ToString("N") + ".tmp");
                        ComprobantePdfService_380_jh pdf = new ComprobantePdfService_380_jh();
                        if (datos.Tipo == "PN1") pdf.GuardarPN1_380_jh(temporal, datos.Unidades, datos.Usuario, true, pendiente.Identificador_380_jh.ToString("D"));
                        else if (datos.Tipo == "PN2" && datos.Solicitud != null)
                            pdf.GuardarPN2_380_jh(temporal, datos.Solicitud, datos.Entregas, datos.Usuario, pendiente.IdReferencia_380_jh, true, pendiente.Identificador_380_jh.ToString("D"));
                        else throw new InvalidDataException();
                        try { File.Move(temporal, ruta); temporal = null; }
                        catch (IOException) { if (!File.Exists(ruta)) throw; }
                    }
                    _repositorio.MarcarGenerado_380_jh(pendiente.Id_380_jh, usuario.Id_380_jh, ruta);
                    resultado.Archivos_380_jh.Add(ruta);
                }
                catch { resultado.Pendientes_380_jh++; }
                finally { if (temporal != null) { try { File.Delete(temporal); } catch { } } }
            }
            return resultado;
        }

        public static string DirectorioRaiz_380_jh()
        {
            string configurada = ConfigurationManager.AppSettings["ComprobantesDirectorio"];
            if (string.IsNullOrWhiteSpace(configurada))
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HemovidaGest", "Comprobantes");
            return Path.GetFullPath(Environment.ExpandEnvironmentVariables(configurada.Trim()));
        }
    }
}
