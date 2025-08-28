using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.SecureCoding;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public struct TalExecutionSettings
    {
        public bool CodingModeSwitch;

        public bool DifferentialMode;

        public string HddUpdateURL;

        public bool Parallel;

        public bool ProgrammingModeSwitch;

        public int TaMaxRepeat;

        public bool UseAep;

        public bool UseFlaMode;

        public bool UseProgrammingCounter;

        public IPsdzSecureCodingConfigCto SecureCodingConfig;

        public IEnumerable<IPsdzDiagAddress> EcusNotToSwitchProgrammingMode;

        public IEnumerable<IPsdzDiagAddress> EcusToPreventUdsFallback;

        public IPsdzProgrammingProtectionDataCto ProgrammingProtectionDataCto;

        public IEnumerable<IPsdzProgrammingTokenCto> ProgrammingTokens;

        public bool IgnoreSignatureForProgrammingToken;

        public bool ExpectedSgbmidValidationActive;

        public bool ExpectedSgbmIdValidationForSmacTransferStartActive;
    }
}
