using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    internal abstract class ProgrammingEnumMapperBase<TPsdz, TApi>
    {
        private readonly IDictionary<TPsdz, TApi> map;

        private readonly IDictionary<TApi, TPsdz> revertedMap;

        public IEnumerable<TApi> ApiEnumValues => revertedMap.Keys;

        public IEnumerable<TPsdz> PsdzEnumValues => map.Keys;

        protected ProgrammingEnumMapperBase()
        {
            map = CreateMap();
            revertedMap = ReverseMap(map);
        }

        internal virtual TPsdz GetValue(TApi key)
        {
            return revertedMap[key];
        }

        internal virtual TApi GetValue(TPsdz key)
        {
            return map[key];
        }

        protected abstract IDictionary<TPsdz, TApi> CreateMap();

        internal virtual IDictionary<TPsdz, TApi> CreateMapBase()
        {
            return Enum.GetValues(typeof(TPsdz)).Cast<TPsdz>().ToDictionary((TPsdz k) => k, (TPsdz v) => (TApi)Enum.Parse(typeof(TApi), Enum.GetName(typeof(TPsdz), v), ignoreCase: true));
        }

        private static IDictionary<TApi, TPsdz> ReverseMap(IEnumerable<KeyValuePair<TPsdz, TApi>> sourceMap)
        {
            return sourceMap.ToDictionary((KeyValuePair<TPsdz, TApi> x) => x.Value, (KeyValuePair<TPsdz, TApi> x) => x.Key);
        }
    }
}
