using System;
using In.ServiceCommon.Streaming;

namespace In.SomeService
{
    public class MyCStreamingService : IStreamingContract<int, MyCStreamingData>, IStreamingCallback<MyCStreamingData>
    {
        private readonly StreamingManager<int, MyCStreamingData> _streamingManager;

        public MyCStreamingService(IAdapterFactory<int, MyCStreamingData> adapterFactory)
        {
            var adapter = adapterFactory.GetAdapter();
            _streamingManager = new StreamingManager<int, MyCStreamingData>( data => data.Key, adapter);
        }

        public bool Subscribe(int key, IStreamingCallback<MyCStreamingData> callback)
        {
            return _streamingManager.SubscribeCallback(key, callback);
        }

        public bool Unsubscribe(int key, IStreamingCallback<MyCStreamingData> callback)
        {
            return _streamingManager.UnsubscribeCallback(key, callback);
        }

        public bool Unsubscribe(IStreamingCallback<MyCStreamingData> callback)
        {
            return _streamingManager.UnsubscribeCallback(callback);
        }

        public void Send(MyCStreamingData data)
        {
            _streamingManager.Send(data);
        }
    }
}