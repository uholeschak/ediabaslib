using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class IPsecEcuBitmaskResultCtoModel
    {
        [JsonProperty("failedEcus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuFailureResponseCtoModel> FailedEcus { get; set; }

        [JsonProperty("successEcus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<KeyValuePairModel<EcuIdentifierModel, byte[]>> SuccessEcus { get; set; }
    }
}