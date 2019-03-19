using System;
using In.ServiceCommon.Client;
using In.ServiceCommon.Network;
using In.ServiceCommon.Service;

namespace In.ServiceCommon.Streaming
{
    public class NetworkStreamingServerAdapter<TKey> : IStreamingAdapter<TKey>, IDisposable
    {
        private readonly DelegateContract _contract;
        private readonly object _callback;

        public NetworkStreamingServerAdapter(Type implementationType, NetworkChannel channel, ServiceMessageBuilder builder, DelegateContract contract)
        {
            var streamingCallback = typeof(NetworkStreamingCallback<>).MakeGenericType(contract.ValueType);
            _callback = Activator.CreateInstance(streamingCallback, new object[]
            {
                contract.StreamingType,
                builder,
                channel,
                implementationType
            });
            _contract = contract;
        }

        public bool[] Subscribe(TKey[] keys)
        {
            var result = new bool[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                result[i] = _contract.Subscribe(keys[i], _callback);
            }
            return result;
        }

        public bool Subscribe(TKey key)
        {
            return _contract.Subscribe(key, _callback);
        }

        public bool[] Unsubscribe(TKey[] keys)
        {
            var result = new bool[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                result[i] = _contract.Unsubscribe(keys[i], _callback);
            }
            return result;
        }

        public bool Unsubscribe(TKey keys)
        {
            return _contract.Unsubscribe(keys, _callback);
        }

        public void Dispose()
        {
            _contract.Unsubscribe(_callback);
        }
    }
}