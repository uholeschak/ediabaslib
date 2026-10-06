using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class ProgrammingProtectionDataCtoRequestModel
    {
        [JsonProperty("tal", NullValueHandling = NullValueHandling.Ignore)]
        public TalModel Tal;
    }
}