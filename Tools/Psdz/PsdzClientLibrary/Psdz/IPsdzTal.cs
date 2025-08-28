using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    public enum PsdzTalExecutionState
    {
        AbortedByError,
        AbortedByUser,
        Executable,
        Finished,
        FinishedForHardwareTransactions,
        FinishedForHardwareTransactionsWithError,
        FinishedForHardwareTransactionsWithWarnings,
        FinishedWithError,
        FinishedWithWarnings,
        Running
    }

    public interface IPsdzTal : IPsdzTalElement
    {
        IEnumerable<IPsdzEcuIdentifier> AffectedEcus { get; }

        string AsXml { get; }

        IEnumerable<IPsdzEcuIdentifier> InstalledEcuListIst { get; }

        IEnumerable<IPsdzEcuIdentifier> InstalledEcuListSoll { get; }

        PsdzTalExecutionState TalExecutionState { get; }

        IEnumerable<IPsdzTalLine> TalLines { get; }

        IPsdzExecutionTime PsdzExecutionTime { get; }
    }
}
