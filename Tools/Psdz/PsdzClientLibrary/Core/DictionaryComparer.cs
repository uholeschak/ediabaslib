using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace PsdzClient
{
    [DataContract]
    public class DictionaryComparer<TKey, TValue> : IEqualityComparer<Dictionary<TKey, TValue>>
    {
        private IEqualityComparer<TValue> valueComparer;

        public DictionaryComparer(IEqualityComparer<TValue> valueComparer = null)
        {
            this.valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;
        }

        public bool Equals(Dictionary<TKey, TValue> x, Dictionary<TKey, TValue> y)
        {
            if ((x == null && y != null) || (x != null && y == null))
            {
                return false;
            }
            if (x == null && y == null)
            {
                return true;
            }
            if (x.Count != y.Count)
            {
                return false;
            }
            if (x.Keys.Except(y.Keys).Any())
            {
                return false;
            }
            if (y.Keys.Except(x.Keys).Any())
            {
                return false;
            }
            foreach (KeyValuePair<TKey, TValue> item in x)
            {
                if (!valueComparer.Equals(item.Value, y[item.Key]))
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(Dictionary<TKey, TValue> obj)
        {
            throw new NotImplementedException();
        }
    }
}
