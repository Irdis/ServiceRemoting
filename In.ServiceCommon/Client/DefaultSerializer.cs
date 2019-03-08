using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace In.ServiceCommon.Client
{
    public class DefaultSerializer : ISerializer
    {
        private readonly BinaryFormatter _binaryFormatter = new BinaryFormatter();

        public void Serialize(object arg, Stream stream)
        {
            _binaryFormatter.Serialize(stream, arg);
        }

        public object Deserialize(Stream stream)
        {
            var result = _binaryFormatter.Deserialize(stream);
            return result;
        }
    }
}