using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class SFADeleteTAModel : TaModel
    {
        [JsonProperty("estimatedExecutionTime", NullValueHandling = NullValueHandling.Ignore)]
        public long EstimatedExecutionTime { get; set; }

        [JsonProperty("featureId", NullValueHandling = NullValueHandling.Ignore)]
        public long FeatureId { get; set; }
    }
}