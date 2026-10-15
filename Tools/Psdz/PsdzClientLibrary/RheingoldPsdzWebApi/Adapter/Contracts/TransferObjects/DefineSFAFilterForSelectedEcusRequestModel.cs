using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class DefineSFAFilterForSelectedEcusRequestModel
    {
        [JsonProperty("ecuOptions", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<int, SfaPerEcuOptionsModel> EcuOptions { get; set; }

        [JsonProperty("inputTalFilter", NullValueHandling = NullValueHandling.Ignore)]
        public TalFilterModel InputTalFilter { get; set; }
    }
}
