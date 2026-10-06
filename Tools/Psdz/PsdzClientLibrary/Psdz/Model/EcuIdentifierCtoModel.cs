using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class EcuIdentifierCtoModel
    {
        [JsonProperty("baseVariant", NullValueHandling = NullValueHandling.Ignore)]
        public string BaseVariant { get; set; }

        [JsonProperty("diagAddress", NullValueHandling = NullValueHandling.Ignore)]
        public DiagAddressCtoModel DiagAddress { get; set; }
    }
}