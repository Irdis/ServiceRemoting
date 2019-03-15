using System;
using In.ServiceCommon.Streaming;

namespace In.ServiceCommon.Service
{
    public class DelegateContract
    {
        private Func<object, object, bool> _subscribe;
        private Func<object, object, bool> _unsubscribe;
        private Func<object, bool> _unsubscribeCallback;

        public Type KeyType { get; set; }
        public Type ValueType { get; set; }

        public static DelegateContract Create<TKey, TData>(IStreamingContract<TKey, TData> contract)
        {
            var delegateContract = new DelegateContract();
            delegateContract.KeyType = typeof(TKey);
            delegateContract.ValueType = typeof(TData);
            delegateContract._subscribe = (key, callback) => contract.Subscribe((TKey) key, (IStreamingCallback<TData>) callback); 
            delegateContract._unsubscribe = (key, callback) => contract.Unsubscribe((TKey) key, (IStreamingCallback<TData>) callback); 
            delegateContract._unsubscribeCallback = (callback) => contract.Unsubscribe((IStreamingCallback<TData>) callback);
            return delegateContract;
        }

        public bool Subscribe(object key, object callback)
        {
            return _subscribe(key, callback);
        }

        public bool Unsubscribe(object key, object callback)
        {
            return _unsubscribe(key, callback);
        }

        public bool Unsubscribe(object callback)
        {
            return _unsubscribeCallback(callback);
        }
    }
}