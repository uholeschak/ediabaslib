using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using RheingoldPsdzWebApi.Adapter.Contracts;

namespace RheingoldPsdzWebApi.Adapter.Contracts
{
    public class SvtModel : StandardSvtModel
    {
        [JsonProperty("isValid", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsValid { get; set; }

        [JsonProperty("vin", NullValueHandling = NullValueHandling.Ignore)]
        public string Vin { get; set; }
    }
}