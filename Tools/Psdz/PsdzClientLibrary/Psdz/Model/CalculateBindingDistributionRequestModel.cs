using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class CalculateBindingDistributionRequestModel
    {
        [JsonProperty("bindingsFromCbb", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SecurityMemoryObjectEtoModel> BindingsFromCbb { get; set; }

        [JsonProperty("bindingsFromVehicle", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SecurityMemoryObjectEtoModel> BindingsFromVehicle { get; set; }
    }
}