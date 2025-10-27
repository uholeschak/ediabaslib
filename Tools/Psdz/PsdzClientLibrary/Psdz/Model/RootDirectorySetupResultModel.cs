using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class RootDirectorySetupResultModel
    {
        [JsonProperty("success", NullValueHandling = NullValueHandling.Ignore)]
        public bool Success { get; set; }

        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }
    }
}