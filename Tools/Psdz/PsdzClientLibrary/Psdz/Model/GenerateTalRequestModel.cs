using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class GenerateTalRequestModel
    {
        [JsonProperty("backupDataPath", NullValueHandling = NullValueHandling.Ignore)]
        public string BackupDataPath { get; set; }

        [JsonProperty("sollverbauung", NullValueHandling = NullValueHandling.Ignore)]
        public SollverbauungModel Sollverbauung { get; set; }

        [JsonProperty("svtActual", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel SvtActual { get; set; }

        [JsonProperty("swtAction", NullValueHandling = NullValueHandling.Ignore)]
        public SwtActionModel SwtAction { get; set; }

        [JsonProperty("talFilter", NullValueHandling = NullValueHandling.Ignore)]
        public TalFilterModel TalFilter { get; set; }

        [JsonProperty("vinFromFa", NullValueHandling = NullValueHandling.Ignore)]
        public string VinFromFa { get; set; }

        [JsonProperty("talGenerationSettings", NullValueHandling = NullValueHandling.Ignore)]
        public TalGenerationSettingsModel TalGenerationSettings { get; set; }
    }
}