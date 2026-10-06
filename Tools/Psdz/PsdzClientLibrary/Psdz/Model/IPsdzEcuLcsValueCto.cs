using BMW.Rheingold.Psdz.Model.Ecu;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    public interface IPsdzEcuLcsValueCto
    {
        IPsdzEcuIdentifier EcuIdentifier { get; }

        int LcsNumber { get; }

        int LcsValue { get; }
    }
}
