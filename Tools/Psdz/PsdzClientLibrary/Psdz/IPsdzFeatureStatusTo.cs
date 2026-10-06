using BMW.Rheingold.Psdz.Model.SecureCoding;
using BMW.Rheingold.Psdz.Model.Sfa;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa.FeatureStatusTo
{
    public interface IPsdzFeatureStatusTo
    {
        PsdzFeatureStatusEtoEnum FeatureStatus { get; set; }

        IPsdzFeatureIdCto FeatureId { get; set; }

        IPsdzDiagAddressCto DiagAddress { get; set; }

        PsdzValidationStatusEtoEnum ValidationStatus { get; set; }
    }
}