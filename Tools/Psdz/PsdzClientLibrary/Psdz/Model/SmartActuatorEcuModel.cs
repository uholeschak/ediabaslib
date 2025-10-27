using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class SmartActuatorEcuModel : EcuModel
    {
        [JsonProperty("smacID", NullValueHandling = NullValueHandling.Ignore)]
        public string SmacID { get; set; }

        [JsonProperty("smacMasterDiagAddress", NullValueHandling = NullValueHandling.Ignore)]
        public DiagAddressModel SmacMasterDiagAddress { get; set; }
    }
}