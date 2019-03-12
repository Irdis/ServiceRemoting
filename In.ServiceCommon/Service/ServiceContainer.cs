using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using In.ServiceCommon.Client;
using In.ServiceCommon.Interface;
using In.ServiceCommon.Network;

namespace In.ServiceCommon.Service
{
    public class ServiceContainer : INetworkMessageProcessor
    {
        private readonly Dictionary<Type, object> _services;
        private readonly NetworkListener _networkListener;
        private readonly ServiceMessageBuilder _messageBuilder;

        public ServiceContainer(Dictionary<Type, object> services, Dictionary<Type, ISerializer> serializers)
        {
            _services = services;
            _networkListener = new NetworkListener(this);
            var interfaceInfoProvider = new InterfaceInfoProvider(services.Keys.ToList());
            _messageBuilder = new ServiceMessageBuilder(interfaceInfoProvider, new DefaultSerializer(), serializers);
            Init();
        }

        private void Init()
        {
            _networkListener.Listen();
        }

        public void OnMessage(object sender, byte[] message)
        {
            var channel = (INetworkChannel) sender;
            var msg = _messageBuilder.ParseMessage(message);
            CallMethod(msg, channel);
        }

        private void CallMethod(MessageTarget msg, INetworkChannel channel)
        {
            if (msg.Method.ReturnType == typeof(void) && !msg.Await)
            {
                msg.Method.Invoke(_services[msg.Type], msg.Arguments);
                return;
            }

            using (var memory = new MemoryStream())
            {
                if (msg.Method.ReturnType != typeof(void))
                {
                    var methodResult = msg.Method.Invoke(_services[msg.Type], msg.Arguments);
                    _messageBuilder.WriteResult(memory, msg, methodResult);
                    channel.Send(memory);
                }
                else
                {
                    msg.Method.Invoke(_services[msg.Type], msg.Arguments);
                    _messageBuilder.WriteVoidResult(memory, msg);
                    channel.Send(memory);
                }
            }
        }
    }
}