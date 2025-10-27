using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class SecureCodingConfigCtoModel
    {
        [JsonProperty("authenticationTypeEto", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public AuthenticationTypeEto AuthenticationTypeEto { get; set; }

        [JsonProperty("backendNcdCalculationEto", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public BackendNcdCalculationEto BackendNcdCalculationEto { get; set; }

        [JsonProperty("backendSignatureEto", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public BackendSignatureEto BackendSignatureEto { get; set; }

        [JsonProperty("connectionTimeout", NullValueHandling = NullValueHandling.Ignore)]
        public int ConnectionTimeout { get; set; }

        [JsonProperty("crls", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<string> Crls { get; set; }

        [JsonProperty("ncdRecalculationEto", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public NcdRecalculationEto NcdRecalculationEto { get; set; }

        [JsonProperty("ncdRootDirectory", NullValueHandling = NullValueHandling.Ignore)]
        public string NcdRootDirectory { get; set; }

        [JsonProperty("retries", NullValueHandling = NullValueHandling.Ignore)]
        public int Retries { get; set; }

        [JsonProperty("scbPollingTimeout", NullValueHandling = NullValueHandling.Ignore)]
        public int ScbPollingTimeout { get; set; }

        [JsonProperty("scbUrls", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<string> ScbUrls { get; set; }

        [JsonProperty("swlSecBackendUrls", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<string> SwlSecBackendUrls { get; set; }
    }
}