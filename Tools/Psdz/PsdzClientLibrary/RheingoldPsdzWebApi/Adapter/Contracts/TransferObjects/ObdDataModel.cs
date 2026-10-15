using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class ObdDataModel
    {
        [JsonProperty("obdTripleValues", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<ObdTripleValueModel> ObdTripleValues { get; set; }
    }
}