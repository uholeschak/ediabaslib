using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class ParseXmlRequestModel
    {
        [JsonProperty("xmlPathString", NullValueHandling = NullValueHandling.Ignore)]
        public string XmlPathString { get; set; }
    }
}