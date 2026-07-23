using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Contracts;
using PsdzClient.Core;
using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.AutomotiveSecurity
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IKdsCollectionResult
    {
        IBoolResultObject OverallResult { get; }

        IList<IKdsResult> IndividualResults { get; }
    }
}
