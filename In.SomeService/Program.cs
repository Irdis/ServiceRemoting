using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using In.ServiceCommon.Client;
using In.ServiceCommon.Interface;
using log4net.Config;

namespace In.SomeService
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us");
            var services = new List<Type>
            {
                typeof(IMyAService),
                typeof(IMyBService),
            };
            var generator = new ClientProxyGenerator(new InterfaceInfoProvider(
                services));
            var proxies = generator.Build(services);
            foreach (var proxy in proxies.OfType<ClientProxyBase>())
            {
                proxy.ServiceProxy = new ClientServiceProxy("", "");
            }

            ((IMyAService) proxies[0]).Add(1, 2);
            XmlConfigurator.Configure();
        }
    }
}