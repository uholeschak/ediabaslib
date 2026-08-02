using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IZfsContext : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        long STAT_DM_ZEITSTEMPEL { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        int STAT_SYSKONTEXT_WEGSTRECKE_KILOMETER_WERT { get; }
    }
}
