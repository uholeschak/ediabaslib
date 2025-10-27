using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class TalExecutionConfigModel
    {
        [JsonProperty("codingModeSwitch", NullValueHandling = NullValueHandling.Ignore)]
        public bool CodingModeSwitch { get; set; }

        [JsonProperty("differentialMode", NullValueHandling = NullValueHandling.Ignore)]
        public bool DifferentialMode { get; set; }

        [JsonProperty("ecusNotToSwitchToProgrammingMode", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<DiagAddressModel> EcusNotToSwitchToProgrammingMode { get; set; }

        [JsonProperty("ecusToPreventUDSFallback", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<DiagAddressModel> EcusToPreventUDSFallback { get; set; }

        [JsonProperty("hddUpdateURL", NullValueHandling = NullValueHandling.Ignore)]
        public string HddUpdateURL { get; set; }

        [JsonProperty("parallel", NullValueHandling = NullValueHandling.Ignore)]
        public bool Parallel { get; set; }

        [JsonProperty("programmingModeSwitch", NullValueHandling = NullValueHandling.Ignore)]
        public bool ProgrammingModeSwitch { get; set; }

        [JsonProperty("secureCodingConfig", NullValueHandling = NullValueHandling.Ignore)]
        public SecureCodingConfigCtoModel SecureCodingConfig { get; set; }

        [JsonProperty("programmingProtectionData", NullValueHandling = NullValueHandling.Ignore)]
        public ProgrammingProtectionDataCtoModel ProgrammingProtectionData { get; set; }

        [JsonProperty("taMaxRepeat", NullValueHandling = NullValueHandling.Ignore)]
        public int TaMaxRepeat { get; set; }

        [JsonProperty("useAep", NullValueHandling = NullValueHandling.Ignore)]
        public bool UseAep { get; set; }

        [JsonProperty("useFlaMode", NullValueHandling = NullValueHandling.Ignore)]
        public bool UseFlaMode { get; set; }

        [JsonProperty("useProgrammingCounter", NullValueHandling = NullValueHandling.Ignore)]
        public bool UseProgrammingCounter { get; set; }

        [JsonProperty("programmingTokens", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<ProgrammingTokenCtoModel> ProgrammingTokens { get; set; }

        [JsonProperty("ignoreSignatureForProgrammingToken", NullValueHandling = NullValueHandling.Ignore)]
        public bool IgnoreSignatureForProgrammingToken { get; set; }

        [JsonProperty("expectedSgbmidValidationActive", NullValueHandling = NullValueHandling.Ignore)]
        public bool ExpectedSgbmidValidationActive { get; set; }

        [JsonProperty("expectedSgbmIdValidationForSmacTransferStartActive", NullValueHandling = NullValueHandling.Ignore)]
        public bool ExpectedSgbmIdValidationForSmacTransferStartActive { get; set; }
    }
}