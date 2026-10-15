using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class PerformQuickKdsCheckSP25RequestModel
    {
        [JsonProperty("kdsId", NullValueHandling = NullValueHandling.Ignore)]
        public KdsIdCtoModel KdsId { get; set; }

        [JsonProperty("retries", NullValueHandling = NullValueHandling.Ignore)]
        public int Retries { get; set; }

        [JsonProperty("timeBetweenRetries", NullValueHandling = NullValueHandling.Ignore)]
        public int TimeBetweenRetries { get; set; }
    }
}