using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class WriteIPsecBitmasksRequestModel
    {
        [JsonProperty("ecus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuIdentifierModel> Ecus { get; set; }

        [JsonProperty("svt", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel Svt { get; set; }

        [JsonProperty("targetBm", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] TargetBm { get; set; }
    }
}