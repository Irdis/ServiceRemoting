using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using In.SomeService;

namespace In.ServiceCommon.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new ClientServiceContainer("localhost", 8000, new List<Type>
            {
                typeof(IMyAService),
                typeof(IMyBService)
            }, new Dictionary<Type, ISerializer>());
            Console.ReadKey();
            var serviceA = (IMyAService)client.GetService(typeof(IMyAService));
            var result = serviceA.Add(1, 2);
            Console.WriteLine(result);
            Console.ReadKey();
        }
    }
}
