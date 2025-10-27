using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class SmartActuatorMasterEcuModel : EcuModel
    {
        [JsonProperty("smacMasterSVK", NullValueHandling = NullValueHandling.Ignore)]
        public StandardSvkModel SmacMasterSVK { get; set; }

        [JsonProperty("smartActuatorEcus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SmartActuatorEcuModel> SmartActuatorEcus { get; set; }
    }
}