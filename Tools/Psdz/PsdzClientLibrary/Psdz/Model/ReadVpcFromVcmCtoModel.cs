using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
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