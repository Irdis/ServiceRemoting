using System;
using System.Collections.Concurrent;
using System.Linq;

namespace In.ServiceCommon.Streaming
{
    public class StreamingManager<TKey, TData> : IStreamingCallback<TData>
    {
        private readonly Func<TData, TKey> _keyFunc;
        private readonly IStreamingAdapter<TKey> _streamingAdapter;

        private readonly ConcurrentDictionary<TKey, SubscriptionList<TData>> _subscriptions = new ConcurrentDictionary<TKey, SubscriptionList<TData>>();
        private readonly ConcurrentDictionary<IStreamingCallback<TData>, CallbackContext<TKey>> _callbackContexts = new ConcurrentDictionary<IStreamingCallback<TData>, CallbackContext<TKey>>();

        public StreamingManager(Func<TData, TKey> keyFunc, IStreamingAdapter<TKey> streamingAdapter)
        {
            _keyFunc = keyFunc;
            _streamingAdapter = streamingAdapter;
        }

        public bool SubscribeCallback(TKey key, IStreamingCallback<TData> callback)
        {
            var callbackContext = GetCallbackContext(callback);
            var subscription = GetSubscription(key);
            lock (callbackContext)
                lock (subscription)
                {
                    var subscribe = subscription.IsEmpty;
                    callbackContext.AddKey(key);
                    subscription.AddSubscription(callback);
                    if (subscribe)
                    {
                        return _streamingAdapter.Subscribe(key);
                    }
                }

            return true;
        }

        public bool UnsubscribeCallback(TKey key, IStreamingCallback<TData> callback)
        {
            var callbackContext = GetCallbackContext(callback);
            var subscription = GetSubscription(key);
            var unsubscribe = false;
            lock (callbackContext)
                lock (subscription)
                {
                    callbackContext.RemoveKey(key);
                    subscription.RemoveSubscription(callback);
                    if (subscription.IsEmpty)
                    {
                        _subscriptions.TryRemove(key, out _);
                        unsubscribe = true;
                    }

                    if (callbackContext.IsEmpty)
                    {
                        _callbackContexts.TryRemove(callback, out _);
                    }

                    if (unsubscribe)
                    {
                        return _streamingAdapter.Unsubscribe(key);
                    }
                }

            return true;
        }


        public bool UnsubscribeCallback(IStreamingCallback<TData> callback)
        {
            var callbackContext = GetCallbackContext(callback);
            var unsubscribe = false;
            lock (callbackContext)
            {
                foreach (var contextKey in callbackContext.Keys)
                {
                    var subscription = GetSubscription(contextKey);
                    lock (subscription)
                    {
                        subscription.RemoveSubscription(callback);
                        if (subscription.IsEmpty)
                        {
                            _subscriptions.TryRemove(contextKey, out _);
                            unsubscribe = true;
                        }
                        if (unsubscribe)
                        {
                            return _streamingAdapter.Unsubscribe(contextKey);
                        }
                    }
                }
            }

            return true;
        }

        private CallbackContext<TKey> GetCallbackContext(IStreamingCallback<TData> callback)
        {
            return _callbackContexts.GetOrAdd(callback, streamingCallback => new CallbackContext<TKey>());
        }

        private SubscriptionList<TData> GetSubscription(TKey key)
        {
            return _subscriptions.GetOrAdd(key, streamingCallback => new SubscriptionList<TData>());
        }

        public void Send(TData data)
        {
            var key = _keyFunc(data);
            var subscriptions = GetSubscription(key);
            IStreamingCallback<TData>[] callbacks;
            lock (subscriptions)
            {
                callbacks = subscriptions.Callbacks.ToArray();
            }
            foreach (var callback in callbacks)
            {
                if (!_callbackContexts.TryGetValue(callback, out var context))
                {
                    continue;
                }

                lock (context)
                {
                    if (context.HasKey(key))
                    {
                        callback.Send(data);
                    }
                }
            }
        }
    }
}
