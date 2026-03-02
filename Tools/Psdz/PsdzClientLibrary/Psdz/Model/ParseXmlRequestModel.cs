using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class ParseXmlRequestModel
    {
        [JsonProperty("xmlPathString", NullValueHandling = NullValueHandling.Ignore)]
        public string XmlPathString { get; set; }
    }
}