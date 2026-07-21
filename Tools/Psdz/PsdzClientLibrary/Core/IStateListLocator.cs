namespace PsdzClient.Core;

[AuthorAPI(SelectableTypeDeclaration = true)]
public interface IStateListLocator : ISPELocator
{
    IStateLocator GetState(object obj);
}
