using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class RequestHDDUpdateTalRequestModel
    {
        [JsonProperty("backupPath", NullValueHandling = NullValueHandling.Ignore)]
        public string BackupPath { get; set; }

        [JsonProperty("svtActual", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel SvtActual { get; set; }

        [JsonProperty("svtTarget", NullValueHandling = NullValueHandling.Ignore)]
        public SvtModel SvtTarget { get; set; }

        [JsonProperty("swtAction", NullValueHandling = NullValueHandling.Ignore)]
        public SwtActionModel SwtAction { get; set; }
    }
}