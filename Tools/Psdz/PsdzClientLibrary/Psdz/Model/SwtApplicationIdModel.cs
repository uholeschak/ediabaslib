using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class SwtApplicationIdModel
    {
        [JsonProperty("applicationNumber", NullValueHandling = NullValueHandling.Ignore)]
        public int ApplicationNumber { get; set; }

        [JsonProperty("upgradeIndex", NullValueHandling = NullValueHandling.Ignore)]
        public int UpgradeIndex { get; set; }
    }
}