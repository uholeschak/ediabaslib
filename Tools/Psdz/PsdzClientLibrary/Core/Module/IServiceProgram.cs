using PsdzClient.Core;

namespace BMW.Rheingold.ISTA.CoreFramework.SOCAccessor
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    public interface IServiceProgram
    {
        object GetPersistantProperty(string name);

        T GetPersistantProperty<T>(string name);

        void SetPersistantProperty(string name, object parameter);

        void SetProperty(string name, object parameter);
    }
}
