using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz
{
    public interface IPsdzSecureTokenForVehicleEto
    {
        IPsdzFeatureIdCto FeatureIdCto { get; }

        string TokenId { get; }

        string SerializedSecureToken { get; }
    }
}
