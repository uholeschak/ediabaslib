using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class SetLcsRequestModel
    {
        [JsonProperty("ecuLcsValues", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuLcsValueCtoModel> EcuLcsValues { get; set; }

        [JsonProperty("svt", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel Svt { get; set; }
    }
}