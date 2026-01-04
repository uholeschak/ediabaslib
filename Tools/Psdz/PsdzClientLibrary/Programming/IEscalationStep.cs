using PsdzClient.Programming;
using System;
using System.Collections.Generic;

namespace PsdzClient.Programming
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