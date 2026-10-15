using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class FpModel : StandardFpModel
    {
        [JsonProperty("baureihenverbund", NullValueHandling = NullValueHandling.Ignore)]
        public string Baureihenverbund { get; set; }

        [JsonProperty("entwicklungsbaureihe", NullValueHandling = NullValueHandling.Ignore)]
        public string Entwicklungsbaureihe { get; set; }
    }
}