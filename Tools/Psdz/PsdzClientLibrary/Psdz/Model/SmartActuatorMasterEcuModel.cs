using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class SmartActuatorMasterEcuModel : EcuModel
    {
        [JsonProperty("smacMasterSVK", NullValueHandling = NullValueHandling.Ignore)]
        public StandardSvkModel SmacMasterSVK { get; set; }

        [JsonProperty("smartActuatorEcus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SmartActuatorEcuModel> SmartActuatorEcus { get; set; }
    }
}