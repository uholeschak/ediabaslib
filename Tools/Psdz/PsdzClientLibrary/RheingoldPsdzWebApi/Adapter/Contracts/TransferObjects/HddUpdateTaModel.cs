using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class HddUpdateTaModel : TaModel
    {
        [JsonProperty("secondsToCompletion", NullValueHandling = NullValueHandling.Ignore)]
        public long SecondsToCompletion { get; set; }
    }
}