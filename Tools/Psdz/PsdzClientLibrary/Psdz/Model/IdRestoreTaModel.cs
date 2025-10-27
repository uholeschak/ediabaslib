using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class IdRestoreTaModel : TaModel
    {
        [JsonProperty("backupFile", NullValueHandling = NullValueHandling.Ignore)]
        public string BackupFile { get; set; }
    }
}