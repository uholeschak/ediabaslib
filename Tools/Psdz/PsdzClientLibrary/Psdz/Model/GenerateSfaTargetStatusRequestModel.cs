using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class GenerateSfaTargetStatusRequestModel
    {
        [JsonProperty("psdzTokenPack", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SecureTokenEtoModel> PsdzTokenPack { get; set; }

        [JsonProperty("svtCurrent", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel SvtCurrent { get; set; }
    }
}