using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class WriteSecureTokenToOBDFirewallRequestModel
    {
        [JsonProperty("secureToken", NullValueHandling = NullValueHandling.Ignore)]
        public SecureTokenEtoModel SecureToken { get; set; }
    }
}
