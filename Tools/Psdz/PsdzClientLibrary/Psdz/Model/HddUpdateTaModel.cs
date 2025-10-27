using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class HddUpdateTaModel : TaModel
    {
        [JsonProperty("secondsToCompletion", NullValueHandling = NullValueHandling.Ignore)]
        public long SecondsToCompletion { get; set; }
    }
}