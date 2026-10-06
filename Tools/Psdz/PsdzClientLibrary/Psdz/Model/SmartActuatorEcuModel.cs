using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class SmartActuatorEcuModel : EcuModel
    {
        [JsonProperty("smacID", NullValueHandling = NullValueHandling.Ignore)]
        public string SmacID { get; set; }

        [JsonProperty("smacMasterDiagAddress", NullValueHandling = NullValueHandling.Ignore)]
        public DiagAddressModel SmacMasterDiagAddress { get; set; }
    }
}