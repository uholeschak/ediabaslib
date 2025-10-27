using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal static class TalExecutionConfigMapper
    {
        public static TalExecutionConfigModel Map(TalExecutionSettings talExecutionSettings)
        {
            return new TalExecutionConfigModel
            {
                CodingModeSwitch = talExecutionSettings.CodingModeSwitch,
                DifferentialMode = talExecutionSettings.DifferentialMode,
                HddUpdateURL = talExecutionSettings.HddUpdateURL,
                Parallel = talExecutionSettings.Parallel,
                ProgrammingModeSwitch = talExecutionSettings.ProgrammingModeSwitch,
                TaMaxRepeat = talExecutionSettings.TaMaxRepeat,
                UseAep = talExecutionSettings.UseAep,
                UseFlaMode = talExecutionSettings.UseFlaMode,
                UseProgrammingCounter = talExecutionSettings.UseProgrammingCounter,
                SecureCodingConfig = SecureCodingConfigCtoMapper.Map(talExecutionSettings.SecureCodingConfig),
                EcusNotToSwitchToProgrammingMode = talExecutionSettings.EcusNotToSwitchProgrammingMode?.Select(DiagAddressMapper.Map).ToList(),
                EcusToPreventUDSFallback = talExecutionSettings.EcusToPreventUdsFallback?.Select(DiagAddressMapper.Map).ToList(),
                ProgrammingProtectionData = ProgrammingProtectionDataCtoMapper.Map(talExecutionSettings.ProgrammingProtectionDataCto),
                ProgrammingTokens = talExecutionSettings.ProgrammingTokens?.Select(ProgrammingTokenCtoMapper.Map).ToList(),
                IgnoreSignatureForProgrammingToken = talExecutionSettings.IgnoreSignatureForProgrammingToken,
                ExpectedSgbmidValidationActive = talExecutionSettings.ExpectedSgbmidValidationActive,
                ExpectedSgbmIdValidationForSmacTransferStartActive = talExecutionSettings.ExpectedSgbmIdValidationForSmacTransferStartActive
            };
        }
    }
}