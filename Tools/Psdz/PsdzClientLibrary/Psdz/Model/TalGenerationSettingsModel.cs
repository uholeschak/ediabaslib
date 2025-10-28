using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class TalGenerationSettingsModel
    {
        [JsonProperty("ecusToSuppress", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<DiagAddressModel> EcuToSuppress { get; set; }

        [JsonProperty("allAllowedIntelligentSensors", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<DiagAddressModel> AllAllowedIntelligentSensors { get; set; }

        [JsonProperty("fa", NullValueHandling = NullValueHandling.Ignore)]
        public FaModel Fa { get; set; }

        [JsonProperty("vehicleVPC", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] VehicleVPC { get; set; }

        [JsonProperty("checkProgrammingDeps", NullValueHandling = NullValueHandling.Ignore)]
        public bool CheckProgrammingDeps { get; set; }

        [JsonProperty("filterIntelligentSensors", NullValueHandling = NullValueHandling.Ignore)]
        public bool FilterIntelligentSensors { get; set; }

        [JsonProperty("preventInconsistentSwFlash", NullValueHandling = NullValueHandling.Ignore)]
        public bool PreventInconsistentSwFlash { get; set; }

        [JsonProperty("useMirrorProtocol", NullValueHandling = NullValueHandling.Ignore)]
        public bool UseMirrorProtocol { get; set; }
    }
}