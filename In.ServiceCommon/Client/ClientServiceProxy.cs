using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using In.ServiceCommon.Network;

namespace In.ServiceCommon.Client
{
    public class ClientServiceProxy : INetworkMessageProcessor
    {
        private readonly ISerializer _defaultSerializer;
        private readonly Dictionary<Type, ISerializer> _serializers = new Dictionary<Type, ISerializer>();
        private readonly Dictionary<Tuple<string, string>, RemoteMethodInfo> _methodInfos = new Dictionary<Tuple<string, string>, RemoteMethodInfo>();
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<Stream>> _pendingRequests = new ConcurrentDictionary<Guid, TaskCompletionSource<Stream>>();
        private readonly NetworkClient _networkClient;

        public ClientServiceProxy(string host, string port)
        {
            _defaultSerializer = new DefaultSerializer();
            _networkClient = new NetworkClient(this);
        }

        public object Call(string type, string method, object[] args)
        {
            var key = Guid.NewGuid();
            TaskCompletionSource<Stream> tcs;
            using (var memory = new MemoryStream())
            using (var writer = new BinaryWriter(memory))
            {
                writer.Write(true);
                writer.Write(key.ToByteArray());
                writer.Write(type);
                writer.Write(method);
                WriteArguments(memory, writer, args);

                tcs = new TaskCompletionSource<Stream>();
                _pendingRequests[key] = tcs;
                Send(memory);

                tcs.Task.Wait();
                _pendingRequests.TryRemove(key, out _);
            }
            var result = Result(type, method, tcs);
            return result;

        }

        public void CallVoidSync(string type, string method, object[] args)
        {
            var key = Guid.NewGuid();
            using (var memory = new MemoryStream())
            using (var writer = new BinaryWriter(memory))
            {
                writer.Write(true);
                writer.Write(key.ToByteArray());
                writer.Write(type);
                writer.Write(method);
                WriteArguments(memory, writer, args);

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
            using (var writer = new BinaryWriter(memory))
            {
                writer.Write(false);
                writer.Write(type);
                writer.Write(method);
                WriteArguments(memory, writer, args);
                Send(memory);
            }
        }

        private object Result(string type, string method, TaskCompletionSource<Stream> tcs)
        {
            using (var serializedResult = tcs.Task.Result)
            {
                var methodInfo = _methodInfos[Tuple.Create(type, method)];
                if (!_serializers.TryGetValue(methodInfo.ReturnType, out var serializer))
                {
                    serializer = _defaultSerializer;
                }

                var result = serializer.Deserialize(serializedResult);
                return result;
            }
        }


        private void WriteArguments(Stream memory, BinaryWriter writer, object[] args)
        {
            writer.Write(args.Length);
            foreach (var arg in args)
            {
                var argType = arg.GetType();
                if (_serializers.TryGetValue(argType, out var serializer))
                {
                    serializer.Serialize(arg, memory);
                }
                else
                {
                    _defaultSerializer.Serialize(arg, memory);
                }
            }

        }

        private void Send(Stream memory)
        {
            _networkClient.Send(memory);
        }

        public void OnMessage(object sender, byte[] message)
        {
            using (var memory = new MemoryStream(message))
            using (var reader = new BinaryReader(memory))
            {
                const int guidLength = 16;
                var guidBytes = reader.ReadBytes(guidLength);
                var guid = new Guid(guidBytes);
                if (_pendingRequests.TryGetValue(guid, out var completion))
                {
                    var messageStream = new MemoryStream(message, guidLength, message.Length - guidLength);
                    completion.SetResult(messageStream);
                }
            }
        }
    }
}
