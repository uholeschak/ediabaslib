using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using RheingoldPsdzWebApi.Adapter.Contracts;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
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