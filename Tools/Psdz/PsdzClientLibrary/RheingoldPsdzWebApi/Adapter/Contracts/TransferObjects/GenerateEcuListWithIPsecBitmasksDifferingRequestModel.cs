using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class GenerateEcuListWithIPsecBitmasksDifferingRequestModel
    {
        [JsonProperty("ecuBitmasks", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<KeyValuePairModel<EcuIdentifierModel, byte[]>> EcuBitmasks { get; set; }

        [JsonProperty("targetBitmask", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] TargetBitmask { get; set; }
    }
}