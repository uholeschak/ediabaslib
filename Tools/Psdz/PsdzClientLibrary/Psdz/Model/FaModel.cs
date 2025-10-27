using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class FaModel : StandardFaModel
    {
        [JsonProperty("asXml", NullValueHandling = NullValueHandling.Ignore)]
        public string AsXml { get; set; }
    }
}