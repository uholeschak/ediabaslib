using System.ComponentModel;
using BMW.Rheingold.CoreFramework;

namespace BMW.Authoring.API.OBFCM
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public enum LastTransmissionStatus
    {
        OK,
        ECU_COMMUNICATION_ERROR,
        VEHICLE_MANIPULATION_DETECTED,
        MANUFACTURER_EXCLUDED
    }
}
