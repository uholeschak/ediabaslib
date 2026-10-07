using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class TargetSelectorModel
    {
        [JsonProperty("baureihenverbund", NullValueHandling = NullValueHandling.Ignore)]
        public string Baureihenverbund { get; set; }

        [JsonProperty("isDirect", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsDirect { get; set; }

        [JsonProperty("project", NullValueHandling = NullValueHandling.Ignore)]
        public string Project { get; set; }

        [JsonProperty("vehicleInfo", NullValueHandling = NullValueHandling.Ignore)]
        public string VehicleInfo { get; set; }
    }
}