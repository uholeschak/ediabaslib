using BMW.Authoring;
using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IMerkmaleEMaschine : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Nr8stellig { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Nr3stellig { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Drehmoment { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Einbaulage { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Kraftstoffart { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Leistungsklasse { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Motorarbeitsverfahren { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Überarbeitung { get; }
    }
}
