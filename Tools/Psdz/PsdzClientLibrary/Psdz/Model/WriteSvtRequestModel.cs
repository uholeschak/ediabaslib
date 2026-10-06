using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using RheingoldPsdzWebApi.Adapter.Contracts;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class WriteSvtRequestModel
    {
        [JsonProperty("standardSvt", NullValueHandling = NullValueHandling.Ignore)]
        public StandardSvtModel StandardSvt { get; set; }
    }
}