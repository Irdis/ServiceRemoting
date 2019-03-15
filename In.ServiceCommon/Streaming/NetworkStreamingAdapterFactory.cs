using System;

namespace In.ServiceCommon.Streaming
{
    public class NetworkStreamingAdapterFactory<TKey, TData> : IAdapterFactory<TKey, TData>
    {
        private NetworkStreamingClientAdapter<TKey> _streamingAdapter;

        public NetworkStreamingAdapterFactory(String streamingType)
        {
            _streamingAdapter = new NetworkStreamingClientAdapter<TKey>(streamingType);
        }

        public IStreamingAdapter<TKey> GetAdapter()
        {
            return _streamingAdapter;
        }
    }
}