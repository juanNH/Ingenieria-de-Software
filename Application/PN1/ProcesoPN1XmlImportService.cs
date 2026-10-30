using System;
using System.Collections.Generic;
using System.IO;
using Abstractions;
using Domain;

namespace Application
{
    public class ProcesoPN1XmlImportService_380_jh
    {
        private readonly IXmlSerializer_380_jh _serializer_380_jh;

        public ProcesoPN1XmlImportService_380_jh(IXmlSerializer_380_jh serializer)
        {
            _serializer_380_jh = serializer ?? throw new ArgumentNullException("serializer");
        }

        // El atributo cantidadUnidades describe el archivo original. Se calcula la
        // cantidad mostrada a partir de las unidades efectivamente importadas.
        public ProcesoPN1Xml_380_jh Importar_380_jh(Stream stream)
        {
            ProcesoPN1Xml_380_jh proceso = _serializer_380_jh.Deserialize_380_jh<ProcesoPN1Xml_380_jh>(stream, Esquema_380_jh);
            if (proceso == null || proceso.Donacion_380_jh == null || proceso.Donacion_380_jh.Donante_380_jh == null ||
                proceso.ExportadoEnUtc_380_jh == default(DateTime) ||
                string.IsNullOrWhiteSpace(proceso.Donacion_380_jh.Donante_380_jh.Nombre_380_jh) ||
                proceso.Donacion_380_jh.Unidades_380_jh == null || proceso.Donacion_380_jh.Unidades_380_jh.Count == 0)
                throw new InvalidDataException("Faltan datos obligatorios del proceso.");

            HashSet<int> ids = new HashSet<int>();
            HashSet<string> codigos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (UnidadProcesoXml_380_jh unidad in proceso.Donacion_380_jh.Unidades_380_jh)
            {
                if (unidad == null || unidad.Id_380_jh <= 0 || string.IsNullOrWhiteSpace(unidad.CodigoIdentificacion_380_jh) ||
                    unidad.CodigoIdentificacion_380_jh.Length > 50 || string.IsNullOrWhiteSpace(unidad.TipoComponente_380_jh) ||
                    unidad.TipoComponente_380_jh.Length > 100 || (unidad.Observaciones_380_jh != null && unidad.Observaciones_380_jh.Length > 500) ||
                    unidad.FechaVencimiento_380_jh == default(DateTime) || unidad.FechaAlta_380_jh == default(DateTime) ||
                    unidad.FechaUltimaModificacion_380_jh == default(DateTime) ||
                    !ids.Add(unidad.Id_380_jh) || !codigos.Add(unidad.CodigoIdentificacion_380_jh.Trim()))
                    throw new InvalidDataException("Unidad inválida o identificador/código duplicado.");

                bool tieneGrupo = !string.IsNullOrWhiteSpace(unidad.GrupoSanguineo_380_jh);
                bool tieneRh = !string.IsNullOrWhiteSpace(unidad.FactorRh_380_jh);
                if (tieneGrupo != tieneRh ||
                    (tieneGrupo && unidad.GrupoSanguineo_380_jh != "A" && unidad.GrupoSanguineo_380_jh != "B" && unidad.GrupoSanguineo_380_jh != "AB" && unidad.GrupoSanguineo_380_jh != "O") ||
                    (tieneRh && unidad.FactorRh_380_jh != "+" && unidad.FactorRh_380_jh != "-") ||
                    (unidad.EstadoOperativo_380_jh != EstadoUnidadPN1_380_jh.EnRevision_380_jh && !tieneGrupo))
                    throw new InvalidDataException("Clasificación de unidad inválida.");
            }
            return proceso;
        }

        private const string Esquema_380_jh = @"<?xml version='1.0' encoding='utf-8'?>
<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' elementFormDefault='qualified'>
  <xs:element name='ProcesoPN1'><xs:complexType><xs:sequence>
    <xs:element name='ExportadoEnUtc' type='xs:dateTime'/>
    <xs:element name='Donacion'><xs:complexType><xs:sequence>
      <xs:element name='Donante'><xs:complexType><xs:sequence>
        <xs:element name='Nombre' type='xs:string' minOccurs='0'/>
      </xs:sequence><xs:attribute name='id' type='xs:positiveInteger' use='required'/></xs:complexType></xs:element>
      <xs:element name='Unidades'><xs:complexType><xs:sequence>
        <xs:element name='Unidad' minOccurs='1' maxOccurs='unbounded'><xs:complexType><xs:sequence>
          <xs:element name='CodigoIdentificacion' type='xs:string'/>
          <xs:element name='TipoComponente' type='xs:string'/>
          <xs:element name='GrupoSanguineo' type='xs:string' minOccurs='0'/>
          <xs:element name='FactorRh' type='xs:string' minOccurs='0'/>
          <xs:element name='FechaVencimiento' type='xs:dateTime'/>
          <xs:element name='EstadoOperativo'><xs:simpleType><xs:restriction base='xs:string'>
            <xs:enumeration value='EN_REVISION'/><xs:enumeration value='LIBERADA'/><xs:enumeration value='BLOQUEADA'/><xs:enumeration value='DESCARTADA'/>
            <xs:enumeration value='RESERVADA'/><xs:enumeration value='ENTREGADA'/><xs:enumeration value='TRANSFUNDIDA'/><xs:enumeration value='DEVUELTA'/>
          </xs:restriction></xs:simpleType></xs:element>
          <xs:element name='Observaciones' type='xs:string' minOccurs='0'/>
          <xs:element name='FechaAlta' type='xs:dateTime'/>
          <xs:element name='FechaUltimaModificacion' type='xs:dateTime'/>
        </xs:sequence><xs:attribute name='id' type='xs:positiveInteger' use='required'/></xs:complexType></xs:element>
      </xs:sequence></xs:complexType></xs:element>
    </xs:sequence><xs:attribute name='id' type='xs:positiveInteger' use='required'/>
      <xs:attribute name='cantidadUnidades' type='xs:positiveInteger' use='required'/>
    </xs:complexType></xs:element>
  </xs:sequence><xs:attribute name='version' use='required' fixed='1.0'/></xs:complexType></xs:element>
</xs:schema>";
    }
}
