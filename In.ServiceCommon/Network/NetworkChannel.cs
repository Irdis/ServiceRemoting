using System;
using System.IO;
using System.Net.Sockets;
using log4net;

namespace In.ServiceCommon.Network
{
    public class NetworkChannel : INetworkChannel
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(NetworkChannel));

        private readonly INetworkMessageProcessor _messageProcessor;
        private readonly byte[] _buffer = new byte[BufferLength];
        private Stream _stream;
        private const int BufferLength = 1024;
        private volatile int _bytesNeed;
        private volatile int _bytesRead;
        public event Action<object> OnDisconnect;
        private readonly object _lock = new object();
        private TcpClient _client;

        public NetworkChannel(INetworkMessageProcessor messageProcessor)
        {
            _messageProcessor = messageProcessor;
        }

        public void Listen(TcpClient client)
        {
            _client = client;
            _stream = client.GetStream();
            _bytesNeed = 4;
            _bytesRead = 0;
            AwaitHeader(new ChannelState
            {
                Client = _client,
                Message = null
            });
        }

        private void AwaitHeader(ChannelState state)
        {
            TryBeginRead(_buffer, _bytesRead, _bytesNeed-_bytesRead, AwaitHeaderCompleted, state);
        }

        private void AwaitHeaderCompleted(IAsyncResult ar)
        {
            if (TryEndRead(ar, out var read))
            {
                var state = (ChannelState)ar.AsyncState;
                _bytesRead += read;
                if (_bytesRead < _bytesNeed)
                {
                    AwaitHeader(state);
                    return;
                }

                var msgSize = BitConverter.ToInt32(_buffer, 0);
                _bytesNeed = msgSize;
                _bytesRead = 0;
                var msg = new NetworkMessage(msgSize);
                state.Message = msg;
                AwaitBody(state);
            }
        }


        private void AwaitBody(ChannelState state)
        {
            TryBeginRead(_buffer, 0, Math.Min(BufferLength, _bytesNeed - _bytesRead), AwaitBodyCompleted, state);
        }

        private void AwaitBodyCompleted(IAsyncResult ar)
        {
            if (TryEndRead(ar, out var read))
            {
                _bytesRead += read;
                var state = (ChannelState) ar.AsyncState;
                var message = state.Message;
                message.AppendData(_buffer, read);
                if (_bytesRead < _bytesNeed)
                {
                    AwaitBody(state);
                    return;
                }

                MessageCompleted(message);
                state.Message = null;
                _bytesNeed = 4;
                _bytesRead = 0;
                AwaitHeader(state);
            }
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
                TrySend(memory);
            }
        }

        private void TrySend(Stream memory)
        {
            try
            {
                var buffer = BitConverter.GetBytes((int) memory.Length);
                _stream.Write(buffer, 0, buffer.Length);
                memory.Position = 0;
                memory.CopyTo(_stream);
            }
            catch (IOException e)
            {
                _log.Warn("Unable to write data", e);
                throw new NetworkChannelDisconnected("Channel disconnected", e);
            }
        }


        private bool TryBeginRead(byte[] buffer,
            int offset,
            int size,
            AsyncCallback callback,
            ChannelState state)
        {
            try
            {
                _stream.BeginRead(buffer, offset, size, callback, state);
                return true;
            }
            catch (ObjectDisposedException e)
            {
                _log.Warn("Stream disposed", e);
                return false;
            }
            catch (IOException e)
            {
                _log.Warn("Unable to attempt reading data", e);
                OnDisconnect?.Invoke(state.Client);
                return false;
            }
        }

        private bool TryEndRead(IAsyncResult ar, out int read)
        {
            try
            {
                read = _stream.EndRead(ar);
                if (read == 0)
                {
                    var state = (ChannelState) ar.AsyncState;
                    OnDisconnect?.Invoke(state.Client);
                    return false;
                }
                return true;
            }
            catch (ObjectDisposedException e)
            {
                _log.Warn("Stream disposed", e);
                read = 0;
                return false;
            }
            catch (IOException e)
            {
                _log.Warn("Unable to read data", e);
                read = 0;
                var state = (ChannelState) ar.AsyncState;
                OnDisconnect?.Invoke(state.Client);
                return false;
            }
        }

    }
}