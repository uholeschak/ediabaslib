using PsdzClient.Core;
using PsdzClient;

namespace BMW.Authoring.API
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    [PreserveSource(Hint = "Dummy class", SuppressWarning = true)]
    public interface IBackendCommunication : IHideObjectMembers
    {
    }
}
