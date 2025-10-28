using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class KdsFailureResponseCtoModel
    {
        [JsonProperty("cause", NullValueHandling = NullValueHandling.Ignore)]
        public LocalizableMessageToModel Cause { get; set; }

        [JsonProperty("kdsIdCto", NullValueHandling = NullValueHandling.Ignore)]
        public KdsIdCtoModel KdsIdCto { get; set; }
    }
}