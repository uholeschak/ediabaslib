using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class IdRestoreTaModel : TaModel
    {
        [JsonProperty("backupFile", NullValueHandling = NullValueHandling.Ignore)]
        public string BackupFile { get; set; }
    }
}