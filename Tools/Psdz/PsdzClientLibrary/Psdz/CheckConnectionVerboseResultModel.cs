using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class CheckConnectionVerboseResultModel
    {
        [JsonProperty("connectionWorking", NullValueHandling = NullValueHandling.Ignore)]
        public bool ConnectionWorking { get; set; }

        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }
    }
}