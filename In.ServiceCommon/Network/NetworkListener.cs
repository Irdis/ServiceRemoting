using System;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<TcpClient, NetworkChannel> _channels = new ConcurrentDictionary<TcpClient, NetworkChannel>();
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
            BeginAcceptClient();
        }

        private void BeginAcceptClient()
        {
            _listener.BeginAcceptTcpClient(OnClientAccepted, null);
        }

        private void OnClientAccepted(IAsyncResult ar)
        {
            var client = _listener.EndAcceptTcpClient(ar);
            var channel = new NetworkChannel(_messageProcessor);
            channel.OnDisconnect += ChannelOnDisconnect;
            _channels[client] = channel;
            _channelObserver.OnChannelConnected(channel);
            channel.Listen(client);
            BeginAcceptClient();
        }

        private void ChannelOnDisconnect(object obj)
        {
            var client = (TcpClient) obj;
            _channels.TryRemove(client, out var channel);
            channel.OnDisconnect -= ChannelOnDisconnect;
            _channelObserver.OnChannelDisconnected(channel);
        }

        public void Shutdown()
        {
            _listener.Stop();
        }
    }
}