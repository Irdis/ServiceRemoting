using System;
using System.Collections.Generic;
using In.ServiceCommon.Client;
using In.ServiceCommon.Service;
using In.ServiceCommon.Streaming;
using In.SomeService;
using log4net.Config;

namespace In.ServiceCommon.Server
{
    class Program
    {
        public static void Main(string[] args)
        {
            XmlConfigurator.Configure();


            var myCStreamingService = MyCStreamingInfo();
            var serviceContainer = new ServiceContainer(8000, new Dictionary<Type, object>{
            {typeof(IMyAService), new MyAService()},
            {typeof(IMyBService), new MyBService()},
            }, new Dictionary<Type, ISerializer>(), new Dictionary<string, DelegateContract>
            {
                {"MyC", DelegateContract.Create(myCStreamingService, "MyC")}
            });

            Console.ReadKey();
        }

        private static MyCStreamingService MyCStreamingInfo()
        {
            var streamingType = "MyC";
            var adapterFactory = new MyCStreamingAdapterFactory();
            var myC = new MyCStreamingService(adapterFactory);
            var client = (IClientAdapter<IStreamingCallback<MyCStreamingData>>) adapterFactory.GetAdapter();
            client.SetCallback(myC);
            return myC;
        }
    }


}