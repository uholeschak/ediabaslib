using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class WriteIStufenRequestModel
    {
        [JsonProperty("istufeCurrent", NullValueHandling = NullValueHandling.Ignore)]
        public string IStufeCurrent { get; set; }

        [JsonProperty("istufeLast", NullValueHandling = NullValueHandling.Ignore)]
        public string IStufeLast { get; set; }

        [JsonProperty("istufeShipment", NullValueHandling = NullValueHandling.Ignore)]
        public string IStufeShipment { get; set; }
    }
}