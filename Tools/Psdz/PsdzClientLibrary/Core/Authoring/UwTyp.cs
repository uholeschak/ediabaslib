using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public enum UwTyp
    {
        NONE,
        Discrete,
        DATA,
        LP,
        TEXT,
        WMU,
        UNKNOWN
    }
}
