using BMW.Authoring;
using System.ComponentModel;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;

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
