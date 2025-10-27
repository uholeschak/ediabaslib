using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class VehicleIdModel
    {
        [JsonProperty("diagAddressString", NullValueHandling = NullValueHandling.Ignore)]
        public string DiagAddressString { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }
    }
}