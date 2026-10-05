using PsdzClient.Core;
using System.ComponentModel;
using BMW.Rheingold.CoreFramework;

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
