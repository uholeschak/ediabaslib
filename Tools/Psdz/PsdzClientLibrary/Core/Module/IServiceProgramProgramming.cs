using BMW.Rheingold.CoreFramework.Contracts.Programming;
using PsdzClient.Programming;
using System;
using System.Collections.Generic;
using PsdzClient;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
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
