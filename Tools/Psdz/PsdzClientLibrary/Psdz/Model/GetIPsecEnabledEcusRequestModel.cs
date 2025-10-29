using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class GetIPsecEnabledEcusRequestModel
    {
        [JsonProperty("svt", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel Svt { get; set; }
    }
}