using Newtonsoft.Json;
using System;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Models.BatteryDemandService
{
    public sealed class Demand
    {
        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("counter")]
        public int Counter { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("details")]
        public DemandDetails Details { get; set; }

        [JsonProperty("isLive")]
        public bool IsLive { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("travelledDistanceReference")]
        public int TravelledDistanceReference { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("urgency")]
        public int Urgency { get; set; }
    }
}
