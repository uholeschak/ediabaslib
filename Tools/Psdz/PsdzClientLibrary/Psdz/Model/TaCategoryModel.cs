using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    [JsonConverter(typeof(JsonInheritanceConverter), new object[] { "discriminatorType" })]
    [JsonInheritance("BlFlashModel", typeof(BlFlashModel))]
    [JsonInheritance("FscBackupModel", typeof(FscBackupModel))]
    [JsonInheritance("FscDeployModel", typeof(FscDeployModel))]
    [JsonInheritance("HddUpdateModel", typeof(HddUpdateModel))]
    [JsonInheritance("IbaDeployModel", typeof(IbaDeployModel))]
    [JsonInheritance("IdBackupModel", typeof(IdBackupModel))]
    [JsonInheritance("IdRestoreModel", typeof(IdRestoreModel))]
    [JsonInheritance("SFADeployModel", typeof(SFADeployModel))]
    [JsonInheritance("SwDeployModel", typeof(SwDeployModel))]
    [JsonInheritance("CdDeployModel", typeof(CdDeployModel))]
    [JsonInheritance("GatewayTableDeployModel", typeof(GatewayTableDeployModel))]
    [JsonInheritance("HwDeinstallModel", typeof(HwDeinstallModel))]
    [JsonInheritance("HwInstallModel", typeof(HwInstallModel))]
    [JsonInheritance("PreviousRunModel", typeof(PreviousRunModel))]
    [JsonInheritance("SwDeleteModel", typeof(SwDeleteModel))]
    [JsonInheritance("SmacTransferStartModel", typeof(SmacTransferStartModel))]
    [JsonInheritance("SmacTransferStatusModel", typeof(SmacTransferStatusModel))]
    [JsonInheritance("EcuMirrorDeployModel", typeof(EcuMirrorDeployModel))]
    [JsonInheritance("EcuActivateModel", typeof(EcuActivateModel))]
    [JsonInheritance("EcuPollModel", typeof(EcuPollModel))]
    public class TaCategoryModel
    {
        [JsonProperty("executionStatus", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public TaExecutionStateModel ExecutionStatus { get; set; }

        [JsonProperty("isEmpty", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsEmpty { get; set; }

        [JsonProperty("tas", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<TaModel> Tas { get; set; }
    }
}