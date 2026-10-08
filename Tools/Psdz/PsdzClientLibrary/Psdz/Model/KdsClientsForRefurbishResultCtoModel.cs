using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class KdsClientsForRefurbishResultCtoModel
    {
        [JsonProperty("kdsFailureResponseCto", NullValueHandling = NullValueHandling.Ignore)]
        public KdsFailureResponseCtoModel KdsFailureResponseCto { get; set; }

        [JsonProperty("kdsIds", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<KdsIdCtoModel> KdsIds { get; set; }
    }
}