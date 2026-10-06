using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class ResetEcusRequestModel
    {
        [JsonProperty("svtIst", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel Svt { get; set; }

        [JsonProperty("ecusToBeReset", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuIdentifierCtoModel> Ecus { get; set; }
    }
}