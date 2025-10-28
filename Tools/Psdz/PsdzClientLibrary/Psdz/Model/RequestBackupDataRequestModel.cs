using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class RequestBackupDataRequestModel
    {
        [JsonProperty("backupPath", NullValueHandling = NullValueHandling.Ignore)]
        public string BackupPath { get; set; }
    }
}