using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class OrderPartModel : LogisticPartModel
    {
        [JsonProperty("deliverables", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<LogisticPartModel> Deliverables { get; set; }

        [JsonProperty("pattern", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<LogisticPartModel> Pattern { get; set; }
    }
}