using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using RheingoldPsdzWebApi.Adapter.Contracts;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class TslUpdateRequestModel
    {
        [JsonProperty("complete", NullValueHandling = NullValueHandling.Ignore)]
        public bool Complete { get; set; }

        [JsonProperty("svtActual", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel SvtActual { get; set; }

        [JsonProperty("svtTarget", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel SvtTarget { get; set; }
    }
}