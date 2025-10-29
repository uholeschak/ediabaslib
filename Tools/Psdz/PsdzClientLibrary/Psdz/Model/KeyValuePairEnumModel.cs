using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BMW.Rheingold.Psdz
{
    public class KeyValuePairEnumModel<TKey, TValue>
    {
        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public TKey Key { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public TValue Value { get; set; }
    }
}