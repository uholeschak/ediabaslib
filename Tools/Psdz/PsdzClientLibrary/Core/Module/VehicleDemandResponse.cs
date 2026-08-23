using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Models.BatteryDemandService
{
    public sealed class VehicleDemandResponse
    {
        [JsonProperty("leads")]
        public List<Lead> Leads { get; set; }

        [JsonProperty("upcomingAppointments")]
        public List<object> UpcomingAppointments { get; set; }

        [JsonProperty("upcomingServiceEvents")]
        public List<object> UpcomingServiceEvents { get; set; }
    }
}
