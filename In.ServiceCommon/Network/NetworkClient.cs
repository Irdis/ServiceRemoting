using System.IO;
using System.Net.Sockets;

namespace In.ServiceCommon.Network
{
    public class NetworkClient
    {
        private TcpClient _tcpClient;
        private NetworkChannel _channel;
        private INetworkMessageProcessor _messageProcessor;

        public NetworkClient(INetworkMessageProcessor processor)
        {
            _tcpClient = new TcpClient();
            _messageProcessor = processor;
        }

        public void Connect(string host, int port)
        {
            _tcpClient.Connect(host, port);
            _channel = new NetworkChannel(_tcpClient, _messageProcessor);
            _channel.Listen();
        }

        public void Send(Stream memory)
        {
            _channel.Send(memory);
        }
    }
}