using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class CheckSwesRequestModel
    {
        [JsonProperty("sgbmIdList", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SgbmIdModel> SgbmIdList { get; set; }
    }
}