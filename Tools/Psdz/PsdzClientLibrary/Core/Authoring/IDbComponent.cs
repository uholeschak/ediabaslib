using System.ComponentModel;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;

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
