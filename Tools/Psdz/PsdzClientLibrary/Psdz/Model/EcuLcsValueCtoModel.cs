using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class EcuLcsValueCtoModel
    {
        [JsonProperty("ecuIdentifier", NullValueHandling = NullValueHandling.Ignore)]
        public EcuIdentifierModel EcuIdentifier { get; set; }

        [JsonProperty("lcsNumber", NullValueHandling = NullValueHandling.Ignore)]
        public int LcsNumber { get; set; }

        [JsonProperty("lcsValue", NullValueHandling = NullValueHandling.Ignore)]
        public int LcsValue { get; set; }
    }
}