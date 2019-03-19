using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using In.ServiceCommon.Network;
using In.ServiceCommon.Service;

namespace In.ServiceCommon
{
    public class NetworkListener
    {
        private TcpListener _listener;
        private readonly List<NetworkChannel> _channels = new List<NetworkChannel>();
        private readonly INetworkMessageProcessor _messageProcessor;
        private readonly IChannelObserver _channelObserver;

        public NetworkListener(INetworkMessageProcessor messageProcessor, IChannelObserver channelObserver)
        {
            _messageProcessor = messageProcessor;
            _channelObserver = channelObserver;
        }

        public void Listen(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            while (true)
            {
                var client = _listener.AcceptTcpClient();
                var channel = new NetworkChannel(_messageProcessor);
                _channels.Add(channel);
                _channelObserver.OnChannelConnected(channel);
                channel.Listen(client);
            }
        }


        public void Shutdown()
        {
        }
    }
}