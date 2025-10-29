using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class WriteSvtRequestModel
    {
        [JsonProperty("standardSvt", NullValueHandling = NullValueHandling.Ignore)]
        public StandardSvtModel StandardSvt { get; set; }
    }
}