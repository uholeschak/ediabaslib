using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public enum MerkmalMotor : long
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        Nr8stellig = 2000008748279L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Nr3stellig = 53363595L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Einbaulage = 2000008748275L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Hubraum = 63721099L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Kraftstoffart = 2000008748271L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        KraftstoffartEinbaulage = 53330059L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Leistungsklasse = 53349515L,
        [EditorBrowsable(EditorBrowsableState.Always)]
        Überarbeitung = 63806987L
    }
}
