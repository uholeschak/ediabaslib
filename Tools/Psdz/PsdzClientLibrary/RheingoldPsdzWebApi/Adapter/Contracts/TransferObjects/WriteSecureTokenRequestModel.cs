using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class WriteSecureTokenRequestModel
    {
        [JsonProperty("secureTokens", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SecureTokenEtoModel> SecureTokens { get; set; }

        [JsonProperty("svt", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel Svt { get; set; }
    }
}