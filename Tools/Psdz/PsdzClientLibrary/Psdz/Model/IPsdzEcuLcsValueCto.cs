using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    public interface IPsdzEcuLcsValueCto
    {
        IPsdzEcuIdentifier EcuIdentifier { get; }

        int LcsNumber { get; }

        int LcsValue { get; }
    }
}
