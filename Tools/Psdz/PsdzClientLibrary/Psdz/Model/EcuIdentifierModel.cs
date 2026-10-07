using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class EcuIdentifierModel
    {
        [JsonProperty("baseVariant", NullValueHandling = NullValueHandling.Ignore)]
        public string BaseVariant { get; set; }

        [JsonProperty("diagAddrAsInt", NullValueHandling = NullValueHandling.Ignore)]
        public int DiagAddrAsInt { get; set; }

        [JsonProperty("diagnosisAddress", NullValueHandling = NullValueHandling.Ignore)]
        public DiagAddressModel DiagnosisAddress { get; set; }
    }
}