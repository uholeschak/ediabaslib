using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Session
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IDealerData : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Vertriebspartnernummer { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Betriebsnummer { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Händlernummer { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        Testerland Testerland { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Vertriebspartnernummer_IsValue(string wert, params string[] wert_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Vertriebspartnernummer_IsValue(string[] wert);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Betriebsnummer_IsValue(string wert, params string[] wert_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Betriebsnummer_IsValue(string[] wert);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Händlernummer_IsValue(string wert, params string[] wert_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Händlernummer_IsValue(string[] wert);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Testerland_IsValue(Testerland wert, params Testerland[] wert_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Testerland_IsValue(Testerland[] wert);
    }
}
