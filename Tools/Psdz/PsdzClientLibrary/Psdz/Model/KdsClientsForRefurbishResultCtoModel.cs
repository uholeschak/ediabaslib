using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class KdsClientsForRefurbishResultCtoModel
    {
        [JsonProperty("kdsFailureResponseCto", NullValueHandling = NullValueHandling.Ignore)]
        public KdsFailureResponseCtoModel KdsFailureResponseCto { get; set; }

        [JsonProperty("kdsIds", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<KdsIdCtoModel> KdsIds { get; set; }
    }
}