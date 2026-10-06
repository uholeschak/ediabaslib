using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Svb
{
    public interface IPsdzEcuVariantInstance : IPsdzLogisticPart
    {
        IPsdzOrderPart OrderablePart { get; }

        IPsdzEcuVariantInstance[] CombinedWith { get; }

        IPsdzEcu Ecu { get; set; }
    }
}
