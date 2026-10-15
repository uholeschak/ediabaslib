namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IVirtualFaultCodeLocator : IFaultCodeLocator, ISPELocator
    {
    }
}
