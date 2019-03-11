using System.IO;

namespace In.ServiceCommon.Network
{
    public interface INetworkChannel
    {
        void Listen();
        void Send(Stream memory);
    }
}