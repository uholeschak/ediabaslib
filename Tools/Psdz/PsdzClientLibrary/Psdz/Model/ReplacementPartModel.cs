using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class ReplacementPartModel : LogisticPartModel
    {
        [JsonProperty("deliverables", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<LogisticPartModel> Deliverables { get; set; }
    }
}