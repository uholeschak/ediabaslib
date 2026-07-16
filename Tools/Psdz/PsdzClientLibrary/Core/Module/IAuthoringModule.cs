using BMW.Authoring;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using BMW.Rheingold.CoreFramework;

namespace BMW.Authoring.API
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    public interface IAuthoringModule : IHideObjectMembers
    {
        ILogic IstaOperationLogic { get; }

        Vehicle Vehicle { get; }

        IFFMDynamicResolver FFMDynamicResolver { get; }

        [Obsolete("Please use EcuKom")]
        IEcuKom ecuKom { get; }

        IEcuKom EcuKom { get; }

        //IProtocolBasicBase FastaProtocolerBase { get; }

        //IDealerData DealerData { get; set; }

        //IDatabaseProvider DBProvider { get; set; }

        SessionInfo SessionInfo { get; }

        //ISfaHandler SfaHandler { get; }

        //IRitaFunctionsProvider RitaFunctionsProvider { get; }

        Vehicle VehicleDeepClone(Vehicle vehicle);
    }
}
