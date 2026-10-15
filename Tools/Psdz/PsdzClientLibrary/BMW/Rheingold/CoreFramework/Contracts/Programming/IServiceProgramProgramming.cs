using System;
using System.Collections.Generic;
using BMW.Rheingold.CoreFramework.DatabaseProvider;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    public interface IServiceProgramProgramming
    {
        IXepInfoObject XepInfoObject { get; }

        string Identifier { get; }

        SwiActionLinkType LinkType { get; }

        decimal? Priority { get; }

        DateTime? StartExecution { get; }

        DateTime? EndExecution { get; }

        IList<ISwiActionReport> SwiActionReport { get; }

        ProgrammingActionState Execute(IProgrammingSessionExt session);

        ProgrammingActionState Execute(IProgrammingSessionExt session, bool setWaringOnInvokeException);
    }
}
