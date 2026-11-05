using BMW.Rheingold.CoreFramework;
using System.ComponentModel;

namespace BMW.Authoring.Programming.API.Models.Interfaces
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface ISgbmId : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        long Id { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        int MainVersion { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        int PatchVersion { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string ProcessClass { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        int SubVersion { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string HexString { get; }
    }
}
