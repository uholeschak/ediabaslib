using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class ReadLcsRequestModel
    {
        [JsonProperty("svt", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel Svt { get; set; }

        [JsonProperty("whitelistedEcus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuIdentifierModel> WhitelistedEcus { get; set; }
    }
}