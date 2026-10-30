using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Abstractions;
using Domain;

namespace Application
{
    public class TablaUnidadesXmlService_380_jh
    {
        private readonly IXmlSerializer_380_jh _serializer_380_jh;

        public TablaUnidadesXmlService_380_jh(IXmlSerializer_380_jh serializer)
        {
            _serializer_380_jh = serializer ?? throw new ArgumentNullException("serializer");
        }

        public string Serializar_380_jh(List<FilaUnidadXml_380_jh> filas)
        {
            var tabla = new TablaUnidadesXml_380_jh { Version_380_jh = "1.0", Unidades_380_jh = filas };
            Validar_380_jh(tabla);
            return _serializer_380_jh.Serialize_380_jh(tabla);
        }

        public TablaUnidadesXml_380_jh Deserializar_380_jh(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            using (var contenido = new MemoryStream())
            {
                byte[] buffer = new byte[8192];
                int leidos;
                while ((leidos = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (contenido.Length + leidos > 10000000) throw new InvalidDataException("El XML supera el tamaño permitido.");
                    contenido.Write(buffer, 0, leidos);
                }

                contenido.Position = 0;
                string raiz;
                using (XmlReader reader = XmlReader.Create(contenido, new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null,
                    MaxCharactersInDocument = 5000000, CloseInput = false
                }))
                {
                    reader.MoveToContent();
                    if (reader.NamespaceURI != string.Empty) throw new InvalidDataException("Espacio de nombres no admitido.");
                    raiz = reader.LocalName;
                }
                contenido.Position = 0;
                TablaUnidadesXml_380_jh tabla;
                if (raiz == "ProcesoPN1")
                {
                    // Los archivos exportados por la versión anterior siguen siendo importables.
                    var anterior = new ProcesoPN1XmlImportService_380_jh(_serializer_380_jh).Importar_380_jh(contenido);
                    tabla = new TablaUnidadesXml_380_jh { Version_380_jh = "1.0" };
                    foreach (var unidad in anterior.Donacion_380_jh.Unidades_380_jh)
                        tabla.Unidades_380_jh.Add(new FilaUnidadXml_380_jh
                        {
                            CodigoIdentificacion_380_jh = unidad.CodigoIdentificacion_380_jh,
                            Donante_380_jh = anterior.Donacion_380_jh.Donante_380_jh.Nombre_380_jh,
                            TipoComponente_380_jh = unidad.TipoComponente_380_jh,
                            GrupoSanguineo_380_jh = unidad.GrupoSanguineo_380_jh,
                            FactorRh_380_jh = unidad.FactorRh_380_jh,
                            FechaVencimiento_380_jh = unidad.FechaVencimiento_380_jh,
                            EstadoOperativo_380_jh = unidad.EstadoOperativo_380_jh
                        });
                }
                else
                {
                    tabla = _serializer_380_jh.Deserialize_380_jh<TablaUnidadesXml_380_jh>(contenido, Esquema_380_jh);
                }
                Validar_380_jh(tabla);
                return tabla;
            }
        }

        private static void Validar_380_jh(TablaUnidadesXml_380_jh tabla)
        {
            if (tabla == null || tabla.Version_380_jh != "1.0" || tabla.Unidades_380_jh == null)
                throw new InvalidDataException("Tabla XML inválida.");
            var codigos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int numeroFila = 0;
            foreach (var fila in tabla.Unidades_380_jh)
            {
                numeroFila++;
                string ubicacion = "Fila " + numeroFila + ": ";
                if (fila == null) throw new InvalidDataException(ubicacion + "faltan los datos de la unidad.");
                if (string.IsNullOrWhiteSpace(fila.CodigoIdentificacion_380_jh) || fila.CodigoIdentificacion_380_jh.Length > 50)
                    throw new InvalidDataException(ubicacion + "el código es obligatorio y admite hasta 50 caracteres.");
                if (!codigos.Add(fila.CodigoIdentificacion_380_jh.Trim()))
                    throw new InvalidDataException(ubicacion + "el código está repetido.");
                if (string.IsNullOrWhiteSpace(fila.Donante_380_jh) || fila.Donante_380_jh.Length > 201)
                    throw new InvalidDataException(ubicacion + "el donante es obligatorio y admite hasta 201 caracteres.");
                if (string.IsNullOrWhiteSpace(fila.TipoComponente_380_jh) || fila.TipoComponente_380_jh.Length > 100)
                    throw new InvalidDataException(ubicacion + "el componente es obligatorio y admite hasta 100 caracteres.");
                if (fila.FechaVencimiento_380_jh == default(DateTime))
                    throw new InvalidDataException(ubicacion + "la fecha de vencimiento no es válida.");
                bool grupo = !string.IsNullOrWhiteSpace(fila.GrupoSanguineo_380_jh);
                bool rh = !string.IsNullOrWhiteSpace(fila.FactorRh_380_jh);
                bool estadoValido = fila.EstadoOperativo_380_jh == EstadoUnidadPN1_380_jh.EnRevision_380_jh ||
                    fila.EstadoOperativo_380_jh == EstadoUnidadPN1_380_jh.Liberada_380_jh ||
                    fila.EstadoOperativo_380_jh == EstadoUnidadPN1_380_jh.Bloqueada_380_jh ||
                    fila.EstadoOperativo_380_jh == EstadoUnidadPN1_380_jh.Descartada_380_jh ||
                    fila.EstadoOperativo_380_jh == EstadoUnidadPN2_380_jh.Reservada_380_jh ||
                    fila.EstadoOperativo_380_jh == EstadoUnidadPN2_380_jh.Entregada_380_jh ||
                    fila.EstadoOperativo_380_jh == EstadoUnidadPN2_380_jh.Transfundida_380_jh ||
                    fila.EstadoOperativo_380_jh == EstadoUnidadPN2_380_jh.Devuelta_380_jh;
                if (!estadoValido) throw new InvalidDataException(ubicacion + "el estado operativo no es válido.");
                if (grupo != rh ||
                    (grupo && fila.GrupoSanguineo_380_jh != "A" && fila.GrupoSanguineo_380_jh != "B" && fila.GrupoSanguineo_380_jh != "AB" && fila.GrupoSanguineo_380_jh != "O") ||
                    (rh && fila.FactorRh_380_jh != "+" && fila.FactorRh_380_jh != "-") ||
                    (fila.EstadoOperativo_380_jh != EstadoUnidadPN1_380_jh.EnRevision_380_jh && !grupo))
                    throw new InvalidDataException(ubicacion + "revisá el grupo sanguíneo y el factor Rh.");
            }
        }

        private const string Esquema_380_jh = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' elementFormDefault='qualified'>
<xs:element name='TablaUnidadesPN1'><xs:complexType><xs:sequence>
 <xs:element name='Unidad' minOccurs='0' maxOccurs='unbounded'><xs:complexType><xs:sequence>
  <xs:element name='CodigoIdentificacion' type='xs:string'/>
  <xs:element name='Donante' type='xs:string'/>
  <xs:element name='TipoComponente' type='xs:string'/>
  <xs:element name='GrupoSanguineo' type='xs:string' minOccurs='0'/>
  <xs:element name='FactorRh' type='xs:string' minOccurs='0'/>
  <xs:element name='FechaVencimiento' type='xs:date'/>
  <xs:element name='EstadoOperativo'><xs:simpleType><xs:restriction base='xs:string'>
   <xs:enumeration value='EN_REVISION'/><xs:enumeration value='LIBERADA'/><xs:enumeration value='BLOQUEADA'/><xs:enumeration value='DESCARTADA'/>
   <xs:enumeration value='RESERVADA'/><xs:enumeration value='ENTREGADA'/><xs:enumeration value='TRANSFUNDIDA'/><xs:enumeration value='DEVUELTA'/>
  </xs:restriction></xs:simpleType></xs:element>
 </xs:sequence></xs:complexType></xs:element>
</xs:sequence><xs:attribute name='version' use='required' fixed='1.0'/></xs:complexType></xs:element>
</xs:schema>";
    }
}
