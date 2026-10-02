using BMW.Authoring.Vehicle;
using Newtonsoft.Json;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Models.BatteryDemandService
{
    public sealed class DemandDetails
    {
        [JsonProperty("CCM")]
        public CCM CCM { get; set; }
    }
}
