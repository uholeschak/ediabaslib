using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class HddUpdateTaModel : TaModel
    {
        [JsonProperty("secondsToCompletion", NullValueHandling = NullValueHandling.Ignore)]
        public long SecondsToCompletion { get; set; }
    }
}