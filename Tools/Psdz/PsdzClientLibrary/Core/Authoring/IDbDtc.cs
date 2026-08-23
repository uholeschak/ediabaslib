using BMW.Authoring;
using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Database
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public interface IDbDtc : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        long IsarID { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        long Code { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        ITextContent Titel { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Relevanz { get; }
    }
}
