using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using In.ServiceCommon.Client;
using In.ServiceCommon.Interface;

namespace In.ServiceCommon.Service
{
    public class ServiceMessageBuilder
    {
        private readonly ISerializer _defaultSerializer;
        private readonly Dictionary<Type, ISerializer> _serializers;
        private readonly Dictionary<Tuple<string, string>, ServiceCallInfo> _infoProvider;

        public ServiceMessageBuilder(InterfaceInfoProvider infoProvider, ISerializer defaultSerializer, Dictionary<Type, ISerializer> serializers)
        {
            _infoProvider = infoProvider.GetServiceCallInfos().ToDictionary(info => Tuple.Create(info.ShortTypeName, info.ShortMethodName));
            _serializers = serializers;
            _defaultSerializer = defaultSerializer;
        }

        public MessageTarget ParseMessage(byte[] message)
        {
            using (var memory = new MemoryStream(message))
            using (var reader = new BinaryReader(memory))

            {
                var header = GetTargetKey(reader);
                var target = GetTarget(header);
                FillParameters(reader, target);
                return target;
            }
        }

        public void WriteResult(Stream stream, MessageTarget target, object result)
        {
            using (var writer = new BinaryWriter(stream))
            {
                var messageKeyValue = target.MessageKey.Value;
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
            using (var writer = new BinaryWriter(stream))
            {
                var messageKeyValue = target.MessageKey.Value;
                writer.Write(messageKeyValue.ToByteArray());
            }
        }

        private void FillParameters(BinaryReader reader, MessageTarget target)
        {
            var parameterLength = reader.ReadInt32();
            target.Arguments = new object[parameterLength];
            var methodParameters = target.Method.GetParameters();
            for (int i = 0; i < parameterLength; i++)
            {
                var methodParameter = methodParameters[i];
                if (_serializers.TryGetValue(methodParameter.ParameterType, out var serializer))
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
            target.Method = info.Method;
            target.Await = header.Await;
            target.MessageKey = header.MessageKey;
            return target;
        }
    }
}