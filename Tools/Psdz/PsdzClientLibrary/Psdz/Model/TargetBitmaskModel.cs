using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class TargetBitmaskModel
    {
        [JsonProperty("failedEcus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuFailureResponseCtoModel> FailedEcus { get; set; }

        [JsonProperty("targetBitmask", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] TargetBitmask { get; set; }
    }
}