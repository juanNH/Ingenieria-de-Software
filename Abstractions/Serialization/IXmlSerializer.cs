using System.IO;

namespace Abstractions
{
    public interface IXmlSerializer_380_jh
    {
        string Serialize_380_jh<T>(T value);
        T Deserialize_380_jh<T>(Stream stream, string schema);
    }
}
