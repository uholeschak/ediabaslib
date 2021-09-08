using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public interface IPsdzInfo
    {
        string ExpectedPsdzVersion { get; }

        bool IsPsdzInitialized { get; }

        bool IsValidPsdzVersion { get; }

        string PsdzDataPath { get; }

        string PsdzVersion { get; }
    }
}
