using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class RequestNcdEtoModel
    {
        [JsonProperty("btld", NullValueHandling = NullValueHandling.Ignore)]
        public SgbmIdModel Btld { get; set; }

        [JsonProperty("cafd", NullValueHandling = NullValueHandling.Ignore)]
        public SgbmIdModel Cafd { get; set; }
    }
}