using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class EcuUidCtoModel
    {
        [JsonProperty("ecuUid", NullValueHandling = NullValueHandling.Ignore)]
        public string EcuUid { get; set; }
    }
}