using System.ComponentModel;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public interface IFsc : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        IEcu Ecu { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        int Applikationsnummer { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        int UpgradeIndex { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        ITextContent Titel { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        FscStatus FscStatus { get; }
    }
}
