using System.ComponentModel;
using BMW.Rheingold.CoreFramework;

namespace BMW.Authoring.Session.Enums
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public enum OperationalMode
    {
        ISTA,
        ISTA_PLUS,
        ISTA_LIGHT,
        ISTA_POWERTRAIN,
        RITA,
        ISTAHV,
        OPAPI
    }
}
