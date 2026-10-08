using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class EcuResetMapping
    {
        [JsonProperty("ecu", NullValueHandling = NullValueHandling.Ignore)]
        public EcuIdentifierCtoModel Ecu { get; set; }

        [JsonProperty("resetType", NullValueHandling = NullValueHandling.Ignore)]
        public ResetTypeEto ResetType { get; set; }
    }
}