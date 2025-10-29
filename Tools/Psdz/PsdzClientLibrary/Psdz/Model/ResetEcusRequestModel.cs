using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class ResetEcusRequestModel
    {
        [JsonProperty("svtIst", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel Svt { get; set; }

        [JsonProperty("ecusToBeReset", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuIdentifierCtoModel> Ecus { get; set; }
    }
}