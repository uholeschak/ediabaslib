using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class FetchCertificatesBindingsAndKeypacksCalculationResultModel
    {
        [JsonProperty("calculatedBindings", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SecurityCalculatedObjectCto> CalculatedBindings { get; set; }

        [JsonProperty("calculatedCertificates", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SecurityCalculatedObjectCto> CalculatedCertificates { get; set; }

        [JsonProperty("calculatedKeypacks", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SecurityCalculatedObjectCto> CalculatedKeypacks { get; set; }

        [JsonProperty("durationOfLastRequest", NullValueHandling = NullValueHandling.Ignore)]
        public int DurationOfLastRequest { get; set; }

        [JsonProperty("failures", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SecurityBackendRequestFailureCtoModel> Failures { get; set; }

        [JsonProperty("progressStatus", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public SecurityBackendRequestProgressStatusTo ProgressStatus { get; set; }
    }
}