using PsdzClient.Core;
using System.ComponentModel;

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
