using BMW.Authoring;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;
using PsdzClient;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;

namespace BMW.Authoring.API
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    [PreserveSource(Hint = "Simplified")]
    public interface IAuthoringModule : IHideObjectMembers
    {
        ILogic IstaOperationLogic { get; }

        Vehicle Vehicle { get; }

        IFFMDynamicResolver FFMDynamicResolver { get; }

        [Obsolete("Please use EcuKom")]
        IEcuKom ecuKom { get; }

        IEcuKom EcuKom { get; }

        //IProtocolBasicBase FastaProtocolerBase { get; }

        IDealerData DealerData { get; set; }

        //IDatabaseProvider DBProvider { get; set; }

        SessionInfo SessionInfo { get; }

        //ISfaHandler SfaHandler { get; }

        //IRitaFunctionsProvider RitaFunctionsProvider { get; }

        Vehicle VehicleDeepClone(Vehicle vehicle);
    }
}
