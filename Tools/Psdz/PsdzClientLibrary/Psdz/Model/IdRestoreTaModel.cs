using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class IdRestoreTaModel : TaModel
    {
        [JsonProperty("backupFile", NullValueHandling = NullValueHandling.Ignore)]
        public string BackupFile { get; set; }
    }
}