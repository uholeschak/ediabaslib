using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz.Model
{
    public class ResetEcusFlashModeRequestModel
    {
        [JsonProperty("svtIst", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel Svt { get; set; }

        [JsonProperty("ecusToBeReset", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuIdentifierCtoModel> Ecus { get; set; }

        [JsonProperty("performWithFlashMode", NullValueHandling = NullValueHandling.Ignore)]
        public bool PerformWithFlashMode { get; set; }
    }
}