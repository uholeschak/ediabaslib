using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class BuildSwtActionFromXmlRequestModel
    {
        [JsonProperty("xml", NullValueHandling = NullValueHandling.Ignore)]
        public string Xml { get; set; }
    }
}