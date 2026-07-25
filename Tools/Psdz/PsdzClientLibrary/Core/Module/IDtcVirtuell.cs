using BMW.Authoring;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public interface IDtcVirtuell : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        ITextContent Titel { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string CodeAsString { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Relevanz { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        FehlerklasseWert Fehlerklasse { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        FehlergruppeWert Fehlergruppe { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        IEcu Ecu { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        WarnlampenStatus WarnlampenStatus { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        void _Write_WarnlampenStatus(WarnlampenStatus newValue);
    }
}
