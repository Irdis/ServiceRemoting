using System;
using System.Linq;
using In.ServiceCommon.Client;

namespace In.ServiceCommon.Streaming
{
    public class NetworkStreamingClientAdapter<TKey> : ClientProxyBase, IStreamingAdapter<TKey>
    {
        private const string Sub = "sub";
        private const string Unsub = "unsub";
        private readonly string _streamingType;

        public string StreamingType => _streamingType;

        public NetworkStreamingClientAdapter(string streamingType)
        {
            _streamingType = streamingType;
        }

        public bool[] Subscribe(TKey[] keys)
        {
            return (bool[])ServiceProxy.Call(_streamingType, Sub, new object[]{keys});
        }

        public bool Subscribe(TKey key)
        {
            return Subscribe(new[] {key})[0];
        }

        public bool[] Unsubscribe(TKey[] keys)
        {
            return (bool[])ServiceProxy.Call(_streamingType, Unsub, new object[]{keys});
        }

        public bool Unsubscribe(TKey keys)
        {
            return Unsubscribe(new[] {keys})[0];
        }
    }
}