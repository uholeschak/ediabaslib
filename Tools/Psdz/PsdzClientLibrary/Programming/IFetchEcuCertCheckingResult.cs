using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IFetchEcuCertCheckingResult
    {
        IEnumerable<IEcuFailureResponse> FailedEcus { get; }

        IEnumerable<IEcuCertCheckingResponse> Results { get; }
    }
}