using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class PerformEcuSwitchResetWithFlashModeRequestModel
    {
        [JsonProperty("svtIst", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel Svt { get; set; }

        [JsonProperty("ecusToBeReset", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<EcuResetMapping> EcusToBeReset { get; set; }

        [JsonProperty("performWithFlashMode", NullValueHandling = NullValueHandling.Ignore)]
        public bool PerformWithFlashMode { get; set; }
    }
}