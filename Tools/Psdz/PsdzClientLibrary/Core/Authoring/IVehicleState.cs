using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Session
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IVehicleState : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        int PWF_Zustand { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        double KL15 { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        double KL30 { get; }
    }
}
