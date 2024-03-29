﻿using System;
using System.Collections.Generic;
using System.Linq;
using In.ServiceCommon.Interface;
using In.ServiceCommon.Streaming;

namespace In.ServiceCommon.Client
{
    public class ClientServiceContainer : IDisposable
    {
        private readonly Dictionary<Type, object> _services;
        private ClientServiceProxy _serviceProxy;

        public ClientServiceContainer(
            string host,
            int port,
            List<Type> remoteInterfaces,
            Dictionary<Type, ISerializer> serializers,
            List<ClientStreamingInfo> networkStreamingAdapters)
        {
            var interfaceInfoProvider = new InterfaceInfoProvider(remoteInterfaces);
            var generator = new ClientProxyGenerator(interfaceInfoProvider);
            var proxies = generator.Generate();
            _serviceProxy = new ClientServiceProxy(interfaceInfoProvider, serializers, networkStreamingAdapters);

            var streamingAdapters = networkStreamingAdapters.Select(info => info.Adapter).OfType<ClientProxyBase>();
            var rpcProxies = proxies.OfType<ClientProxyBase>();

            foreach (var proxy in rpcProxies.Concat(streamingAdapters))
            {
                proxy.ServiceProxy = _serviceProxy;
            }

            _services = remoteInterfaces.Zip(proxies, Tuple.Create)
                .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);

            _serviceProxy.Connect(host, port);
        }

        public object GetService(Type interfaceType)
        {
            return _services[interfaceType];
        }

        public void Dispose()
        {
            _serviceProxy?.Dispose();
        }
    }
}