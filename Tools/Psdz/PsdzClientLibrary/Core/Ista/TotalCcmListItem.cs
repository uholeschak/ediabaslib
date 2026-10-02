using Newtonsoft.Json;
using System;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Models.BatteryDemandService
{
    public sealed class TotalCcmListItem
    {
        [JsonProperty("ccmId")]
        public int CcmId { get; set; }

        [JsonProperty("lastMileage")]
        public int LastMileage { get; set; }

        [JsonProperty("lastOccurrenceTimestamp")]
        public DateTime LastOccurrenceTimestamp { get; set; }

        [JsonProperty("occurrences")]
        public int Occurrences { get; set; }
    }
}
