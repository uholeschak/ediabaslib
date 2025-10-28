using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class PerformQuickKdsCheckRequestModel
    {
        [JsonProperty("kdsId", NullValueHandling = NullValueHandling.Ignore)]
        public KdsIdCtoModel KdsId { get; set; }

        [JsonProperty("retries", NullValueHandling = NullValueHandling.Ignore)]
        public int Retries { get; set; }

        [JsonProperty("timeBetweenRetries", NullValueHandling = NullValueHandling.Ignore)]
        public int TimeBetweenRetries { get; set; }
    }
}