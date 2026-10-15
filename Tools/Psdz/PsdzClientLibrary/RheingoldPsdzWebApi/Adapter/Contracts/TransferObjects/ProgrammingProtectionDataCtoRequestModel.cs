using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class ProgrammingProtectionDataCtoRequestModel
    {
        [JsonProperty("tal", NullValueHandling = NullValueHandling.Ignore)]
        public TalModel Tal;
    }
}