using PsdzClient.Programming;
using System.Collections.Generic;
using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.Contracts.Programming;

namespace BMW.Rheingold.CoreFramework.AutomotiveSecurity
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISecureTokenMetaObject
    {
        string EcuHexAddress { get; }

        long FeatureId { get; }

        IList<IFeatureSpecificField> FeatureSpecificFields { get; }

        IList<IValidityCondition> ValidityConditions { get; }

        int EnableType { get; }

        IBoolResultObject State { get; }
    }
}
