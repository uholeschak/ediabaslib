using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts
{
    public class VinModel
    {
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }
    }
}