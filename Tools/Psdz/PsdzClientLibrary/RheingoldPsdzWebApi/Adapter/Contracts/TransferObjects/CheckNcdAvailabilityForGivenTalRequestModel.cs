using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class CheckNcdAvailabilityForGivenTalRequestModel
    {
        [JsonProperty("ncdDirectory", NullValueHandling = NullValueHandling.Ignore)]
        public string NcdDirectory { get; set; }

        [JsonProperty("tal", NullValueHandling = NullValueHandling.Ignore)]
        public TalModel Tal { get; set; }

        [JsonProperty("vin", NullValueHandling = NullValueHandling.Ignore)]
        public VinModel Vin { get; set; }
    }
}