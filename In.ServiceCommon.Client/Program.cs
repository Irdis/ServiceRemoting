using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using In.ServiceCommon.Streaming;
using In.SomeService;

namespace In.ServiceCommon.Client
{
    class Program : IStreamingCallback<MyCStreamingData>
    {
        private static MyCStreamingService _service;
        static void Main(string[] args)
        {
            var client = new ClientServiceContainer("localhost", 8000, new List<Type>
            {
                typeof(IMyAService),
                typeof(IMyBService)
            }, new Dictionary<Type, ISerializer>(), new List<ClientStreamingInfo>
            {
                MyCStreamingNetworkInfo()
            });
            Console.ReadKey();
            var serviceA = (IMyAService)client.GetService(typeof(IMyAService));
            var result = serviceA.Add(1, 2);
            Console.WriteLine(result);
            Console.ReadKey();
            var serviceB = (IMyBService)client.GetService(typeof(IMyBService));
            var result2 = serviceB.Rotate(new Bar
            {
                Name = "hello"
            });
            Console.WriteLine(result2);
            Console.ReadKey();
            _service.Subscribe(1, new Program());
            Console.ReadKey();
        }

        private static ClientStreamingInfo MyCStreamingNetworkInfo()
        {
            var streamingType = "MyC";
            var adapterFactory = new NetworkStreamingAdapterFactory<int, MyCStreamingData>(streamingType);
            _service = new MyCStreamingService(adapterFactory);
            return new ClientStreamingInfo
            {
                Type = streamingType,
                Adapter = (ClientProxyBase) adapterFactory.GetAdapter(),
                Callback = DelegateCallback.Create(_service)
            };
        }

        private static void MyCStreamingInfo()
        {
            var streamingType = "MyC";
            var adapterFactory = new MyCStreamingAdapterFactory();
            _service = new MyCStreamingService(adapterFactory);
            var client = (IClientAdapter<IStreamingCallback<MyCStreamingData>>) adapterFactory.GetAdapter();
            client.SetCallback(_service);
        }

        public void Send(MyCStreamingData data)
        {
            Console.WriteLine(data.Data);
        }
    }
}
