using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class RequestBackupDataRequestModel
    {
        [JsonProperty("backupPath", NullValueHandling = NullValueHandling.Ignore)]
        public string BackupPath { get; set; }
    }
}