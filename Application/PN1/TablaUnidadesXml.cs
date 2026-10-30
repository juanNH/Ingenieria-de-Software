using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Application
{
    [XmlRoot("TablaUnidadesPN1")]
    public class TablaUnidadesXml_380_jh
    {
        [XmlAttribute("version")]
        public string Version_380_jh { get; set; }

        [XmlElement("Unidad")]
        public List<FilaUnidadXml_380_jh> Unidades_380_jh { get; set; } = new List<FilaUnidadXml_380_jh>();
    }

    public class FilaUnidadXml_380_jh
    {
        [XmlElement("CodigoIdentificacion")]
        public string CodigoIdentificacion_380_jh { get; set; }
        [XmlElement("Donante")]
        public string Donante_380_jh { get; set; }
        [XmlElement("TipoComponente")]
        public string TipoComponente_380_jh { get; set; }
        [XmlElement("GrupoSanguineo")]
        public string GrupoSanguineo_380_jh { get; set; }
        [XmlElement("FactorRh")]
        public string FactorRh_380_jh { get; set; }
        [XmlElement("FechaVencimiento", DataType = "date")]
        public DateTime FechaVencimiento_380_jh { get; set; }
        [XmlElement("EstadoOperativo")]
        public string EstadoOperativo_380_jh { get; set; }
    }
}
