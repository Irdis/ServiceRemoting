using System.IO;

namespace In.ServiceCommon.Network
{
    public class NetworkMessageProcessor
    {
        public void Process(NetworkChannel channel, byte[] message)
        {
            var memoryStream = new MemoryStream(message);
            var bf = new BinaryReader(memoryStream);
            var type = bf.ReadString();
            var method = bf.ReadString();
            
        }
    }
}