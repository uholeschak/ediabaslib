using System.ComponentModel;
using BMW.Rheingold.CoreFramework;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public enum FaultType
    {
        Dtc = 1,
        DtcVirtuell,
        DtcSammel
    }
}
