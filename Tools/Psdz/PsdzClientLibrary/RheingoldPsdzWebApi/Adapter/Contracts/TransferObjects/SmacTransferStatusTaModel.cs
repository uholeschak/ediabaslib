using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class SmacTransferStatusTaModel : TaModel
    {
        [JsonProperty("smartActuatorIDs", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<string> SmartActuatorIDs { get; set; }

        [JsonProperty("smartActuatorFlashStatus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SmartActuatorFlashStatusModel> SmartActuatorFlashStatus { get; set; }
    }
}