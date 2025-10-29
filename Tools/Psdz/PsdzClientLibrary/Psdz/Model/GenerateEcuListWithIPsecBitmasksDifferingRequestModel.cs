using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class GenerateEcuListWithIPsecBitmasksDifferingRequestModel
    {
        [JsonProperty("ecuBitmasks", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<KeyValuePairModel<EcuIdentifierModel, byte[]>> EcuBitmasks { get; set; }

        [JsonProperty("targetBitmask", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] TargetBitmask { get; set; }
    }
}