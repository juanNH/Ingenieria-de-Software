using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Application
{
    [XmlRoot("ProcesoPN1")]
    public class ProcesoPN1Xml_380_jh
    {
        [XmlAttribute("version")]
        public string VersionEsquema_380_jh { get; set; }

        [XmlElement("ExportadoEnUtc")]
        public DateTime ExportadoEnUtc_380_jh { get; set; }

        [XmlElement("Donacion")]
        public DonacionProcesoPN1Xml_380_jh Donacion_380_jh { get; set; }
    }

    public class DonacionProcesoPN1Xml_380_jh
    {
        [XmlAttribute("id")]
        public int Id_380_jh { get; set; }

        [XmlAttribute("cantidadUnidades")]
        public int CantidadUnidades_380_jh { get; set; }

        [XmlElement("Donante")]
        public DonanteProcesoXml_380_jh Donante_380_jh { get; set; }

        [XmlArray("Unidades")]
        [XmlArrayItem("Unidad")]
        public List<UnidadProcesoXml_380_jh> Unidades_380_jh { get; set; }
    }

    public class DonanteProcesoXml_380_jh
    {
        [XmlAttribute("id")]
        public int Id_380_jh { get; set; }

        [XmlElement("Nombre")]
        public string Nombre_380_jh { get; set; }
    }

    public class UnidadProcesoXml_380_jh
    {
        [XmlAttribute("id")]
        public int Id_380_jh { get; set; }

        [XmlElement("CodigoIdentificacion")]
        public string CodigoIdentificacion_380_jh { get; set; }

        [XmlElement("TipoComponente")]
        public string TipoComponente_380_jh { get; set; }

        [XmlElement("GrupoSanguineo")]
        public string GrupoSanguineo_380_jh { get; set; }

        [XmlElement("FactorRh")]
        public string FactorRh_380_jh { get; set; }

        [XmlElement("FechaVencimiento")]
        public DateTime FechaVencimiento_380_jh { get; set; }

        [XmlElement("EstadoOperativo")]
        public string EstadoOperativo_380_jh { get; set; }

        [XmlElement("Observaciones")]
        public string Observaciones_380_jh { get; set; }

        [XmlElement("FechaAlta")]
        public DateTime FechaAlta_380_jh { get; set; }

        [XmlElement("FechaUltimaModificacion")]
        public DateTime FechaUltimaModificacion_380_jh { get; set; }
    }
}
