using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public enum MerkmalEMaschine : long
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        Nr8stellig = 20000143363276L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Nr3stellig = 20000143363271L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Drehmoment = 20000143363273L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Einbaulage = 20000143363278L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Kraftstoffart = 20000143363277L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Leistungsklasse = 20000143363274L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Motorarbeitsverfahren = 20000143363272L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Überarbeitung = 20000143363275L
    }
}
