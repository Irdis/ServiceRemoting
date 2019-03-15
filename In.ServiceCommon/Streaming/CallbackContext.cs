using System.Collections.Generic;

namespace In.ServiceCommon.Streaming
{
    public class CallbackContext<TKey>
    {
        private readonly HashSet<TKey> _keys = new HashSet<TKey>();

        public IEnumerable<TKey> Keys
        {
            get { return _keys; }
        }

        public bool IsEmpty
        {
            get { return _keys.Count == 0; }
        }

        public bool HasKey(TKey key)
        {
            return _keys.Contains(key);
        }

        public void AddKey(TKey key)
        {
            _keys.Add(key);
        }

        public void RemoveKey(TKey key)
        {
            _keys.Remove(key);
        }
    }
}