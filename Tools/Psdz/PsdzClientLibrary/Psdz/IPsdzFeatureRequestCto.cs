using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.SecurityManagement;

namespace BMW.Rheingold.Psdz.Model.Sfa
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
