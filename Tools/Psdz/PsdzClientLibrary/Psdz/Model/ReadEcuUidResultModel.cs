using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class ReadEcuUidResultModel
    {
        [JsonProperty("ecuUids", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<KeyValuePairModel<EcuIdentifierModel, EcuUidCtoModel>> EcuUids { get; set; }

        [JsonProperty("failureResponse", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuFailureResponseCtoModel> FailureResponse { get; set; }
    }
}