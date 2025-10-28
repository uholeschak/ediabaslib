using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class SetThresholdRequestModel
    {
        [JsonProperty("threshold", NullValueHandling = NullValueHandling.Ignore)]
        public int Threshold { get; set; }
    }
}