using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class CalculateBindingDistributionRequestModel
    {
        [JsonProperty("bindingsFromCbb", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SecurityMemoryObjectEtoModel> BindingsFromCbb { get; set; }

        [JsonProperty("bindingsFromVehicle", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SecurityMemoryObjectEtoModel> BindingsFromVehicle { get; set; }
    }
}