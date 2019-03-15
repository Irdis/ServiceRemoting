using System;
using In.ServiceCommon.Streaming;

namespace In.ServiceCommon.Client
{
    public class DelegateCallback : IStreamingCallback<object>
    {
        private Action<object> _delegate;

        public Type Type { get; set; }

        private void SetCallback<T>(IStreamingCallback<T> callback)
        {
            Type = typeof(T);
            _delegate = (data) => callback.Send((T)data);
        }

        public void Send(object data)
        {
            _delegate(data);
        }

        public static DelegateCallback Create<T>(IStreamingCallback<T> callback)
        {
            var c = new DelegateCallback();
            c.SetCallback(callback);
            return c;
        }
    }
}