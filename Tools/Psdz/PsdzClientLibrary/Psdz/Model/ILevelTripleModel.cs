using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class ILevelTripleModel
    {
        [JsonProperty("current", NullValueHandling = NullValueHandling.Ignore)]
        public string Current { get; set; }

        [JsonProperty("last", NullValueHandling = NullValueHandling.Ignore)]
        public string Last { get; set; }

        [JsonProperty("shipment", NullValueHandling = NullValueHandling.Ignore)]
        public string Shipment { get; set; }
    }
}