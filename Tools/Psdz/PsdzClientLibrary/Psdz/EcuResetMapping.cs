using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using PsdzClient.Psdz.Model;

namespace BMW.Rheingold.Psdz
{
    public class EcuResetMapping
    {
        [JsonProperty("ecu", NullValueHandling = NullValueHandling.Ignore)]
        public EcuIdentifierCtoModel Ecu { get; set; }

        [JsonProperty("resetType", NullValueHandling = NullValueHandling.Ignore)]
        public ResetTypeEto ResetType { get; set; }
    }
}