using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class StoreFscRequestModel
    {
        [JsonProperty("ecuIdentifier", NullValueHandling = NullValueHandling.Ignore)]
        public EcuIdentifierModel EcuIdentifier { get; set; }

        [JsonProperty("fsc", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] Fsc { get; set; }

        [JsonProperty("swtApplicationId", NullValueHandling = NullValueHandling.Ignore)]
        public SwtApplicationIdModel SwtApplicationId { get; set; }
    }
}