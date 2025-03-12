using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Crmf;

namespace PsdzClient.Core
{
    public class Sec4DiagRequestData
    {
        [JsonProperty("vin17")]
        public string Vin17 { get; set; }

        [JsonProperty("certReqProfile")]
        public string CertReqProfile { get; set; }

        [JsonProperty("publicKey")]
        public string PublicKey { get; set; }

        [JsonProperty("proofOfPossession")]
        public ProofOfPossession ProofOfPossession { get; set; }
    }
}