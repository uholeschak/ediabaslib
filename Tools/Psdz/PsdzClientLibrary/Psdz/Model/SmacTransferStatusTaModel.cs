using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class SmacTransferStatusTaModel : TaModel
    {
        [JsonProperty("smartActuatorIDs", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<string> SmartActuatorIDs { get; set; }
    }
}