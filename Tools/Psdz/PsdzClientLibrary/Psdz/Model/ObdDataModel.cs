using Newtonsoft.Json;
using System.Collections.Generic;
using BMW.Rheingold.Psdz;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class ObdDataModel
    {
        [JsonProperty("obdTripleValues", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<ObdTripleValueModel> ObdTripleValues { get; set; }
    }
}