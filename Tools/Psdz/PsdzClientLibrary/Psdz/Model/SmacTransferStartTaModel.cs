using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class SmacTransferStartTaModel : TaModel
    {
        [JsonProperty("smartActuatorData", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, ICollection<SgbmIdModel>> SmartActuatorData { get; set; }
    }
}