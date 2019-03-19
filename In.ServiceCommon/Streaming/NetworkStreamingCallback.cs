using System;
using System.IO;
using In.ServiceCommon.Network;
using In.ServiceCommon.Service;

namespace In.ServiceCommon.Streaming
{
    public class NetworkStreamingCallback<T> : IStreamingCallback<T>
    {
        private readonly string _streamingType;
        private readonly ServiceMessageBuilder _builder;
        private readonly NetworkChannel _channel;
        private readonly Type _implementationType;

        public NetworkStreamingCallback(string streamingType, ServiceMessageBuilder builder, NetworkChannel channel, Type implementationType)
        {
            _streamingType = streamingType;
            _builder = builder;
            _channel = channel;
            _implementationType = implementationType;
        }

        public void Send(T data)
        {
            using (var memory  = new MemoryStream())
            {
                _builder.WriteStreamingResult(memory, _streamingType, _implementationType, data);
                _channel.Send(memory);
            }
        }
    }
}