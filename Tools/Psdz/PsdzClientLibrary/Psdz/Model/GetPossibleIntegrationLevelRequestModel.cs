using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using RheingoldPsdzWebApi.Adapter.Contracts;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class GetPossibleIntegrationLevelRequestModel
    {
        [JsonProperty("fa", NullValueHandling = NullValueHandling.Ignore)]
        public FaModel Fa { get; set; }
    }
}