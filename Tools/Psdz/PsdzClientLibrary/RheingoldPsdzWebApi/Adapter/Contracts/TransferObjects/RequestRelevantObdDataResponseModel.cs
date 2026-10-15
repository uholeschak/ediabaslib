using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class RequestRelevantObdDataResponseModel
    {
        [JsonProperty("ecuToObdMap", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<KeyValuePairModel<EcuIdentifierModel, ObdDataModel>> EcuToObdMap { get; set; }
    }
}