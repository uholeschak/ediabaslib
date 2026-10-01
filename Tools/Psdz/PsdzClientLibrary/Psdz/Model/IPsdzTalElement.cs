using System;
using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Tal.TalStatus;

namespace BMW.Rheingold.Psdz.Model.Tal
{
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
