using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BMW.Rheingold.Psdz
{
    public class TalLineModel
    {
        [JsonProperty("blFlash", NullValueHandling = NullValueHandling.Ignore)]
        public BlFlashModel BlFlash { get; set; }

        [JsonProperty("ecuIdentifier", NullValueHandling = NullValueHandling.Ignore)]
        public EcuIdentifierModel EcuIdentifier { get; set; }

        [JsonProperty("fscBackup", NullValueHandling = NullValueHandling.Ignore)]
        public FscBackupModel FscBackup { get; set; }

        [JsonProperty("fscDeploy", NullValueHandling = NullValueHandling.Ignore)]
        public FscDeployModel FscDeploy { get; set; }

        [JsonProperty("hddUpdate", NullValueHandling = NullValueHandling.Ignore)]
        public HddUpdateModel HddUpdate { get; set; }

        [JsonProperty("ibaDeploy", NullValueHandling = NullValueHandling.Ignore)]
        public IbaDeployModel IbaDeploy { get; set; }

        [JsonProperty("idBackup", NullValueHandling = NullValueHandling.Ignore)]
        public IdBackupModel IdBackup { get; set; }

        [JsonProperty("idRestore", NullValueHandling = NullValueHandling.Ignore)]
        public IdRestoreModel IdRestore { get; set; }

        [JsonProperty("sfaDeploy", NullValueHandling = NullValueHandling.Ignore)]
        public SFADeployModel SfaDeploy { get; set; }

        [JsonProperty("swDeploy", NullValueHandling = NullValueHandling.Ignore)]
        public SwDeployModel SwDeploy { get; set; }

        [JsonProperty("smacTransferStart", NullValueHandling = NullValueHandling.Ignore)]
        public SmacTransferStartModel SmacTransferStart { get; set; }

        [JsonProperty("smacTransferStatus", NullValueHandling = NullValueHandling.Ignore)]
        public SmacTransferStatusModel SmacTransferStatus { get; set; }

        [JsonProperty("ecuMirrorDeploy", NullValueHandling = NullValueHandling.Ignore)]
        public EcuMirrorDeployModel EcuMirrorDeploy { get; set; }

        [JsonProperty("ecuActivate", NullValueHandling = NullValueHandling.Ignore)]
        public EcuActivateModel EcuActivate { get; set; }

        [JsonProperty("ecuPoll", NullValueHandling = NullValueHandling.Ignore)]
        public EcuPollModel EcuPoll { get; set; }

        [JsonProperty("taCategories", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public TACategories TaCategories { get; set; }

        [JsonProperty("taCategory", NullValueHandling = NullValueHandling.Ignore)]
        public TaCategoryModel TaCategory { get; set; }

        [JsonProperty("talElement", NullValueHandling = NullValueHandling.Ignore)]
        public TalElementModel TalElement { get; set; }
    }
}