namespace RheingoldPsdzWebApi.Adapter.Contracts
{
    public interface IPsdzConnectionVerboseResult
    {
        bool CheckConnection { get; }

        string Message { get; }
    }
}