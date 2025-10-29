using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class IPsecEcuBitmaskResultCtoModel
    {
        [JsonProperty("failedEcus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuFailureResponseCtoModel> FailedEcus { get; set; }

        [JsonProperty("successEcus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<KeyValuePairModel<EcuIdentifierModel, byte[]>> SuccessEcus { get; set; }
    }
}