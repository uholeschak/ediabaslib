using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class WriteFaRequestModel
    {
        [JsonProperty("fa", NullValueHandling = NullValueHandling.Ignore)]
        public StandardFaModel Fa { get; set; }
    }
}