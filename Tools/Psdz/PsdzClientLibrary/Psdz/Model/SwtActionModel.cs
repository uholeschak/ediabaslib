using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class SwtActionModel
    {
        [JsonProperty("swtEcus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SwtEcuModel> SwtEcus { get; set; }
    }
}