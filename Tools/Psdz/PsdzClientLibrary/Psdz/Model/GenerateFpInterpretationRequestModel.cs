using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class GenerateFpInterpretationRequestModel
    {
        [JsonProperty("baureihe", NullValueHandling = NullValueHandling.Ignore)]
        public string Baureihe { get; set; }

        [JsonProperty("fp", NullValueHandling = NullValueHandling.Ignore)]
        public FpModel Fp { get; set; }
    }
}