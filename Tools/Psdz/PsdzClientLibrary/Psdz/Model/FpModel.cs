using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class FpModel : StandardFpModel
    {
        [JsonProperty("baureihenverbund", NullValueHandling = NullValueHandling.Ignore)]
        public string Baureihenverbund { get; set; }

        [JsonProperty("entwicklungsbaureihe", NullValueHandling = NullValueHandling.Ignore)]
        public string Entwicklungsbaureihe { get; set; }
    }
}