using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using In.ServiceCommon.Interface;
using In.ServiceCommon.Network;

namespace In.ServiceCommon.Client
{
    public class ClientServiceProxy : INetworkMessageProcessor, IClientServiceProxy, IDisposable
    {
        private NetworkClient _networkClient;
        
        private readonly ClientMessageBuilder _messageBuilder;
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<Stream>> _pendingRequests = new ConcurrentDictionary<Guid, TaskCompletionSource<Stream>>();
        private readonly Dictionary<string, DelegateCallback> _streamingCallbacks;


        public ClientServiceProxy(InterfaceInfoProvider interfaceInfo, Dictionary<Type, ISerializer> serializers, List<ClientStreamingInfo> streamingCallbacks)
        {
            var streamers = streamingCallbacks.ToDictionary(info => info.Type, info => info.Callback);
            _streamingCallbacks = streamers;
            _messageBuilder = new ClientMessageBuilder(interfaceInfo, serializers, streamingCallbacks);
            
        }

        public void Connect(string host, int port)
        {
            _networkClient = new NetworkClient(host, port, this);
            _networkClient.Initialize();
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
                var header = _messageBuilder.ReadHeader(memory);

                if (header.Type == MessageType.Streaming)
                {
                    ProcessStreamingMessage(header, memory, message);
                }
                else if (header.Type == MessageType.Rpc)
                {
                    ProcessRpcMessage(header, memory, message);
                }
            }
        }

        private void ProcessRpcMessage(ClientMessageHeader header, MemoryStream memory, byte[] message)
        {
            if (_pendingRequests.TryGetValue(header.Key.Value, out var completionSource))
            {
                var messageStream = new MemoryStream(message, (int) memory.Position, message.Length - (int) memory.Position);
                completionSource.SetResult(messageStream);
            }
        }


        private void ProcessStreamingMessage(ClientMessageHeader header, MemoryStream memory, byte[] message)
        {
            if (_streamingCallbacks.TryGetValue(header.StreamingTarget, out var callback))
            {
                var result = _messageBuilder.DeserializeResult(callback.Type, memory);
                callback.Send(result);
            }
        }

        public void Dispose()
        {
            _networkClient?.Shutdown();
        }
    }
}
