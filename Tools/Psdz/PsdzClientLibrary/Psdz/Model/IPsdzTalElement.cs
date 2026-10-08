using System;
using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal.TalStatus;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal
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
