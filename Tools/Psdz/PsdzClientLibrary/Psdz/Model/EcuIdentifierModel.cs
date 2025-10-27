using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
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