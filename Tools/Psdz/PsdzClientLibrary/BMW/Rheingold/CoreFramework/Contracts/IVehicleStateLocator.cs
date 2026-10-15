namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IVehicleStateLocator : ISPELocator
    {
        string Title { get; }
    }

}
