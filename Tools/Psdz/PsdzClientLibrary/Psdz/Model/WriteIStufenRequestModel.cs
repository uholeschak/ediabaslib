using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
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