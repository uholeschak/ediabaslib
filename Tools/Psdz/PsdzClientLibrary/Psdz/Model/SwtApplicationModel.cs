using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BMW.Rheingold.Psdz
{
    public class SwtApplicationModel
    {
        [JsonProperty("backupPossible", NullValueHandling = NullValueHandling.Ignore)]
        public bool BackupPossible { get; set; }

        [JsonProperty("fsc", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] Fsc { get; set; }

        [JsonProperty("fscCert", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] FscCert { get; set; }

        [JsonProperty("fscCertState", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public FscCertStateModel FscCertState { get; set; }

        [JsonProperty("fscState", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public FscStateModel FscState { get; set; }

        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public int Position { get; set; }

        [JsonProperty("softwareSigState", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public SoftwareSigStateModel? SoftwareSigState { get; set; }

        [JsonProperty("swtActionType", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public SwtActionTypeModel? SwtActionType { get; set; }

        [JsonProperty("swtApplicationId", NullValueHandling = NullValueHandling.Ignore)]
        public SwtApplicationIdModel SwtApplicationId { get; set; }

        [JsonProperty("swtType", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public SwtTypeModel SwtType { get; set; }
    }
}