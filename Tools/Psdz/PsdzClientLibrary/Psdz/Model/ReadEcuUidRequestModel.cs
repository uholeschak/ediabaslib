using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class ReadEcuUidRequestModel
    {
        [JsonProperty("ecus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuIdentifierModel> Ecus { get; set; }

        [JsonProperty("svt", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel Svt { get; set; }
    }
}