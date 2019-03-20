using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using In.ServiceCommon.Client;
using In.ServiceCommon.Interface;
using In.ServiceCommon.Network;
using In.ServiceCommon.Streaming;

namespace In.ServiceCommon.Service
{
    public class ServiceContainer : INetworkMessageProcessor, IChannelObserver, IDisposable
    {
        private readonly Dictionary<Type, object> _services;
        private readonly Dictionary<string, DelegateContract> _streamingContracts;
        private readonly NetworkListener _networkListener;
        private readonly ServiceMessageBuilder _messageBuilder;
        private readonly Dictionary<string, NetworkAdapterService> _networkAdapters;
        

        public ServiceContainer(int port, Dictionary<Type, object> services, Dictionary<Type, ISerializer> serializers, Dictionary<string, DelegateContract> streamingContracts)
        {
            _services = services;
            _streamingContracts = streamingContracts;
            var networkAdapterServices = streamingContracts
                .Select(pair => new NetworkAdapterService(pair.Key, pair.Value)).ToList();
            _networkAdapters = networkAdapterServices
                .ToDictionary(service => service.StreamingType);
            _networkListener = new NetworkListener(this, this);
            var interfaceInfoProvider = new InterfaceInfoProvider(services.Keys.ToList());
            _messageBuilder = new ServiceMessageBuilder(interfaceInfoProvider, new DefaultSerializer(), serializers,  networkAdapterServices);
            Init(port);
        }

        private void Init(int port)
        {
            _networkListener.Listen(port);
        }

        public void OnChannelConnected(INetworkChannel networkChannel)
        {
            foreach (var contract in _streamingContracts)
            {
                var delegateContract = contract.Value;
                var serverAdapterType = typeof(NetworkStreamingServerAdapter<>).MakeGenericType(delegateContract.KeyType);
                var adapter = Activator.CreateInstance(serverAdapterType, new object[]
                {
                    delegateContract.ValueType,
                    networkChannel,
                    _messageBuilder,
                    delegateContract
                });
                _networkAdapters[contract.Key].Add(adapter, networkChannel);
            }
        }

        public void OnChannelDisconnected(INetworkChannel networkChannel)
        {
            foreach (var contract in _streamingContracts)
            {
                _networkAdapters[contract.Key].Remove(networkChannel);
            }
        }

        public void OnMessage(object sender, byte[] message)
        {
            var channel = (INetworkChannel) sender;
            var msg = _messageBuilder.ParseMessage(message);
            if (!msg.StreamingCall)
            {
                CallRpcMethod(msg, channel);
            }
            else
            {
                CallStreamingSubUnsub(msg, channel);
            }
        }

        private void CallStreamingSubUnsub(MessageTarget msg, INetworkChannel channel)
        {
            using (var memory = new MemoryStream())
            {
                var streamer = _networkAdapters[msg.ShortTypeName];
                var methodResult = msg.Method.Invoke(streamer, msg.Arguments.Concat(new []{channel}).ToArray());
                _messageBuilder.WriteResult(memory, msg, methodResult);
                channel.Send(memory);
            }
        }

        private void CallRpcMethod(MessageTarget msg, INetworkChannel channel)
        {
            var arguments =  msg.Arguments;
            if (msg.Method.ReturnType == typeof(void) && !msg.Await)
            {
                msg.Method.Invoke(_services[msg.Type], arguments);
                return;
            }

            using (var memory = new MemoryStream())
            {
                if (msg.Method.ReturnType != typeof(void))
                {
                    var methodResult = msg.Method.Invoke(_services[msg.Type], arguments);
                    _messageBuilder.WriteResult(memory, msg, methodResult);
                    channel.Send(memory);
                }
                else
                {
                    msg.Method.Invoke(_services[msg.Type], arguments);
                    _messageBuilder.WriteVoidResult(memory, msg);
                    channel.Send(memory);
                }
            }
        }

        public void Dispose()
        {
            _networkListener.Shutdown();
        }
    }
}