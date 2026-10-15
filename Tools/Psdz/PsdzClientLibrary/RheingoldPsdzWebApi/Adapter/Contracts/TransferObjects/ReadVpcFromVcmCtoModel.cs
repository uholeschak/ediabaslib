using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class ReadVpcFromVcmCtoModel
    {
        [JsonProperty("successful", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsSuccessful { get; set; }

        [JsonProperty("vpcCrc", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] VpcCrc { get; set; }

        [JsonProperty("vpcVersion", NullValueHandling = NullValueHandling.Ignore)]
        public long VpcVersion { get; set; }

        [JsonProperty("failedEcus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuFailureResponseCtoModel> FailedEcus { get; set; }
    }
}