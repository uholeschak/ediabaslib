using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class GenerateSollverbauungGesamtFlashRequestModel
    {
        [JsonProperty("faTarget", NullValueHandling = NullValueHandling.Ignore)]
        public FaModel FaTarget { get; set; }

        [JsonProperty("faultTolerant", NullValueHandling = NullValueHandling.Ignore)]
        public bool FaultTolerant { get; set; }

        [JsonProperty("iLevelShipment", NullValueHandling = NullValueHandling.Ignore)]
        public ILevelModel ILevelShipment { get; set; }

        [JsonProperty("iLevelTarget", NullValueHandling = NullValueHandling.Ignore)]
        public ILevelModel ILevelTarget { get; set; }

        [JsonProperty("svtActual", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel SvtActual { get; set; }

        [JsonProperty("talFilter", NullValueHandling = NullValueHandling.Ignore)]
        public TalFilterModel TalFilter { get; set; }
    }
}