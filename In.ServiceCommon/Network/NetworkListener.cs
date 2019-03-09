using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using In.ServiceCommon.Network;

namespace In.ServiceCommon
{
    public class NetworkListener
    {
        private TcpListener _listener;
        private readonly List<NetworkChannel> _channels = new List<NetworkChannel>();
        public void Listen()
        {
            _listener = new TcpListener(IPAddress.Any, 8000);
            _listener.Start();
            while (true)
            {
                var client = _listener.AcceptTcpClient();
                var channel = new NetworkChannel(client, null);
                _channels.Add(channel);
                channel.Listen();
            }
        }
    }
}