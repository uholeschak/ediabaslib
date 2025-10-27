using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
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