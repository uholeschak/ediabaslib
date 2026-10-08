namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Svb
{
    public interface IPsdzOrderPart : IPsdzLogisticPart
    {
        IPsdzLogisticPart[] Deliverables { get; }

        IPsdzLogisticPart[] Pattern { get; }
    }
}
