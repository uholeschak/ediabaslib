using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    public interface IPsdzSecureTokenEto
    {
        IPsdzEcuIdentifier EcuIdentifier { get; }

        IPsdzFeatureIdCto FeatureIdCto { get; }

        string SerializedSecureToken { get; }

        string TokenId { get; }
    }
}
