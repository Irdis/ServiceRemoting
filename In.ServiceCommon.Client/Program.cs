using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using In.ServiceCommon.Streaming;
using In.SomeService;
using log4net.Config;

namespace In.ServiceCommon.Client
{
    class Program : IStreamingCallback<MyCStreamingData>
    {
        private static MyCStreamingService _service;

        static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            while (true)
            {
                using (var client = new ClientServiceContainer("localhost", 8000, new List<Type>
                {
                    typeof(IMyAService),
                    typeof(IMyBService)
                }, new Dictionary<Type, ISerializer>(), new List<ClientStreamingInfo>
                {
                    MyCStreamingNetworkInfo()
                }))
                {
                    var streamingCallback = new Program();
                    _service.Subscribe(1, streamingCallback);
                    Console.ReadKey();
                    _service.Unsubscribe(1, streamingCallback);
                    Console.ReadKey();
                }
            }
            
            //Console.ReadKey();
            //while (true)
            //{
            //    var streamingCallback = new Program();
            //    _service.Subscribe(1, streamingCallback);
            //    Console.ReadKey();
            //    _service.Unsubscribe(1, streamingCallback);
            //    Console.ReadKey();
            //}
        }

        private static ClientStreamingInfo MyCStreamingNetworkInfo()
        {
            var streamingType = "MyC";
            var adapterFactory = new NetworkStreamingAdapterFactory<int, MyCStreamingData>(streamingType);
            _service = new MyCStreamingService(adapterFactory);
            return new ClientStreamingInfo
            {
                Type = streamingType,
                KeyType = typeof(int),
                ValueType = typeof(MyCStreamingData),
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