using Newtonsoft.Json;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Models.HighVoltageBattery
{
    public sealed class HighVoltageBatteryPassResponse
    {
        [JsonProperty("HvbUid")]
        public string HvbUid { get; set; }

        [JsonProperty("cellPerformanceId")]
        public string CellPerformanceId { get; set; }

        [JsonProperty("cellPerformanceCrc")]
        public string CellPerformanceCrc { get; set; }

        [JsonProperty("cellContactSystemId")]
        public string CellContactSystemId { get; set; }

        [JsonProperty("cellContactSystemCrc")]
        public string CellContactSystemCrc { get; set; }
    }
}
