using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    abstract class ProgrammingEnumMapperBase<TPsdz, TApi>
    {
        protected ProgrammingEnumMapperBase()
        {
            this.map = this.CreateMap();
            this.revertedMap = ProgrammingEnumMapperBase<TPsdz, TApi>.ReverseMap(this.map);
        }

        public IEnumerable<TApi> ApiEnumValues
        {
            get
            {
                return this.revertedMap.Keys;
            }
        }

        public IEnumerable<TPsdz> PsdzEnumValues
        {
            get
            {
                return this.map.Keys;
            }
        }

        internal virtual TPsdz GetValue(TApi key)
        {
            return this.revertedMap[key];
        }

        internal virtual TApi GetValue(TPsdz key)
        {
            return this.map[key];
        }

        protected abstract IDictionary<TPsdz, TApi> CreateMap();

        private static IDictionary<TApi, TPsdz> ReverseMap(IEnumerable<KeyValuePair<TPsdz, TApi>> sourceMap)
        {
            return sourceMap.ToDictionary((KeyValuePair<TPsdz, TApi> x) => x.Value, (KeyValuePair<TPsdz, TApi> x) => x.Key);
        }

        private readonly IDictionary<TPsdz, TApi> map;

        private readonly IDictionary<TApi, TPsdz> revertedMap;
    }
}
