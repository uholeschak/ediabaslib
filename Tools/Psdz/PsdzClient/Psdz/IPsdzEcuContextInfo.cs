using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public interface IPsdzEcuContextInfo
    {
        IPsdzEcuIdentifier EcuId { get; }

        DateTime? LastProgrammingDate { get; }

        DateTime ManufacturingDate { get; }

        int PerformedFlashCycles { get; }

        int ProgramCounter { get; }

        int RemainingFlashCycles { get; }
    }
}
