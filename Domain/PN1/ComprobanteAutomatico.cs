using System.Collections.Generic;
using System;
using System.Xml.Serialization;

namespace Domain
{
    [XmlRoot("Comprobante")]
    public class DatosComprobanteAutomatico_380_jh
    {
        public string Tipo { get; set; }
        public string Usuario { get; set; }
        [XmlArray("Unidades"), XmlArrayItem("Unidad")]
        public List<Unidad_380_jh> Unidades { get; set; }
        public SolicitudTransfusional_380_jh Solicitud { get; set; }
        [XmlArray("Entregas"), XmlArrayItem("Entrega")]
        public List<Entrega_380_jh> Entregas { get; set; }
    }

    public class ComprobantePendiente_380_jh
    {
        public long Id_380_jh { get; set; }
        public Guid Identificador_380_jh { get; set; }
        public string Proceso_380_jh { get; set; }
        public int IdReferencia_380_jh { get; set; }
        public string DatosXml_380_jh { get; set; }
    }
}
