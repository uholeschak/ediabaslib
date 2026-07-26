using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public enum FehlergruppeWert
    {
        None,
        Funktionszustand,
        Steuergerätefehler,
        ElektrischePlausibilitätsfehler,
        BusKommunikationsfehler,
        Informationseintrage,
        BotschaftsSignalfehler
    }
}
