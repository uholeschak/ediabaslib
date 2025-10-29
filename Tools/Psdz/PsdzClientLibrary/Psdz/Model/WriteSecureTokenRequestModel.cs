using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class WriteSecureTokenRequestModel
    {
        [JsonProperty("secureTokens", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SecureTokenEtoModel> SecureTokens { get; set; }

        [JsonProperty("svt", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel Svt { get; set; }
    }
}