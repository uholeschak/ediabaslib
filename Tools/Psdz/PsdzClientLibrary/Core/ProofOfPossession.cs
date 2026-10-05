using Newtonsoft.Json;

namespace BMW.Rheingold.CoreFramework.Sec4Diag
{
    public class ProofOfPossession
    {
        [JsonProperty("signatureType")]
        public string SignatureType { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }
    }
}