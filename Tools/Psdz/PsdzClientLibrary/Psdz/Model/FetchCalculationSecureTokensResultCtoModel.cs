using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class FetchCalculationSecureTokensResultCtoModel
    {
        [JsonProperty("durationOfLastRequest", NullValueHandling = NullValueHandling.Ignore)]
        public int DurationOfLastRequest { get; set; }

        [JsonProperty("failures", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SecurityBackendRequestFailureCtoModel> Failures { get; set; }

        [JsonProperty("featureSetReference", NullValueHandling = NullValueHandling.Ignore)]
        public string FeatureSetReference { get; set; }

        [JsonProperty("jsonString", NullValueHandling = NullValueHandling.Ignore)]
        public string JsonString { get; set; }

        [JsonProperty("progressStatus", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public SecurityBackendRequestProgressStatusTo ProgressStatus { get; set; }

        [JsonProperty("secureTokens", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SecureTokenEtoModel> SecureTokens { get; set; }

        [JsonProperty("secureTokenForVehicle", NullValueHandling = NullValueHandling.Ignore)]
        public SecureTokenForVehicleEtoModel SecureTokenForVehicle { get; set; }

        [JsonProperty("tokenDetailedStatusEtos", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<DetailedStatusCtoModel> TokenDetailedStatusEtos { get; set; }

        [JsonProperty("tokenOverallStatusEto", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public TokenOverallStatusEtoModel TokenOverallStatusEto { get; set; }

        [JsonProperty("tokenPackageReference", NullValueHandling = NullValueHandling.Ignore)]
        public string TokenPackageReference { get; set; }
    }
}