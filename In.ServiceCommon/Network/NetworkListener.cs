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
        private readonly INetworkMessageProcessor _messageProcessor;

        public NetworkListener(INetworkMessageProcessor messageProcessor)
        {
            _messageProcessor = messageProcessor;
        }

        public void Listen(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            while (true)
            {
                var client = _listener.AcceptTcpClient();
                var channel = new NetworkChannel(client, _messageProcessor);
                _channels.Add(channel);
                channel.Listen();
            }
        }


        public void Shutdown()
        {

        }

    }
}