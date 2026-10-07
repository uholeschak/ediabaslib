using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class EcuUidCtoModel
    {
        [JsonProperty("ecuUid", NullValueHandling = NullValueHandling.Ignore)]
        public string EcuUid { get; set; }
    }
}