using System.ComponentModel;
using BMW.Rheingold.CoreFramework;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public enum FscStatus
    {
        Accepted,
        Cancelled,
        Imported,
        Invalid,
        NotAvailable,
        Rejected,
        Repair
    }
}
