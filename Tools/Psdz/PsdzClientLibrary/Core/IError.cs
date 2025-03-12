namespace PsdzClient.Core
{
    public interface IError
    {
        string Code { get; }

        string Description { get; }
    }
}