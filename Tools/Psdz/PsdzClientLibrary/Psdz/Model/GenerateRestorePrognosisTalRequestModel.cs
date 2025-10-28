using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class GenerateRestorePrognosisTalRequestModel
    {
        [JsonProperty("backupPath", NullValueHandling = NullValueHandling.Ignore)]
        public string BackupPath { get; set; }

        [JsonProperty("backupTal", NullValueHandling = NullValueHandling.Ignore)]
        public TalModel BackupTal { get; set; }

        [JsonProperty("standardTal", NullValueHandling = NullValueHandling.Ignore)]
        public TalModel StandardTal { get; set; }

        [JsonProperty("talFilter", NullValueHandling = NullValueHandling.Ignore)]
        public TalFilterModel TalFilter { get; set; }
    }
}