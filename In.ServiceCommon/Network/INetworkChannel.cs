using System;
using System.IO;
using System.Net.Sockets;

namespace In.ServiceCommon.Network
{
    public interface INetworkChannel
    {
        void Listen(TcpClient stream);
        void Send(Stream memory);

        event Action<object> OnDisconnect;
    }
}