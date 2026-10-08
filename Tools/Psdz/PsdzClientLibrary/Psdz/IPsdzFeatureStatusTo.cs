using RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding;

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