namespace BMW.Rheingold.CoreFramework.Contracts
{
    public interface IServiceProgramLocator : ISPELocator
    {
        string DocNumber { get; }

        decimal ControlId { get; }
    }
}
