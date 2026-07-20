namespace PsdzClient.Core
{
    public interface IServiceProgramLocator : ISPELocator
    {
        string DocNumber { get; }

        decimal ControlId { get; }
    }
}
