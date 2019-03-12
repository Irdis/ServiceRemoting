using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using In.ServiceCommon.Interface;
using In.ServiceCommon.Network;

namespace In.ServiceCommon.Client
{
    public class ClientServiceProxy : INetworkMessageProcessor
    {
        private readonly ClientMessageBuilder _messageBuilder;
        private readonly NetworkClient _networkClient;
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<Stream>> _pendingRequests = new ConcurrentDictionary<Guid, TaskCompletionSource<Stream>>();

        public ClientServiceProxy(InterfaceInfoProvider interfaceInfo, Dictionary<Type, ISerializer> serializers)
        {
            _messageBuilder = new ClientMessageBuilder(interfaceInfo, serializers);
            _networkClient = new NetworkClient(this);
        }

        public void Connect(string host, int port)
        {
            _networkClient.Connect(host, port);
        }

        public object Call(string type, string method, object[] args)
        {
            var key = Guid.NewGuid();
            TaskCompletionSource<Stream> tcs;
            using (var memory = new MemoryStream())
            {
                _messageBuilder.WriteCall(key, type, method, args, memory);

                tcs = new TaskCompletionSource<Stream>();
                _pendingRequests[key] = tcs;
                Send(memory);

                tcs.Task.Wait();
                _pendingRequests.TryRemove(key, out _);
            }
            var result = _messageBuilder.DeserializeResult(type, method, tcs.Task.Result);
            return result;

        }

        public void CallVoidSync(string type, string method, object[] args)
        {
            var key = Guid.NewGuid();
            using (var memory = new MemoryStream())
            {  
                _messageBuilder.WriteCall(key, type, method, args, memory);

                var tcs = new TaskCompletionSource<Stream>();
                _pendingRequests[key] = tcs;
                Send(memory);

                tcs.Task.Wait();
                _pendingRequests.TryRemove(key, out _);
                tcs.Task.Result.Dispose();
            }
        }

        public void CallVoidAsync(string type, string method, object[] args)
        {
            using (var memory = new MemoryStream())
            {
                _messageBuilder.WriteCall(null, type, method, args, memory);
                Send(memory);
            }
        }

        private void Send(Stream memory)
        {
            _networkClient.Send(memory);
        }

        public void OnMessage(object sender, byte[] message)
        {
            using (var memory = new MemoryStream(message))
            {
                var guid = _messageBuilder.ReadResult(memory);
                if (_pendingRequests.TryGetValue(guid, out var completion))
                {
                    var messageStream = new MemoryStream(message, (int) memory.Position, message.Length - (int)memory.Position);
                    completion.SetResult(messageStream);
                }
            }
        }
    }
}
