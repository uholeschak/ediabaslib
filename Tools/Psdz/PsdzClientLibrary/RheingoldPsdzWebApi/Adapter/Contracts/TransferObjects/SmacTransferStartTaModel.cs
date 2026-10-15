using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class SmacTransferStartTaModel : TaModel
    {
        [JsonProperty("smartActuatorData", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, ICollection<SgbmIdModel>> SmartActuatorData { get; set; }
    }
}