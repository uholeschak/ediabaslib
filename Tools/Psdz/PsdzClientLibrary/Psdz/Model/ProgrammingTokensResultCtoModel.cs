using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class ProgrammingTokensResultCtoModel
    {
        [JsonProperty("failures", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuFailureResponseCtoModel> Failures { get; set; }

        [JsonProperty("tokens", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<ProgrammingTokenCtoModel> Tokens { get; set; }
    }
}