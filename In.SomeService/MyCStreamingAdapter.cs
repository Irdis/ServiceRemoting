using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using In.ServiceCommon.Client;
using In.ServiceCommon.Streaming;

namespace In.SomeService
{
    public class MyCStreamingAdapter : IStreamingAdapter<int>, IClientAdapter<IStreamingCallback<MyCStreamingData>>
    {
        private IStreamingCallback<MyCStreamingData> _callback;

        private HashSet<int> _keys = new HashSet<int>();
        private Timer _timer;
        private volatile int _counter = 0;

        public MyCStreamingAdapter()
        {
        }

        private void OnTimer(object state)
        {
            lock (_keys)
            {
                foreach (var key in _keys)
                {
                    _callback.Send(new MyCStreamingData
                    {
                        Key = key,
                        Data = "Hello " + key + " " + _counter++,
                    });
                }
            }
        }


        public bool[] Subscribe(int[] keys)
        {
            lock (_keys)
            {
                foreach (var key in keys)
                {
                    _keys.Add(key);
                }
            }
            return keys.Select(i => true).ToArray();
        }

        public bool Subscribe(int key)
        {
            lock (_keys)
            {
                _keys.Add(key);
            }

            return true;
        }

        public bool[] Unsubscribe(int[] keys)
        {
            lock (_keys)
            {
                foreach (var key in keys)
                {
                    _keys.Remove(key);
                }
            }
            return keys.Select(i => true).ToArray();
        }

        public bool Unsubscribe(int keys)
        {
            lock (_keys)
            {
                _keys.Remove(keys);
            }
            return true;
        }
        
        public void SetCallback(IStreamingCallback<MyCStreamingData> callback)
        {
            _callback = callback;
            _timer = new Timer(OnTimer, null, TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(1));
        }
    }
}