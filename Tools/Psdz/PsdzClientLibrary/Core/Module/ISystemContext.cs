using PsdzClient.Core;

namespace BMW.Rheingold.ISTA.CoreFramework.SOCAccessor
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    public interface ISystemContext
    {
        object GetProperty(string PropertyName);

        T GetProperty<T>(string PropertyName);
    }
}
