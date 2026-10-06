using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using RheingoldPsdzWebApi.Adapter.Contracts;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class RequestRelevantObdDataRequestModel
    {
        [JsonProperty("svtActual", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel SvtActual { get; set; }
    }
}