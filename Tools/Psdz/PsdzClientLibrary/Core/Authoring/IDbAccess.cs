using PsdzClient.Core;
using System.ComponentModel;
using PsdzClient;

namespace BMW.Authoring.Database
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    [PreserveSource(Hint = "Dummy class", SuppressWarning = true)]
    public interface IDbAccess : IHideObjectMembers
    {
    }
}
