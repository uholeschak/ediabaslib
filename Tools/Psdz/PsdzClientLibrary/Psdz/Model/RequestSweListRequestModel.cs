using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class RequestSweListRequestModel
    {
        [JsonProperty("ignoreSwDelete", NullValueHandling = NullValueHandling.Ignore)]
        public bool IgnoreSwDelete { get; set; }

        [JsonProperty("tal", NullValueHandling = NullValueHandling.Ignore)]
        public TalModel Tal { get; set; }
    }
}