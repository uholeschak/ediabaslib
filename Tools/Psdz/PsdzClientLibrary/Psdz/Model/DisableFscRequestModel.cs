using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class DisableFscRequestModel
    {
        [JsonProperty("ecuIdentifier", NullValueHandling = NullValueHandling.Ignore)]
        public EcuIdentifierModel EcuIdentifier { get; set; }

        [JsonProperty("swtApplicationId", NullValueHandling = NullValueHandling.Ignore)]
        public SwtApplicationIdModel SwtApplicationId { get; set; }
    }
}