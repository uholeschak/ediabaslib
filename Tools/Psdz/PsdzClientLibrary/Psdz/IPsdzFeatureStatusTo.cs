using BMW.Rheingold.Psdz.Model.SecureCoding;
using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz
{
    public interface IPsdzFeatureStatusTo
    {
        PsdzFeatureStatusEtoEnum FeatureStatus { get; set; }

        IPsdzFeatureIdCto FeatureId { get; set; }

        IPsdzDiagAddressCto DiagAddress { get; set; }

        PsdzValidationStatusEtoEnum ValidationStatus { get; set; }
    }
}