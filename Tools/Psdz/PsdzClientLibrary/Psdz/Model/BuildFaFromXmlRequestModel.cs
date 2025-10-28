using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class BuildFaFromXmlRequestModel
    {
        [JsonProperty("xml", NullValueHandling = NullValueHandling.Ignore)]
        public string Xml { get; set; }
    }
}