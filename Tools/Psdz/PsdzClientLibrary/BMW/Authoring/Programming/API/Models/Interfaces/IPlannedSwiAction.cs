using BMW.Rheingold.CoreFramework;
using System.ComponentModel;

namespace BMW.Authoring.Programming.API.Models.Interfaces
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IPlannedSwiAction : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        string SwiActionName { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool IsDisabled { get; }
    }
}
