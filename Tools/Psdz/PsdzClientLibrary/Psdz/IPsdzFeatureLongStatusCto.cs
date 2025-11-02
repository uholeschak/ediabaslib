using System;
using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
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
