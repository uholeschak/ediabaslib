using System;
using System.Collections.Generic;

namespace BmwDeepObd
{
    public class MultiMap<TKey, TValue>
    {
        private readonly Dictionary<TKey, IList<TValue>> _storage;

        public MultiMap()
        {
            _storage = new Dictionary<TKey, IList<TValue>>();
        }

        public void Add(TKey key, TValue value)
        {
            if (!_storage.ContainsKey(key))
            {
                _storage.Add(key, new List<TValue>());
            }
            _storage[key].Add(value);
        }

        public IEnumerable<TKey> Keys => _storage.Keys;

        public bool ContainsKey(TKey key)
        {
            return _storage.ContainsKey(key);
        }

        public IList<TValue> this[TKey key]
        {
            get
            {
                if (!_storage.ContainsKey(key))
                {
                    throw new KeyNotFoundException(string.Format("The given key {0} was not found in the collection.", key));
                }
                return _storage[key];
            }
        }

        public bool TryGetValue(TKey key, out IList<TValue> value)
        {
            return _storage.TryGetValue(key, out value);
        }

        public Dictionary<TKey, TValue> ToDictionary()
        {
            Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();
            foreach (TKey key in _storage.Keys)
            {
                try
                {
                    dict.Add(key, _storage[key][0]);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return dict;
        }
    }
}
