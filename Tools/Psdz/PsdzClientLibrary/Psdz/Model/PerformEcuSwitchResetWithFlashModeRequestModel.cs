using Newtonsoft.Json;
using System.Collections.Generic;
using BMW.Rheingold.Psdz;
using RheingoldPsdzWebApi.Adapter.Contracts;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
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