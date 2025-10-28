using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class ReadPublicKeyResultCtoModel
    {
        [JsonProperty("kdsFailureResponseCto", NullValueHandling = NullValueHandling.Ignore)]
        public KdsFailureResponseCtoModel KdsFailureResponseCto { get; set; }

        [JsonProperty("publicKeys", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<KdsPublicKeyResultCtoModel> PublicKeys { get; set; }
    }
}