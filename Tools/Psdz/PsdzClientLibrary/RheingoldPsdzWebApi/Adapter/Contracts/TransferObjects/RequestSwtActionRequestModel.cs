using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class RequestSwtActionRequestModel
    {
        [JsonProperty("periodicalCheck", NullValueHandling = NullValueHandling.Ignore)]
        public bool PeriodicalCheck { get; set; }
    }
}