using PsdzClient.Core;
using System.ComponentModel;

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
