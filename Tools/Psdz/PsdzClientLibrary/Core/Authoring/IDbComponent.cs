using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Database
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public interface IDbComponent : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        long IsarID { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        ITextContent Text { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string SysName { get; }
    }
}
