using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    public interface IPsdzDetailedStatusCto
    {
        IPsdzDiagAddress DiagAddressCto { get; }

        IPsdzFeatureIdCto FeatureIdCto { get; }

        PsdzTokenDetailedStatusEtoEnum TokenDetailedStatusEto { get; }
    }
}
