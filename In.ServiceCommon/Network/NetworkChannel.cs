using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace In.ServiceCommon.Network
{
    public class NetworkChannel : INetworkChannel
    {
        private readonly TcpClient _client;
        private readonly INetworkMessageProcessor _messageProcessor;
        private readonly byte[] _buffer = new byte[BufferLength];
        private NetworkStream _stream;
        private const int BufferLength = 1024;
        private int _bytesNeed;
        private int _bytesRead;
        private readonly object _lock = new object();

        public NetworkChannel(TcpClient client, INetworkMessageProcessor messageProcessor)
        {
            _client = client;
            _messageProcessor = messageProcessor;
        }

        public void Listen()
        {
            _stream = _client.GetStream();
            _bytesNeed = 4;
            _bytesRead = 0;
            AwaitHeader();
        }

        private void AwaitHeader()
        {
            _stream.BeginRead(_buffer, _bytesRead, _bytesNeed-_bytesRead, AwaitHeaderCompleted, null);
        }

        private void AwaitHeaderCompleted(IAsyncResult ar)
        {
            var read = _stream.EndRead(ar);
            _bytesRead += read;
            if (_bytesRead < _bytesNeed)
            {
                AwaitHeader();
                return;
            }

            var msgSize = BitConverter.ToInt32(_buffer, 0);
            _bytesNeed = msgSize;
            _bytesRead = 0;
            var msg = new NetworkMessage(msgSize);
            AwaitBody(msg);

        }

        private void AwaitBody(NetworkMessage message)
        {
            _stream.BeginRead(_buffer, 0, Math.Min(BufferLength, _bytesNeed - _bytesRead), AwaitBodyCompleted, message);
        }

        private void AwaitBodyCompleted(IAsyncResult ar)
        {
            var read = _stream.EndRead(ar);
            _bytesRead += read;
            var message = (NetworkMessage) ar.AsyncState;
            message.AppendData(_buffer, read);
            if (_bytesRead < _bytesNeed)
            {
                AwaitBody(message);
                return;
            }

            MessageCompleted(message);
            _bytesNeed = 4;
            _bytesRead = 0;
            AwaitHeader();
        }

        private void MessageCompleted(NetworkMessage message)
        {
            var msg = message.GetBytes();
            _messageProcessor.OnMessage(this, msg);
        }

        public void Send(Stream memory)
        {
            lock (_lock)
            {
                var buffer = BitConverter.GetBytes((int)memory.Length);
                _stream.Write(buffer, 0, buffer.Length);
                memory.Position = 0;
                memory.CopyTo(_stream);
            }
        }
    }
}