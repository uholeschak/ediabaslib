using System.ComponentModel;
using BMW.Rheingold.CoreFramework;

namespace BMW.Authoring.Session
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public enum NetworkType
    {
        Unknown = -1,
        LAN,
        WLAN,
        directLAN
    }
}
