using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts
{
    public class ILevelModel
    {
        [JsonProperty("valid", NullValueHandling = NullValueHandling.Ignore)]
        public bool Valid { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }
    }
}