using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class TalFilterModel
    {
        [JsonProperty("asXml", NullValueHandling = NullValueHandling.Ignore)]
        public string AsXml { get; set; }
    }
}