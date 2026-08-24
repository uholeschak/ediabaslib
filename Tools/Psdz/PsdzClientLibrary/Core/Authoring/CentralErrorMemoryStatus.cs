using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle.Enums
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public enum CentralErrorMemoryStatus
    {
        UNKNOWN,
        ZFS,
        CEM,
        ERROR
    }
}
