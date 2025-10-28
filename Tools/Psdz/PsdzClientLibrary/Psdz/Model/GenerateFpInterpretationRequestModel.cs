using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class GenerateFpInterpretationRequestModel
    {
        [JsonProperty("baureihe", NullValueHandling = NullValueHandling.Ignore)]
        public string Baureihe { get; set; }

        [JsonProperty("fp", NullValueHandling = NullValueHandling.Ignore)]
        public FpModel Fp { get; set; }
    }
}