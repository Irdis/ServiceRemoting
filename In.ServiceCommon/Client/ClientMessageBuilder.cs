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

        public ClientMessageBuilder(InterfaceInfoProvider interfaceInfo, Dictionary<Type, ISerializer> serializers)
        {
            _defaultSerializer = new DefaultSerializer();
            _serializers = serializers;
            _methodInfos = interfaceInfo.GetServiceCallInfos()
                .ToDictionary(info => Tuple.Create<string, string>(info.ShortTypeName, info.ShortMethodName));
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