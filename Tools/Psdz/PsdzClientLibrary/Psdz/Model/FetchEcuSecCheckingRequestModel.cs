using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class FetchEcuSecCheckingRequestModel
    {
        [JsonProperty("ecus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuIdentifierModel> Ecus { get; set; }

        [JsonProperty("retries", NullValueHandling = NullValueHandling.Ignore)]
        public int Retries { get; set; }

        [JsonProperty("svtIst", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel SvtIst { get; set; }
    }
}