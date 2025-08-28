using System;
using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    public enum PsdzFeatureStatusEtoEnum
    {
        DISABLED,
        ENABLED,
        EXPIRED,
        INITIAL_DISABLED,
        INVALID
    }

    public enum PsdzValidationStatusEtoEnum
    {
        E_CHECK_RUNNING,
        E_EMPTY,
        E_ERROR,
        E_FEATUREID,
        E_MALFORMED,
        E_OK,
        E_OTHER,
        E_SECURITY_ERROR,
        E_TIMESTAMP,
        E_UNCHECKED,
        E_VERSION,
        E_WRONG_LINKTOID
    }

    public interface IPsdzFeatureLongStatusCto
    {
        IPsdzEcuIdentifier EcuIdentifierCto { get; set; }

        IList<IPsdzFeatureConditionCto> FeatureConditions { get; set; }

        IPsdzFeatureIdCto FeatureId { get; set; }

        PsdzFeatureStatusEtoEnum FeatureStatusEto { get; set; }

        int MileageOfActivation { get; set; }

        DateTime TimeOfActivation { get; set; }

        string TokenId { get; set; }

        PsdzValidationStatusEtoEnum ValidationStatusEto { get; set; }
    }
}
