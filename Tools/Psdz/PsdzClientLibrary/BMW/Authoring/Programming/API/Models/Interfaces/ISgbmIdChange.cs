using BMW.Rheingold.CoreFramework;
using System.ComponentModel;

namespace BMW.Authoring.Programming.API.Models.Interfaces
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface ISgbmIdChange : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        string Actual { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string Target { get; }
    }
}
