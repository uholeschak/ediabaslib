using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class FeatureIdCtoModel
    {
        [JsonProperty("asHexString", NullValueHandling = NullValueHandling.Ignore)]
        public string AsHexString { get; set; }

        [JsonProperty("featureId", NullValueHandling = NullValueHandling.Ignore)]
        public long FeatureId { get; set; }
    }
}