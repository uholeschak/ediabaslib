using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class SmacTransferStartTaModel : TaModel
    {
        [JsonProperty("smartActuatorData", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, ICollection<SgbmIdModel>> SmartActuatorData { get; set; }
    }
}