using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

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