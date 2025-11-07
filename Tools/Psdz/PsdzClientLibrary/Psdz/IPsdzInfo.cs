using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz
{
    public interface IPsdzInfo
    {
        // [UH] For backward compatibility
        string ExpectedPsdzVersion { get; }

        bool IsPsdzInitialized { get; }

        bool IsValidPsdzVersion { get; }

        string PsdzDataPath { get; }

        string PsdzVersion { get; }
    }
}
