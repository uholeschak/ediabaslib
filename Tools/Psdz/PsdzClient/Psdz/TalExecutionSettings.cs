using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
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
    }
}
