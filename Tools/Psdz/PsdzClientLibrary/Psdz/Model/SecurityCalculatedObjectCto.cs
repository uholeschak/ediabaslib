using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class SecurityCalculatedObjectCto
    {
        [JsonProperty("keyIdStatus", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, SecurityCalculationDetailedStatusEto> KeyIdStatus { get; set; }

        [JsonProperty("memoryObject", NullValueHandling = NullValueHandling.Ignore)]
        public SecurityMemoryObjectEtoModel MemoryObject { get; set; }

        [JsonProperty("overallStatus", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public SecurityCalculationOverallStatusEto OverallStatus { get; set; }

        [JsonProperty("roleStatus", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, SecurityCalculationDetailedStatusEto> RoleStatus { get; set; }

        [JsonProperty("servicePack", NullValueHandling = NullValueHandling.Ignore)]
        public string ServicePack { get; set; }
    }
}