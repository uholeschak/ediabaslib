using System.ComponentModel;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;

namespace BMW.Authoring.Database
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public interface IDbEcuVariant : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        long IsarID { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string Name { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string Kurzname { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        ITextContent Titel { get; }
    }
}
