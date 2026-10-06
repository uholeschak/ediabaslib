using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    public interface IPsdzEcuFeatureTokenRelationCto
    {
        IPsdzEcuIdentifier ECUIdentifier { get; }

        PsdzFeatureGroupEtoEnum FeatureGroup { get; }

        IPsdzFeatureIdCto FeatureId { get; }

        string TokenId { get; }
    }
}
