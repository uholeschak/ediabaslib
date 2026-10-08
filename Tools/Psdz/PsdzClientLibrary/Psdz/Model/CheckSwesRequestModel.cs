using Newtonsoft.Json;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class CheckSwesRequestModel
    {
        [JsonProperty("sgbmIdList", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<SgbmIdModel> SgbmIdList { get; set; }
    }
}