using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
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
    }
}
