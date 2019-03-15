using System;
using System.Collections.Generic;
using In.ServiceCommon.Network;

namespace In.ServiceCommon.Service
{
    public class NetworkAdapterService
    {
        public const string SubSignature = "sub";
        public const string UnsubSignature = "unsub";

        private readonly object _lock = new object();
        private readonly DelegateContract _delegateContract;
        private readonly Dictionary<INetworkChannel, object> _adapters = new Dictionary<INetworkChannel, object>();

        public string StreamingType { get; }

        public NetworkAdapterService(string streamingType, DelegateContract delegateContract)
        {
            StreamingType = streamingType;
            _delegateContract = delegateContract;
        }

        public Type GetArgumentType()
        {
            return _delegateContract.KeyType;
        }

        public void Add(object adapter, INetworkChannel networkChannel)
        {
            lock (_lock)
            {
                _adapters[networkChannel] = adapter;
            }
        }

        public void Remove(INetworkChannel networkChannel)
        {
            lock (_lock)
            {
                var adapter = _adapters[networkChannel];
                ((IDisposable) adapter).Dispose();
                _adapters.Remove(networkChannel);
            }
        }

        public bool[] Subscribe(object keys, INetworkChannel channel)
        {
            lock (_lock)
            {
                bool[] result = null;
                if (_adapters.TryGetValue(channel, out var adapter))
                {
                    result = (bool[]) adapter.GetType().GetMethod("Subscribe", new []{ keys.GetType() }).Invoke(adapter, new[] {keys});
                }

                return result;
            }
        }


        public bool[] Unsubscribe(object keys, INetworkChannel channel)
        {
            lock (_lock)
            {
                bool[] result = null;
                if (_adapters.TryGetValue(channel, out var adapter))
                {
                    result = (bool[]) adapter.GetType().GetMethod("Unsubscribe", new []{ keys.GetType() }).Invoke(adapter, new[] {keys});
                }

                return result;
            }
        }
    }
}