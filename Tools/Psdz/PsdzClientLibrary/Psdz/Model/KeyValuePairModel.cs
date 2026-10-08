using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class KeyValuePairModel<TKey, TValue>
    {
        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public TKey Key { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public TValue Value { get; set; }
    }
}