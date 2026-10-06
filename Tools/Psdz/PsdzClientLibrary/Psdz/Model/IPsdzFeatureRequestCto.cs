using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.SecurityManagement;
using BMW.Rheingold.Psdz.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    public interface IPsdzFeatureRequestCto
    {
        IPsdzFeatureIdCto FeatureId { get; }

        PsdzSfaLinkTypeEtoEnum SfaLinkType { get; }

        IPsdzEcuUidCto EcuUid { get; }

        IList<IPsdzValidityConditionCto> ValidityConditions { get; }

        IList<IPsdzFeatureSpecificFieldCto> FeatureSpecificFields { get; }

        int EnableType { get; }
    }
}
