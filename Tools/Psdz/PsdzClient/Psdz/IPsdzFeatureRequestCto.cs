using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public enum PsdzSfaLinkTypeEtoEnum
    {
        VIN,
        ECU_UID,
        VIN_ECU_UID
    }

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
