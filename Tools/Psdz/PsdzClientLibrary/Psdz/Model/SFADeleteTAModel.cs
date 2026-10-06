using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class SFADeleteTAModel : TaModel
    {
        [JsonProperty("estimatedExecutionTime", NullValueHandling = NullValueHandling.Ignore)]
        public long EstimatedExecutionTime { get; set; }

        [JsonProperty("featureId", NullValueHandling = NullValueHandling.Ignore)]
        public long FeatureId { get; set; }
    }
}