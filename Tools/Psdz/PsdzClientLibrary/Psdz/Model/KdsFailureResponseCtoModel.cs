using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class KdsFailureResponseCtoModel
    {
        [JsonProperty("cause", NullValueHandling = NullValueHandling.Ignore)]
        public LocalizableMessageToModel Cause { get; set; }

        [JsonProperty("kdsIdCto", NullValueHandling = NullValueHandling.Ignore)]
        public KdsIdCtoModel KdsIdCto { get; set; }
    }
}