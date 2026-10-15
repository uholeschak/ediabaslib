namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IVehicleAdapters
    {
        bool IsInstalled(IVehicleAdapterLocator adapter);
    }
}
