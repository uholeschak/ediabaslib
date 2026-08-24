using BMW.Authoring.API.Interface.Rita;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using PsdzClient;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using PsdzClientLibrary.Core.Module;
using System;
using BMW.Authoring.API.Interface.Sfa;

namespace BMW.Authoring.API
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    public interface IAuthoringModule : IHideObjectMembers
    {
        ILogic IstaOperationLogic { get; }

        BMW.Rheingold.CoreFramework.DatabaseProvider.Vehicle Vehicle { get; }

        IFFMDynamicResolver FFMDynamicResolver { get; }

        [Obsolete("Please use EcuKom")]
        IEcuKom ecuKom { get; }

        IEcuKom EcuKom { get; }

        IProtocolBasicBase FastaProtocolerBase { get; }

        IDealerData DealerData { get; set; }

        [PreserveSource(Hint = "IDatabaseProvider", Placeholder = true)]
        PsdzDatabase DBProvider { get; set; }

        SessionInfo SessionInfo { get; }

        ISfaHandler SfaHandler { get; }

        IRitaFunctionsProvider RitaFunctionsProvider { get; }

        BMW.Rheingold.CoreFramework.DatabaseProvider.Vehicle VehicleDeepClone(BMW.Rheingold.CoreFramework.DatabaseProvider.Vehicle vehicle);
    }
}