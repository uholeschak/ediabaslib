using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class WriteFpRequestModel
    {
        [JsonProperty("fpAsString", NullValueHandling = NullValueHandling.Ignore)]
        public string FpAsString { get; set; }
    }
}