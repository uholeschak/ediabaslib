using Newtonsoft.Json;
using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class SwtActionModel
    {
        [JsonProperty("swtEcus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SwtEcuModel> SwtEcus { get; set; }
    }
}