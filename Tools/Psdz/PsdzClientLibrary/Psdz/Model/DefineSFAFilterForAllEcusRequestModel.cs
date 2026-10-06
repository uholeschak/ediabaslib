using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class DefineSFAFilterForAllEcusRequestModel
    {
        [JsonProperty("inputTalFilter", NullValueHandling = NullValueHandling.Ignore)]
        public TalFilterModel InputTalFilter { get; set; }

        [JsonProperty("ecuOptions", NullValueHandling = NullValueHandling.Ignore)]
        public SfaPerEcuOptionsModel EcuOptions { get; set; }
    }
}
