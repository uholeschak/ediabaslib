using BMW.Authoring;
using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IMerkmaleHeat : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Nr8stellig { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Nr3stellig { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string FortlaufendeNummer { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Kraftstoffart { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Lebenszyklus { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Leistungsklasse { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Platzhalter1 { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Platzhalter2 { get; }
    }
}
