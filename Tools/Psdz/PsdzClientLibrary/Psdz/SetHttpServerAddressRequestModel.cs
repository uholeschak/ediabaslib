using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class SetHttpServerAddressRequestModel
    {
        [JsonProperty("serverAddress", NullValueHandling = NullValueHandling.Ignore)]
        public string ServerAddress { get; set; }
    }
}