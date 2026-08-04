using PsdzClient.Core;
using PsdzClient.Programming;
using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.AutomotiveSecurity
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IIPsecLinkDeactivationResult
    {
        IList<IEcuIdentifier> EcusWithComParameter { get; }

        IList<IEcuIdentifier> SuccessfullyHandledEcus { get; }

        IList<IEcuIdentifier> RollbackEcus { get; }

        IList<IEcuFailureResponse> FailedEcus { get; }
    }
}
