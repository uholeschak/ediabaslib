using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class TargetBitmaskModel
    {
        [JsonProperty("failedEcus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuFailureResponseCtoModel> FailedEcus { get; set; }

        [JsonProperty("targetBitmask", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] TargetBitmask { get; set; }
    }
}