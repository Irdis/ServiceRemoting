using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using In.ServiceCommon.Interface;

namespace In.ServiceCommon.Client
{
    public class ClientMessageBuilder
    {
        private readonly ISerializer _defaultSerializer;
        private readonly Dictionary<Type, ISerializer> _serializers;
        private readonly Dictionary<Tuple<string, string>, ServiceCallInfo> _methodInfos;
        
        public const string SubSignature = "sub";
        public const string UnsubSignature = "unsub";

        public ClientMessageBuilder(InterfaceInfoProvider interfaceInfo, Dictionary<Type, ISerializer> serializers,
            List<ClientStreamingInfo> streamingCallbacks)
        {
            _defaultSerializer = new DefaultSerializer();
            _serializers = serializers;
            _methodInfos = interfaceInfo.GetServiceCallInfos()
                .ToDictionary(info => Tuple.Create(info.ShortTypeName, info.ShortMethodName));
            foreach (var streamingCallback in streamingCallbacks)
            {
                var adapterType = streamingCallback.Adapter.GetType();
                _methodInfos.Add(Tuple.Create(streamingCallback.Type, SubSignature), new ServiceCallInfo
                {
                    Type = adapterType,
                    Method = adapterType.GetMethod("Subscribe", new []{streamingCallback.KeyType.MakeArrayType()}),
                    ArgumentTypes = new []{streamingCallback.KeyType.MakeArrayType()},
                    Await = true,
                    ReturnType = typeof(bool[]),
                    ShortMethodName = SubSignature,
                    ShortTypeName = streamingCallback.Type,
                    StreamingCall = false
                });
                _methodInfos.Add(Tuple.Create(streamingCallback.Type, UnsubSignature), new ServiceCallInfo
                {
                    Type = adapterType,
                    Method = adapterType.GetMethod("Unsubscribe", new []{streamingCallback.KeyType.MakeArrayType()}),
                    ArgumentTypes = new []{streamingCallback.KeyType.MakeArrayType()},
                    Await = true,
                    ReturnType = typeof(bool[]),
                    ShortMethodName = UnsubSignature,
                    ShortTypeName = streamingCallback.Type,
                    StreamingCall = false
                });
            }
        }

        public void WriteCall(Guid? key, string type, string method, object[] args, Stream stream)
        {
            using (var writer = Writer(stream))
            {
                writer.Write(key.HasValue);
                if (key.HasValue)
                {
                    writer.Write(key.Value.ToByteArray());
                }

                writer.Write(type);
                writer.Write(method);
                WriteArguments(stream, writer, args);
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

        public ClientMessageHeader ReadHeader(Stream memory)
        {
            using (var reader = Reader(memory))
            {
                var msgType = reader.ReadInt32();
                if (msgType == (int) MessageType.Streaming)
                {
                    var streamingTarget = reader.ReadString();
                    return new ClientMessageHeader
                    {
                        Type = MessageType.Streaming,
                        StreamingTarget = streamingTarget
                    };
                }

                const int guidLength = 16;
                var guidBytes = reader.ReadBytes(guidLength);
                var guid = new Guid(guidBytes);
                return new ClientMessageHeader
                {
                    Type = MessageType.Rpc,
                    Key = guid
                };
            }
        }

        private static BinaryWriter Writer(Stream stream)
        {
            return new BinaryWriter(stream, new UTF8Encoding(false, true), true);
        }

        private static BinaryReader Reader(Stream memory)
        {
            return new BinaryReader(memory, new UTF8Encoding(false, true), true);
        }

        public object DeserializeResult(string type, string method, Stream memory)
        {
            using (memory)
            {
                var methodInfo = _methodInfos[Tuple.Create(type, method)];
                if (!_serializers.TryGetValue(methodInfo.ReturnType, out var serializer))
                {
                    serializer = _defaultSerializer;
                }
                var result = serializer.Deserialize(memory);
                return result;
            }
        }

        
        public object DeserializeResult(Type t, Stream memory)
        {
            using (memory)
            {
                if (!_serializers.TryGetValue(t, out var serializer))
                {
                    serializer = _defaultSerializer;
                }
                var result = serializer.Deserialize(memory);
                return result;
            }
        }
    }
}