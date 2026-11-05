using BMW.Rheingold.CoreFramework;
using System.ComponentModel;

namespace BMW.Authoring.Programming.API.Models.Interfaces
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IIstufenTriple : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        string Current { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string Last { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string Shipment { get; }
    }
}
