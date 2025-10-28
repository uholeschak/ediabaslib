using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class EcuCheckingMaxWaitingTimeResultModel
    {
        [JsonProperty("ecuIdentifierModel", NullValueHandling = NullValueHandling.Ignore)]
        public EcuIdentifierModel EcuIdentifierModel { get; set; }

        [JsonProperty("maxWaitingTime", NullValueHandling = NullValueHandling.Ignore)]
        public int MaxWaitingTime { get; set; }
    }
}