namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Svb
{
    public interface IPsdzReplacementPart : IPsdzLogisticPart
    {
        IPsdzLogisticPart[] Deliverables { get; }
    }
}
