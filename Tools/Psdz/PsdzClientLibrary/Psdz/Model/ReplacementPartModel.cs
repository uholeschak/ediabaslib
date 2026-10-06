using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class ReplacementPartModel : LogisticPartModel
    {
        [JsonProperty("deliverables", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<LogisticPartModel> Deliverables { get; set; }
    }
}