using PsdzClient.Core;
using PsdzClient.Programming;
using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IEcuFailureResponseSet
    {
        IList<IEcuFailureResponse> FailureEcus { get; }
    }
}
