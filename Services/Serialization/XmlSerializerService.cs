using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Abstractions;

namespace Services
{
    public class XmlSerializerService_380_jh : IXmlSerializer_380_jh
    {
        public string Serialize_380_jh<T>(T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using (Utf8StringWriter_380_jh textWriter = new Utf8StringWriter_380_jh())
            using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = false
            }))
            {
                serializer.Serialize(xmlWriter, value, namespaces);
                return textWriter.ToString();
            }
        }

        public T Deserialize_380_jh<T>(Stream stream, string schema)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (string.IsNullOrWhiteSpace(schema)) throw new ArgumentException("Schema is required.", "schema");

            XmlReaderSettings settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                MaxCharactersInDocument = 5000000,
                ValidationType = ValidationType.Schema,
                CloseInput = false
            };
            using (StringReader schemaText = new StringReader(schema))
            using (XmlReader schemaReader = XmlReader.Create(schemaText, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }))
            {
                settings.Schemas.Add(null, schemaReader);
            }
            settings.ValidationEventHandler += delegate(object sender, System.Xml.Schema.ValidationEventArgs args)
            {
                throw new InvalidDataException("El XML no cumple el formato esperado.", args.Exception);
            };

            using (XmlReader reader = XmlReader.Create(stream, settings))
            {
                T result = (T)new XmlSerializer(typeof(T)).Deserialize(reader);
                while (reader.Read()) { }
                return result;
            }
        }

        private sealed class Utf8StringWriter_380_jh : StringWriter
        {
            public override Encoding Encoding
            {
                get { return new UTF8Encoding(false); }
            }
        }
    }
}
