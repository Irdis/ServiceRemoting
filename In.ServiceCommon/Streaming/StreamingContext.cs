using System.Collections.Generic;

namespace In.ServiceCommon.Streaming
{
    public class StreamingContext<T>
    {
        private IList<IStreamingCallback<T>> _callbacks = new List<IStreamingCallback<T>>();

        public StreamingContext(IList<IStreamingCallback<T>> callbacks)
        {
            _callbacks = callbacks;
        }
    }
}