using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class PerformQuickKdsCheckSP25ResultCtoModel
    {
        [JsonProperty("kdsActionStatus", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public KdsActionStatusEto KdsActionStatus { get; set; }

        [JsonProperty("kdsFailureResponse", NullValueHandling = NullValueHandling.Ignore)]
        public KdsFailureResponseCtoModel KdsFailureResponse { get; set; }

        [JsonProperty("kdsId", NullValueHandling = NullValueHandling.Ignore)]
        public KdsIdCtoModel KdsId { get; set; }

        [JsonProperty("kdsQuickCheckResults", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<KdsQuickCheckResultCtoModel> KdsQuickCheckResults { get; set; }
    }
}