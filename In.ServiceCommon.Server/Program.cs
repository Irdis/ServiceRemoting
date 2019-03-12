using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using In.ServiceCommon.Client;
using In.ServiceCommon.Service;
using In.SomeService;
using log4net.Config;

namespace In.ServiceCommon.Server
{
    class Program
    {
        public static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            var serviceContainer = new ServiceContainer(8000, new Dictionary<Type, object>{
            {typeof(IMyAService), new MyAService()},
            {typeof(IMyBService), new MyBService()},
            }, new Dictionary<Type, ISerializer>());

            Console.ReadKey();
        }
    }
}