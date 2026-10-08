using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class ReplacementPartModel : LogisticPartModel
    {
        [JsonProperty("deliverables", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<LogisticPartModel> Deliverables { get; set; }
    }
}