using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class SwtApplicationIdModel
    {
        [JsonProperty("applicationNumber", NullValueHandling = NullValueHandling.Ignore)]
        public int ApplicationNumber { get; set; }

        [JsonProperty("upgradeIndex", NullValueHandling = NullValueHandling.Ignore)]
        public int UpgradeIndex { get; set; }
    }
}