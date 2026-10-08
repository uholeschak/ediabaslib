using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class ProgrammingTokensResultCtoModel
    {
        [JsonProperty("failures", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuFailureResponseCtoModel> Failures { get; set; }

        [JsonProperty("tokens", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<ProgrammingTokenCtoModel> Tokens { get; set; }
    }
}