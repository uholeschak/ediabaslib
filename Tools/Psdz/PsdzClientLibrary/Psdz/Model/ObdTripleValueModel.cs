using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class ObdTripleValueModel
    {
        [JsonProperty("calId", NullValueHandling = NullValueHandling.Ignore)]
        public string CalId { get; set; }

        [JsonProperty("obdId", NullValueHandling = NullValueHandling.Ignore)]
        public string ObdId { get; set; }

        [JsonProperty("subCVN", NullValueHandling = NullValueHandling.Ignore)]
        public string SubCVN { get; set; }
    }
}