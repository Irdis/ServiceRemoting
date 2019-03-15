using System.Collections.Generic;

namespace In.ServiceCommon.Streaming
{
    public class SubscriptionList<TData>
    {
        private List<IStreamingCallback<TData>> _subscriptions = new List<IStreamingCallback<TData>>();

        public bool IsEmpty
        {
            get { return _subscriptions.Count == 0; }
        }

        public IEnumerable<IStreamingCallback<TData>> Callbacks
        {
            get { return _subscriptions; }
        }

        public void AddSubscription(IStreamingCallback<TData> subsciption)
        {
            _subscriptions.Add(subsciption);
        }

        public void RemoveSubscription(IStreamingCallback<TData> subscription)
        {
            _subscriptions.Remove(subscription);
        }
    }
}