namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding
{
    public interface IPsdzScbResultStatusCto
    {
        string AppErrorId { get; }

        string Code { get; }

        string ErrorMessage { get; }
    }
}
