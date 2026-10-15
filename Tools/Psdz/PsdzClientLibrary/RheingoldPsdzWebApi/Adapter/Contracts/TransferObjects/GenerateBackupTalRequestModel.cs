using Newtonsoft.Json;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public class GenerateBackupTalRequestModel
    {
        [JsonProperty("backupPath", NullValueHandling = NullValueHandling.Ignore)]
        public string BackupPath { get; set; }

        [JsonProperty("standardTal", NullValueHandling = NullValueHandling.Ignore)]
        public TalModel StandardTal { get; set; }

        [JsonProperty("talFilter", NullValueHandling = NullValueHandling.Ignore)]
        public TalFilterModel TalFilter { get; set; }
    }
}