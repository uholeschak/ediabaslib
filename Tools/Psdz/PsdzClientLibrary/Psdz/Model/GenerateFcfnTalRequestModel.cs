using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class GenerateFcfnTalRequestModel
    {
        [JsonProperty("svt", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel Svt { get; set; }

        [JsonProperty("swtApplications", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SwtApplicationModel> SwtApplications { get; set; }

        [JsonProperty("talFilter", NullValueHandling = NullValueHandling.Ignore)]
        public TalFilterModel TalFilter { get; set; }
    }
}