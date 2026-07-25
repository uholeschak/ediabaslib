using BMW.Authoring;
using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IMerkmaleFahrzeug : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Antrieb { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Basisausführung { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Baureihe { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Baureihenverbund { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string EBezeichnung { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string ELebenszyklus { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string ElektrischeReichweite { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Getriebe { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Grundtyp { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Hybridkennzeichen { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Karosserie { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Länderausführung { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Lebenszyklus { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Leittyp { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Lenkung { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Marke { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Montageland { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Produktart { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Produktlinie { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Sicherheitsfahrzeug { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Türen { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Typschlüssel { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Verkaufsbezeichnung { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Sportausfuehrung { get; }
    }
}
