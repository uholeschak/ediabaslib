using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class FeatureIdCtoModel
    {
        [JsonProperty("asHexString", NullValueHandling = NullValueHandling.Ignore)]
        public string AsHexString { get; set; }

        [JsonProperty("featureId", NullValueHandling = NullValueHandling.Ignore)]
        public long FeatureId { get; set; }
    }
}