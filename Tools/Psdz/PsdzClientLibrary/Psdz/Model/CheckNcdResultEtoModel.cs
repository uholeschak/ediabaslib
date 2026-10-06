using Newtonsoft.Json;
using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class CheckNcdResultEtoModel
    {
        [JsonProperty("detailedNcdStatus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<DetailedNcdInfoEtoModel> DetailedNcdStatus { get; set; }

        [JsonProperty("eachNcdSigned", NullValueHandling = NullValueHandling.Ignore)]
        public bool EachNcdSigned { get; set; }
    }
}