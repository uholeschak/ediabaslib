using System;
using System.Collections.Generic;
using BMW.Rheingold.CoreFramework.Contracts.Programming;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    public interface IEscalationStep
    {
        ProgrammingActionState State { get; }

        int Step { get; }

        DateTime StartTime { get; }

        DateTime EndTime { get; }

        IEnumerable<IProgrammingFailure> Errors { get; }
    }
}