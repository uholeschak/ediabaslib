using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    public interface IPsdzSecureTokenForTal
    {
        IPsdzEcuIdentifier EcuIdentifier { get; }

        long FeatureId { get; }

        string SerializedSecureToken { get; }

        string TokenId { get; }
    }
}
