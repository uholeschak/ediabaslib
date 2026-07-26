using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public enum MerkmalHeat : long
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        Nr8stellig = 2000043163697L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Nr3stellig = 2000043163704L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        FortlaufendeNummer = 2000043163725L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Kraftstoffart = 2000043163746L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Lebenszyklus = 2000043163739L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Leistungsklasse = 2000043163732L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Platzhalter1 = 2000043163711L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Platzhalter2 = 2000043163718L
    }
}
