using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class WriteSecurityMemoryObjectsRequestModel
    {
        [JsonProperty("certificates", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SecurityMemoryObjectEtoModel> Certificates { get; set; }

        [JsonProperty("svtIst", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel SvtIst { get; set; }
    }
}