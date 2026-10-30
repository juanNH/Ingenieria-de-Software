using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Domain;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace Services
{
    // El documento es una fotografía de los datos consultados, no un registro de cierre.
    public class ComprobantePdfService_380_jh
    {
        public void GuardarPN1_380_jh(string archivo, IList<Unidad_380_jh> unidades, string usuario)
        {
            if (unidades == null || unidades.Count == 0 || unidades.Any(u => u.IdDonacion_380_jh != unidades[0].IdDonacion_380_jh))
                throw new ArgumentException("Se requiere una donación con unidades guardadas.");
            Unidad_380_jh primera = unidades[0];
            using (Documento_380_jh documento = new Documento_380_jh("Comprobante de proceso de donación", "PN1 · Donación " + primera.IdDonacion_380_jh, usuario))
            {
                documento.Campo_380_jh("Donación", primera.IdDonacion_380_jh.ToString());
                documento.Campo_380_jh("Donante", primera.Donante_380_jh);
                documento.Campo_380_jh("Documento del donante", primera.DocumentoDonante_380_jh);
                documento.Campo_380_jh("Unidades registradas", unidades.Count.ToString());
                documento.Seccion_380_jh("Unidades de la donación");
                foreach (Unidad_380_jh unidad in unidades.OrderBy(u => u.CodigoIdentificacion_380_jh))
                {
                    documento.Subtitulo_380_jh("Unidad " + unidad.CodigoIdentificacion_380_jh);
                    documento.Campo_380_jh("Componente", unidad.TipoComponente_380_jh);
                    documento.Campo_380_jh("Grupo y Rh", (unidad.GrupoSanguineo_380_jh ?? "") + " " + (unidad.FactorRh_380_jh ?? ""));
                    documento.Campo_380_jh("Vencimiento", unidad.FechaVencimiento_380_jh.ToString("dd/MM/yyyy"));
                    documento.Campo_380_jh("Estado actual", unidad.EstadoOperativo_380_jh);
                    documento.Campo_380_jh("Registro de la unidad", unidad.FechaAlta_380_jh.ToString("dd/MM/yyyy HH:mm"));
                    documento.Campo_380_jh("Última modificación", unidad.FechaUltimaModificacion_380_jh.ToString("dd/MM/yyyy HH:mm"));
                    documento.Campo_380_jh("Usuario de última modificación (ID)", unidad.IdUsuarioUltimaModificacion_380_jh.ToString());
                    documento.Campo_380_jh("Observaciones", unidad.Observaciones_380_jh);
                }
                documento.Guardar_380_jh(archivo);
            }
        }

        public void GuardarPN2_380_jh(string archivo, SolicitudTransfusional_380_jh solicitud,
            IList<Entrega_380_jh> entregas, string usuario, int? idEntrega = null)
        {
            if (solicitud == null || entregas == null || entregas.Any(e => e.IdSolicitudTransfusional_380_jh != solicitud.Id_380_jh))
                throw new ArgumentException("Se requiere una solicitud y sus entregas guardadas.");
            List<Entrega_380_jh> seleccion = entregas.Where(e => !idEntrega.HasValue || e.Id_380_jh == idEntrega.Value).OrderBy(e => e.FechaEntrega_380_jh).ToList();
            if (idEntrega.HasValue && seleccion.Count != 1) throw new ArgumentException("Entrega inexistente.");
            string titulo = idEntrega.HasValue ? "Comprobante de entrega" : "Comprobante de atención de solicitud";
            string referencia = idEntrega.HasValue ? "PN2 · Entrega " + idEntrega.Value : "PN2 · Solicitud " + solicitud.Id_380_jh;
            using (Documento_380_jh documento = new Documento_380_jh(titulo, referencia, usuario))
            {
                documento.Campo_380_jh("Solicitud", solicitud.Id_380_jh.ToString());
                documento.Campo_380_jh("Fecha de solicitud", solicitud.FechaSolicitud_380_jh.ToString("dd/MM/yyyy HH:mm"));
                documento.Campo_380_jh("Paciente receptor", solicitud.PacienteReceptor_380_jh);
                documento.Campo_380_jh("Profesional solicitante", solicitud.ProfesionalSolicitante_380_jh);
                documento.Campo_380_jh("Componente solicitado", solicitud.TipoComponente_380_jh);
                documento.Campo_380_jh("Estado de solicitud", solicitud.EstadoSolicitud_380_jh);
                documento.Campo_380_jh("Requeridas / reservadas / entregadas / pendientes",
                    string.Format("{0} / {1} / {2} / {3}", solicitud.CantidadRequerida_380_jh, solicitud.CantidadReservada_380_jh, solicitud.CantidadEntregada_380_jh, solicitud.SaldoPendiente_380_jh));
                documento.Seccion_380_jh(idEntrega.HasValue ? "Entrega seleccionada" : "Historial de entregas");
                if (seleccion.Count == 0) documento.Texto_380_jh("No hay entregas registradas.");
                foreach (Entrega_380_jh entrega in seleccion)
                {
                    documento.Subtitulo_380_jh("Entrega " + entrega.Id_380_jh + " · Unidad " + entrega.CodigoIdentificacion_380_jh);
                    documento.Campo_380_jh("Reserva", entrega.IdReserva_380_jh.ToString());
                    documento.Campo_380_jh("Fecha de entrega", entrega.FechaEntrega_380_jh.ToString("dd/MM/yyyy HH:mm"));
                    documento.Campo_380_jh("Responsable de entrega", entrega.Responsable_380_jh);
                    documento.Campo_380_jh("Componente", entrega.TipoComponente_380_jh);
                    documento.Campo_380_jh("Observación de entrega", entrega.Observacion_380_jh);
                    documento.Campo_380_jh("Destino actual", string.IsNullOrWhiteSpace(entrega.Destino_380_jh) ? "Sin registrar" : entrega.Destino_380_jh);
                    if (entrega.FechaDestino_380_jh.HasValue) documento.Campo_380_jh("Fecha de destino", entrega.FechaDestino_380_jh.Value.ToString("dd/MM/yyyy HH:mm"));
                    if (entrega.IdUsuarioDestino_380_jh.HasValue) documento.Campo_380_jh("Usuario de destino (ID)", entrega.IdUsuarioDestino_380_jh.Value.ToString());
                    documento.Campo_380_jh("Motivo de destino", entrega.MotivoDestino_380_jh);
                }
                documento.Guardar_380_jh(archivo);
            }
        }

        private sealed class Documento_380_jh : IDisposable
        {
            private readonly PdfDocument _pdf = new PdfDocument();
            private readonly XFont _normal = new XFont("Arial", 10, XFontStyle.Regular, new XPdfFontOptions(PdfFontEncoding.Unicode));
            private readonly XFont _negrita = new XFont("Arial", 10, XFontStyle.Bold, new XPdfFontOptions(PdfFontEncoding.Unicode));
            private readonly XFont _titulo = new XFont("Arial", 16, XFontStyle.Bold, new XPdfFontOptions(PdfFontEncoding.Unicode));
            private readonly string _nombre;
            private readonly string _referencia;
            private readonly DateTime _emitido = DateTime.Now;
            private XGraphics _grafico;
            private double _y;

            public Documento_380_jh(string nombre, string referencia, string usuario)
            {
                _nombre = nombre; _referencia = referencia;
                _pdf.Info.Title = nombre;
                NuevaPagina_380_jh();
                Campo_380_jh("Emitido", _emitido.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture));
                Campo_380_jh("Generado por", usuario);
                Texto_380_jh("Datos registrados al momento de la emisión. Los estados pueden cambiar posteriormente.");
            }

            private void NuevaPagina_380_jh()
            {
                if (_grafico != null) _grafico.Dispose();
                PdfPage pagina = _pdf.AddPage();
                pagina.Size = PdfSharp.PageSize.A4;
                _grafico = XGraphics.FromPdfPage(pagina);
                _y = 42;
                _grafico.DrawString("HEMOVIDA GEST", _negrita, XBrushes.DarkRed, 42, _y);
                _y += 26;
                _grafico.DrawString(_nombre, _titulo, XBrushes.Black, 42, _y);
                _y += 20;
                _grafico.DrawString(_referencia, _normal, XBrushes.DimGray, 42, _y);
                _y += 16;
                _grafico.DrawLine(XPens.LightGray, 42, _y, 553, _y);
                _y += 18;
            }

            public void Campo_380_jh(string etiqueta, string valor)
            {
                TextoConFuente_380_jh(etiqueta + ": " + (string.IsNullOrWhiteSpace(valor) ? "—" : valor.Trim()), _normal);
            }
            public void Texto_380_jh(string texto) { TextoConFuente_380_jh(texto, _normal); }
            public void Seccion_380_jh(string texto) { _y += 10; TextoConFuente_380_jh(texto, _negrita); _y += 4; }
            public void Subtitulo_380_jh(string texto) { _y += 7; TextoConFuente_380_jh(texto, _negrita); }

            private void TextoConFuente_380_jh(string texto, XFont fuente)
            {
                // Mide cada línea y corta incluso palabras largas; conserva todo el contenido.
                string limpio = (texto ?? "").Replace('\r', ' ').Replace('\n', ' ');
                StringBuilder linea = new StringBuilder();
                foreach (char caracter in limpio)
                {
                    linea.Append(caracter);
                    if (_grafico.MeasureString(linea.ToString(), fuente).Width > 510 && linea.Length > 1)
                    {
                        char ultimo = linea[linea.Length - 1];
                        linea.Length--;
                        ImprimirLinea_380_jh(linea.ToString(), fuente);
                        linea.Clear(); linea.Append(ultimo);
                    }
                }
                if (linea.Length > 0) ImprimirLinea_380_jh(linea.ToString(), fuente);
            }

            private void ImprimirLinea_380_jh(string linea, XFont fuente)
            {
                if (_y > 790) NuevaPagina_380_jh();
                _grafico.DrawString(linea, fuente, XBrushes.Black, 42, _y);
                _y += 15;
            }

            public void Guardar_380_jh(string archivo)
            {
                if (_grafico != null) { _grafico.Dispose(); _grafico = null; }
                _pdf.Save(archivo);
            }
            public void Dispose() { if (_grafico != null) _grafico.Dispose(); _pdf.Dispose(); }
        }
    }
}
