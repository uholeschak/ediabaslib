namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal
{
    public interface IPsdzFsaTa : IPsdzTa, IPsdzTalElement
    {
        long EstimatedExecutionTime { get; set; }

        long FeatureId { get; set; }
    }
}
