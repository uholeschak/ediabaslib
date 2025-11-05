using System.Collections.Generic;
using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal abstract class MapperBase<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> map;
        private readonly IDictionary<TValue, TKey> revertedMap;
        protected MapperBase()
        {
            map = CreateMap();
            revertedMap = ReverseMap(map);
        }

        internal virtual TKey GetValue(TValue key)
        {
            return revertedMap[key];
        }

        internal virtual TValue GetValue(TKey key)
        {
            return map[key];
        }

        protected abstract IDictionary<TKey, TValue> CreateMap();
        private static IDictionary<TValue, TKey> ReverseMap(IEnumerable<KeyValuePair<TKey, TValue>> sourceMap)
        {
            return sourceMap.ToDictionary((KeyValuePair<TKey, TValue> x) => x.Value, (KeyValuePair<TKey, TValue> x) => x.Key);
        }
    }
}