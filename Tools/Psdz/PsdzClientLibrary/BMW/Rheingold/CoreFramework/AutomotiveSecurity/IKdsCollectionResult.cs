using System.Collections.Generic;
using BMW.Rheingold.CoreFramework.Contracts;

namespace BMW.Rheingold.CoreFramework.AutomotiveSecurity
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IKdsCollectionResult
    {
        IBoolResultObject OverallResult { get; }

        IList<IKdsResult> IndividualResults { get; }
    }
}
