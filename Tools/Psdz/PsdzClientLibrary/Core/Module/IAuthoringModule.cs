using BMW.Authoring.API.Interface.Rita;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using PsdzClient;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using PsdzClientLibrary.Core.Module;
using System;

namespace BMW.Authoring.API
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    [PreserveSource(Hint = "No update", SuppressWarning = true)]
    public interface IAuthoringModule : IHideObjectMembers
    {
        ILogic IstaOperationLogic { get; }

        PsdzClient.Core.Vehicle Vehicle { get; }

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

        PsdzClient.Core.Vehicle VehicleDeepClone(PsdzClient.Core.Vehicle vehicle);
    }
}
