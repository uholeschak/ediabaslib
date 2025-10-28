using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class RequestRelevantObdDataResponseModel
    {
        [JsonProperty("ecuToObdMap", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<KeyValuePairModel<EcuIdentifierModel, ObdDataModel>> EcuToObdMap { get; set; }
    }
}