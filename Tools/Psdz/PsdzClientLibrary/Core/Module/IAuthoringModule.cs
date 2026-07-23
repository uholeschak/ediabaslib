using BMW.Authoring;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;
using PsdzClient;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using BMW.Authoring.API.Interface.Rita;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using PsdzClientLibrary.Core.Module;

namespace BMW.Authoring.API
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    [PreserveSource(Hint = "No update", SuppressWarning = true)]
    public interface IAuthoringModule : IHideObjectMembers
    {
        ILogic IstaOperationLogic { get; }

        Vehicle Vehicle { get; }

        IFFMDynamicResolver FFMDynamicResolver { get; }

        [Obsolete("Please use EcuKom")]
        IEcuKom ecuKom { get; }

        IEcuKom EcuKom { get; }

        IProtocolBasicBase FastaProtocolerBase { get; }

        IDealerData DealerData { get; set; }

        [PreserveSource(Hint = "IDatabaseProvider", Placeholder = true)]
        PlaceholderType DBProvider { get; set; }

        SessionInfo SessionInfo { get; }

        ISfaHandler SfaHandler { get; }

        IRitaFunctionsProvider RitaFunctionsProvider { get; }

        Vehicle VehicleDeepClone(Vehicle vehicle);
    }
}
