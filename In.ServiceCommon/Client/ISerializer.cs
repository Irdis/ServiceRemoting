using System.IO;

namespace In.ServiceCommon.Client
{
    public interface ISerializer
    {
        void Serialize(object arg, Stream stream);
        object Deserialize(Stream stream);
    }
}