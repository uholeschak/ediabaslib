namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    public interface IPsdzSecureTokenForVehicleEto
    {
        IPsdzFeatureIdCto FeatureIdCto { get; }

        string TokenId { get; }

        string SerializedSecureToken { get; }
    }
}
