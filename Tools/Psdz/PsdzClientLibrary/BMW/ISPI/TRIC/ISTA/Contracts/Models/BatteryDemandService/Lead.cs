using BMW.Rheingold.CoreFramework.DatabaseProvider.Dealer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Models.BatteryDemandService
{
    public sealed class Lead
    {
        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("dealer")]
        public Dealer Dealer { get; set; }

        [JsonProperty("leadKey")]
        public string LeadKey { get; set; }

        [JsonProperty("leadTypeName")]
        public string LeadTypeName { get; set; }

        [JsonProperty("overallUrgency")]
        public int OverallUrgency { get; set; }

        [JsonProperty("previousNotifications")]
        public List<object> PreviousNotifications { get; set; }

        [JsonProperty("primarySuggestedCustomerAction")]
        public string PrimarySuggestedCustomerAction { get; set; }

        [JsonProperty("secondarySuggestedCustomerActions")]
        public List<object> SecondarySuggestedCustomerActions { get; set; }

        [JsonProperty("serviceEvents")]
        public List<ServiceEvent> ServiceEvents { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("vin")]
        public string Vin { get; set; }
    }
}
