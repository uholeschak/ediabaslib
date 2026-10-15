using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.ISTA.CoreFramework.SOCAccessor
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    public interface ISessionContext
    {
        object GetProperty(string name);

        T GetProperty<T>(string name);
    }
}
