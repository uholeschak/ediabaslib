using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class SetHttpServerPortRequestModel
    {
        [JsonProperty("serverPort", NullValueHandling = NullValueHandling.Ignore)]
        public int ServerPort { get; set; }
    }
}