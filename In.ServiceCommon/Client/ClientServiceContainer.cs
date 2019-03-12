using System;
using System.Collections.Generic;
using System.Linq;
using In.ServiceCommon.Interface;

namespace In.ServiceCommon.Client
{
    public class ClientServiceContainer
    {
        private readonly Dictionary<Type, object> _services;

        public ClientServiceContainer(
            string host,
            int port,
            List<Type> interfaces, 
            Dictionary<Type, ISerializer> serializers)
        {
            var interfaceInfoProvider = new InterfaceInfoProvider(interfaces);
            var generator = new ClientProxyGenerator(interfaceInfoProvider);
            var proxies = generator.Generate();
            var serviceProxy = new ClientServiceProxy(interfaceInfoProvider, serializers);
            foreach (var proxy in proxies.OfType<ClientProxyBase>())
            {
                proxy.ServiceProxy = serviceProxy;
            }

            _services = interfaces.Zip(proxies, Tuple.Create)
                .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);

            serviceProxy.Connect(host, port);
        }

        public object GetService(Type interfaceType)
        {
            return _services[interfaceType];
        }
    }
}