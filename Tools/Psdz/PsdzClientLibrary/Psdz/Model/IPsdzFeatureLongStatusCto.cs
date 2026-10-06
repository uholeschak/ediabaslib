using System;
using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
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
