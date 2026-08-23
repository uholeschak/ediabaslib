using Newtonsoft.Json;
using System;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Models.BatteryDemandService
{
    public sealed class CcmListItem
    {
        [JsonProperty("ccmId")]
        public string CcmId { get; set; }

        [JsonProperty("mileage")]
        public int Mileage { get; set; }

        [JsonProperty("priority")]
        public int Priority { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
