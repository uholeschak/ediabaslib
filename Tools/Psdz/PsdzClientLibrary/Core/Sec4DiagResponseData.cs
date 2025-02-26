using Newtonsoft.Json;

namespace PsdzClientLibrary.Core
{
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