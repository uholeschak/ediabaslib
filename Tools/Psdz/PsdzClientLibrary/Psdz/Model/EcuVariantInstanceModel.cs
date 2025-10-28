using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class EcuVariantInstanceModel : LogisticPartModel
    {
        [JsonProperty("combinedWith", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuVariantInstanceModel> CombinedWith { get; set; }

        [JsonProperty("ecu", NullValueHandling = NullValueHandling.Ignore)]
        public EcuModel Ecu { get; set; }

        [JsonProperty("orderablePart", NullValueHandling = NullValueHandling.Ignore)]
        public OrderPartModel OrderablePart { get; set; }
    }
}