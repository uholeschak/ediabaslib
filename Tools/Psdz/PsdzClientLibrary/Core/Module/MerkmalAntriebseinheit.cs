using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public enum MerkmalAntriebseinheit : long
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        Nr8stellig = 2000038626172L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Nr3stellig = 2000008748259L,
        [EditorBrowsable(EditorBrowsableState.Never)]
        Leistungsklasse = 2000008748263L,
        [EditorBrowsable(EditorBrowsableState.Never)]
        Überarbeitung = 2000008748267L
    }
}
