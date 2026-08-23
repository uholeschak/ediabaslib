using Newtonsoft.Json;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Models.BatteryDemandService
{
    public sealed class ServiceEvent
    {
        [JsonProperty("demand")]
        public Demand Demand { get; set; }

        [JsonProperty("eventIdentifier")]
        public string EventIdentifier { get; set; }
    }

}
