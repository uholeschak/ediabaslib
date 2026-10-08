using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class GenerateSfaTargetStatusRequestModel
    {
        [JsonProperty("psdzTokenPack", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SecureTokenEtoModel> PsdzTokenPack { get; set; }

        [JsonProperty("svtCurrent", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel SvtCurrent { get; set; }
    }
}