namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IVehiclePartLocator : ISPELocator
    {
        string Title { get; }
    }
}
