using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class ObdDataModel
    {
        [JsonProperty("obdTripleValues", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<ObdTripleValueModel> ObdTripleValues { get; set; }
    }
}