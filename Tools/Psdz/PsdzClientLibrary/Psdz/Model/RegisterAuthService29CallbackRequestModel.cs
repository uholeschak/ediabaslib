using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class RegisterAuthService29CallbackRequestModel
    {
        [JsonProperty("s29CertificateChainByteArray", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] S29CertificateChainByteArray { get; set; }

        [JsonProperty("serializedPrivateKey", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] SerializedPrivateKey { get; set; }
    }
}