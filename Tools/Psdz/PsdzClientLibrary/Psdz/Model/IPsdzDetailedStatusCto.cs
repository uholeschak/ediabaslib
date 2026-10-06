using BMW.Rheingold.Psdz.Model.Ecu;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    public interface IPsdzDetailedStatusCto
    {
        IPsdzDiagAddress DiagAddressCto { get; }

        IPsdzFeatureIdCto FeatureIdCto { get; }

        PsdzTokenDetailedStatusEtoEnum TokenDetailedStatusEto { get; }
    }
}
