using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class SwtActionModel
    {
        [JsonProperty("swtEcus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SwtEcuModel> SwtEcus { get; set; }
    }
}