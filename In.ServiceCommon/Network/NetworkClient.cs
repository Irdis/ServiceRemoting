using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using log4net;

namespace In.ServiceCommon.Network
{
    public enum ClientState
    {
        NotConnected,
        Connecting,
        Connected,
        Shutdown
    }

    public class NetworkClient
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(NetworkClient));

        private volatile TcpClient _tcpClient;
        private readonly NetworkChannel _channel;
        private readonly INetworkMessageProcessor _messageProcessor;

        private readonly Timer _reconnectTimer;
        private readonly int _reconnectTimeout = (int) TimeSpan.FromSeconds(5).TotalMilliseconds;
        private readonly string _host;
        private readonly int _port;

        private volatile ClientState _state = ClientState.NotConnected;
        private readonly object _lock = new object();

        public NetworkClient(string host, int port, INetworkMessageProcessor processor)
        {
            _host = host;
            _port = port;
            _tcpClient = new TcpClient();
            _messageProcessor = processor;
            _reconnectTimer = new Timer(OnReconnect, null, Timeout.Infinite, Timeout.Infinite);
            _channel = new NetworkChannel(_messageProcessor);
            _channel.OnDisconnect += OnDisconnect;
        }

        public void Initialize()
        {
            lock (_lock)
            {
                _state = ClientState.Connecting;
                Connect();
            }
        }

        private void Connect()
        {
            if (TryConnectClient())
            {
                _state = ClientState.Connected;
                _channel.Listen(_tcpClient);
            }
        }

        private bool TryConnectClient()
        {
            try
            {
                _log.InfoFormat("Connecting to {0}:{1}", _host, _port);
                _tcpClient.Connect(_host, _port);
                _log.InfoFormat("Connected to {0}:{1}", _host, _port);
                return true;
            }
            catch (SocketException e)
            {
                _log.Warn("Unable to connect", e);
                _reconnectTimer.Change(_reconnectTimeout, Timeout.Infinite);
                _log.InfoFormat("Reconnecting in {0}...", _reconnectTimer);
                return false;
            }
        }

        private void OnReconnect(object state)
        {
            lock (_lock)
            {
                if (_state == ClientState.Shutdown)
                {
                    return;
                }
                _log.InfoFormat("Reconnecting");
                Connect();
            }
        }

        private void OnDisconnect(object sender)
        {
            lock (_lock)
            {
                if (_state != ClientState.Connected)
                {
                    return;
                }
                if (sender != _tcpClient)
                {
                    return;
                }

                _state = ClientState.Connecting;
                CleanupTcpClient();
                _tcpClient = new TcpClient();
                Connect();
            }
        }

        private void CleanupTcpClient()
        {
            _tcpClient.Close();
        }

        public void Send(Stream memory)
        {
            lock (_lock)
            {
                if (_state != ClientState.Connected)
                {
                    throw new InvalidNetworkClientStateException();
                }
                _channel.Send(memory);
            }
        }

        public void Shutdown()
        {
            lock (_lock)
            {
                _state = ClientState.Shutdown;
                _reconnectTimer.Dispose();
                _channel.OnDisconnect -= OnDisconnect;
                CleanupTcpClient();
            }
        }
    }
}