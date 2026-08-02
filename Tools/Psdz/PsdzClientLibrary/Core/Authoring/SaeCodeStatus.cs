using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public enum SaeCodeStatus
    {
        Unknown = -1,
        ConfirmedPermanent = 1,
        Pending = 2,
        Confirmed = 3,
        Permanent = 4
    }
}
