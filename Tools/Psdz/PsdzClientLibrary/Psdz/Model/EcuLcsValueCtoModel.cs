using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
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