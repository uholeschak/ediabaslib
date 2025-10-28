using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class RegisterAuthService29CallbackRequestModel
    {
        [JsonProperty("s29CertificateChainByteArray", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] S29CertificateChainByteArray { get; set; }

        [JsonProperty("serializedPrivateKey", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] SerializedPrivateKey { get; set; }
    }
}