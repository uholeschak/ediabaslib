using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class DefineSFAFilterForSelectedEcusRequestModel
    {
        [JsonProperty("ecuOptions", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<int, SfaPerEcuOptionsModel> EcuOptions { get; set; }

        [JsonProperty("inputTalFilter", NullValueHandling = NullValueHandling.Ignore)]
        public TalFilterModel InputTalFilter { get; set; }
    }
}
