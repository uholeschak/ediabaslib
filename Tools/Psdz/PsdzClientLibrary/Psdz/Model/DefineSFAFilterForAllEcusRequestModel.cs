using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class DefineSFAFilterForAllEcusRequestModel
    {
        [JsonProperty("inputTalFilter", NullValueHandling = NullValueHandling.Ignore)]
        public TalFilterModel InputTalFilter { get; set; }

        [JsonProperty("ecuOptions", NullValueHandling = NullValueHandling.Ignore)]
        public SfaPerEcuOptionsModel EcuOptions { get; set; }
    }
}
