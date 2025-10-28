using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class FillBntnNamesForMainSeriesRequestModel
    {
        [JsonProperty("baureihenverbund", NullValueHandling = NullValueHandling.Ignore)]
        public string Baureihenverbund { get; set; }

        [JsonProperty("svt", NullValueHandling = NullValueHandling.Ignore)]
        public StandardSvtModel Svt { get; set; }
    }
}