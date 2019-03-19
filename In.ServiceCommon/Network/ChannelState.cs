using System.Net.Sockets;

namespace In.ServiceCommon.Network
{
    public class ChannelState
    {
        public TcpClient Client { get; set; }
        public NetworkMessage Message { get; set; }
    }
}