using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using In.ServiceCommon.Client;
using In.ServiceCommon.Interface;

namespace In.ServiceCommon.Service
{
    public class ServiceMessageBuilder
    {
        private readonly ISerializer _defaultSerializer;
        private readonly Dictionary<Type, ISerializer> _serializers;
        private readonly Dictionary<Tuple<string, string>, ServiceCallInfo> _infoProvider;

        public ServiceMessageBuilder(InterfaceInfoProvider infoProvider, ISerializer defaultSerializer, Dictionary<Type, ISerializer> serializers, List<NetworkAdapterService> services)
        {
            _infoProvider = infoProvider.GetServiceCallInfos().ToDictionary(info => Tuple.Create(info.ShortTypeName, info.ShortMethodName));
            foreach (var service in services)
            {
                _infoProvider.Add(Tuple.Create(service.StreamingType, NetworkAdapterService.SubSignature), new ServiceCallInfo
                {
                    Type = typeof(NetworkAdapterService),
                    Await = true,
                    Method = typeof(NetworkAdapterService).GetMethod("Subscribe"),
                    ArgumentTypes = new []{service.GetArgumentType().MakeArrayType()},
                    ReturnType = typeof(bool[]),
                    ShortMethodName = NetworkAdapterService.SubSignature,
                    ShortTypeName = service.StreamingType,
                    StreamingCall = true
                });

                _infoProvider.Add(Tuple.Create(service.StreamingType, NetworkAdapterService.UnsubSignature), new ServiceCallInfo
                {
                    Type = typeof(NetworkAdapterService),
                    Await = true,
                    Method = typeof(NetworkAdapterService).GetMethod("Unsubscribe"),
                    ArgumentTypes = new []{service.GetArgumentType().MakeArrayType()},
                    ReturnType = typeof(bool[]),
                    ShortMethodName = NetworkAdapterService.SubSignature,
                    ShortTypeName = service.StreamingType,
                    StreamingCall = true
                });
            }
            _serializers = serializers;
            _defaultSerializer = defaultSerializer;
        }

        public MessageTarget ParseMessage(byte[] message)
        {
            using (var memory = new MemoryStream(message))
            using (var reader = Reader(memory))
            {
                var header = GetTargetKey(reader);
                var target = GetTarget(header);
                FillParameters(reader, target);
                return target;
            }
        }

        public void WriteStreamingResult(Stream stream, Type target, object result)
        {
            using (var writer = Writer(stream))
            {
                writer.Write((int)MessageType.Streaming);
                if (_serializers.TryGetValue(target, out var serializer))
                {
                    serializer.Serialize(result, stream);
                }
                else
                {
                    _defaultSerializer.Serialize(result, stream);
                }
            }
        }

        public void WriteResult(Stream stream, MessageTarget target, object result)
        {
            using (var writer = Writer(stream))
            {
                var messageKeyValue = target.MessageKey.Value;
                writer.Write((int)MessageType.Rpc);
                writer.Write(messageKeyValue.ToByteArray());
                if (_serializers.TryGetValue(target.Method.ReturnType, out var serializer))
                {
                    serializer.Serialize(result, stream);
                }
                else
                {
                    _defaultSerializer.Serialize(result, stream);
                }
            }
        }

        public void WriteVoidResult(Stream stream, MessageTarget target)
        {
            using (var writer = Writer(stream))
            {
                var messageKeyValue = target.MessageKey.Value;
                writer.Write(messageKeyValue.ToByteArray());
            }
        }

        private void FillParameters(BinaryReader reader, MessageTarget target)
        {
            var parameterLength = reader.ReadInt32();
            target.Arguments = new object[parameterLength];
            var methodParameters = target.ArgumentTypes;
            for (int i = 0; i < parameterLength; i++)
            {
                var methodParameter = methodParameters[i];
                if (_serializers.TryGetValue(methodParameter, out var serializer))
                {
                    var item = serializer.Deserialize(reader.BaseStream);
                    target.Arguments[i] = item;
                }
                else
                {
                    var item = _defaultSerializer.Deserialize(reader.BaseStream);
                    target.Arguments[i] = item;
                }
            }
        }

        private static MessageTargetHeader GetTargetKey(BinaryReader reader)
        {
            var header = new MessageTargetHeader();
            var await = reader.ReadBoolean();
            header.Await = @await;
            if (@await)
            {
                var guidBytes = reader.ReadBytes(16);
                header.MessageKey = new Guid(guidBytes);
            }

            header.Type = reader.ReadString();
            header.Method = reader.ReadString();
            return header;
        }

        private MessageTarget GetTarget(MessageTargetHeader header)
        {
            var target = new MessageTarget();
            var info = _infoProvider[Tuple.Create(header.Type, header.Method)];
            target.Type = info.Type;
            target.ShortTypeName = info.ShortTypeName;
            target.Method = info.Method;
            target.ShortMethodName = info.ShortMethodName;
            target.ArgumentTypes = info.ArgumentTypes;
            target.Await = header.Await;
            target.MessageKey = header.MessageKey;
            target.StreamingCall = info.StreamingCall;
            return target;
        }

        private static BinaryWriter Writer(Stream stream)
        {
            return new BinaryWriter(stream, new UTF8Encoding(false, true), true);
        }

        private static BinaryReader Reader(Stream memory)
        {
            return new BinaryReader(memory, new UTF8Encoding(false, true), true);
        }
    }
}