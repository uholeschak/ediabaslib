using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class DiagAddressModel
    {
        [JsonProperty("offset", NullValueHandling = NullValueHandling.Ignore)]
        public int Offset { get; set; }
    }
}