using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace EdiabasLib
{
    public class EdSec4Diag
    {
        public class CertReqProfile
        {
            [DataContract]
            public enum EnumType
            {
                [EnumMember]
                crp_subCA_4ISTA,
                [EnumMember]
                crp_subCA_4ISTA_TISonly,
                [EnumMember]
                crp_M2M_3dParty_4_CUST_ControlOnly
            }
        }

        public class ProofOfPossession
        {
            [JsonProperty("signatureType")]
            public string SignatureType { get; set; }

            [JsonProperty("signature")]
            public string Signature { get; set; }
        }

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

        public class Sec4DiagResponseData
        {
            [JsonProperty("vin17")]
            public string Vin17 { get; set; }

            [JsonProperty("certificate")]
            public string Certificate { get; set; }

            [JsonProperty("certificateChain")]
            public string[] CertificateChain { get; set; }
        }
    }
}