using Newtonsoft.Json;

namespace PsdzClient.Core
{
    public class ProofOfPossession
    {
        [JsonProperty("signatureType")]
        public string SignatureType { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }
    }
}