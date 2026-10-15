using Newtonsoft.Json;

namespace BMW.Rheingold.InfoProvider.Sec4Diag.Models
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