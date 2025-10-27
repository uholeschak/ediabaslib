using BMW.Rheingold.Psdz;
using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    [JsonConverter(typeof(JsonInheritanceConverter), new object[] { "discriminatorType" })]
    [JsonInheritance("BlFlashTaModel", typeof(BlFlashTaModel))]
    [JsonInheritance("FscDeployTaModel", typeof(FscDeployTaModel))]
    [JsonInheritance("HddUpdateTaModel", typeof(HddUpdateTaModel))]
    [JsonInheritance("IbaDeployTaModel", typeof(IbaDeployTaModel))]
    [JsonInheritance("IdLightTaModel", typeof(IdLightTaModel))]
    [JsonInheritance("IdRestoreTaModel", typeof(IdRestoreTaModel))]
    [JsonInheritance("SFADeleteTAModel", typeof(SFADeleteTAModel))]
    [JsonInheritance("SFAVerifyTAModel", typeof(SFAVerifyTAModel))]
    [JsonInheritance("SFAWriteTAModel", typeof(SFAWriteTAModel))]
    [JsonInheritance("SwDeployTaModel", typeof(SwDeployTaModel))]
    [JsonInheritance("SmacSwDeployOnMasterTaModel", typeof(SmacSwDeployOnMasterTaModel))]
    [JsonInheritance("SmacEcuMirrorDeployOnMasterTaModel", typeof(SmacEcuMirrorDeployOnMasterTaModel))]
    [JsonInheritance("SmacTransferStartTaModel", typeof(SmacTransferStartTaModel))]
    [JsonInheritance("SmacTransferStatusTaModel", typeof(SmacTransferStatusTaModel))]
    [JsonInheritance("EcuMirrorDeployTaModel", typeof(EcuMirrorDeployTaModel))]
    [JsonInheritance("EcuActivateTaModel", typeof(EcuActivateTaModel))]
    [JsonInheritance("EcuPollTaModel", typeof(EcuPollTaModel))]
    public class TaModel
    {
        [JsonProperty("sgbmId", NullValueHandling = NullValueHandling.Ignore)]
        public SgbmIdModel SgbmId { get; set; }

        [JsonProperty("talElement", NullValueHandling = NullValueHandling.Ignore)]
        public TalElementModel TalElement { get; set; }
    }
}