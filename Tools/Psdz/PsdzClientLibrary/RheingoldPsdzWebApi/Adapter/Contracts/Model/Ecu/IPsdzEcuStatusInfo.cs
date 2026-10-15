namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu
{
    public interface IPsdzEcuStatusInfo
    {
        byte ByteValue { get; }

        bool HasIndividualData { get; }
    }
}
