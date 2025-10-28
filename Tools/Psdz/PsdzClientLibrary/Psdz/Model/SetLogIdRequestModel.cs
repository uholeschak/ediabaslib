using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class SetLogIdRequestModel
    {
        [JsonProperty("logFilePath", NullValueHandling = NullValueHandling.Ignore)]
        public string LogFilePath { get; set; }
    }
}