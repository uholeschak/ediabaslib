namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ICombinedFaultLocator : IFaultCodeLocator, ISPELocator
    {
    }
}
