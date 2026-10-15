namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IStateListLocator : ISPELocator
    {
        IStateLocator GetState(object obj);
    }
}
