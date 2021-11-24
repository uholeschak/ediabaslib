using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Tal.TalStatus;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    public enum PsdzTaExecutionState
    {
        Executable,
        Inactive,
        NotExecutable,
        AbortedByError,
        AbortedByUser,
        Finished,
        FinishedWithError,
        FinishedWithWarnings,
        Running,
        Repeat,
        NotRequired
    }

    public interface IPsdzTalElement
    {
        Guid Id { get; }

        DateTime EndTime { get; }

        PsdzTaExecutionState? ExecutionState { get; }

        IEnumerable<IPsdzFailureCause> FailureCauses { get; }

        bool HasFailureCauses { get; }

        DateTime StartTime { get; }
    }
}
