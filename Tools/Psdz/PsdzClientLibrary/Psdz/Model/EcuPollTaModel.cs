using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class EcuPollTaModel : TaModel
    {
        [JsonProperty("estimatedExecutionTime", NullValueHandling = NullValueHandling.Ignore)]
        public long EstimatedExecutionTime { get; set; }
    }
}