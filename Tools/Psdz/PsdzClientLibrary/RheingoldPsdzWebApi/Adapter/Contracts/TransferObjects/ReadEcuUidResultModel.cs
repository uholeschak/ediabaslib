using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class ReadEcuUidResultModel
    {
        [JsonProperty("ecuUids", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<KeyValuePairModel<EcuIdentifierModel, EcuUidCtoModel>> EcuUids { get; set; }

        [JsonProperty("failureResponse", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuFailureResponseCtoModel> FailureResponse { get; set; }
    }
}