using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class CheckNcdResultEtoModel
    {
        [JsonProperty("detailedNcdStatus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<DetailedNcdInfoEtoModel> DetailedNcdStatus { get; set; }

        [JsonProperty("eachNcdSigned", NullValueHandling = NullValueHandling.Ignore)]
        public bool EachNcdSigned { get; set; }
    }
}