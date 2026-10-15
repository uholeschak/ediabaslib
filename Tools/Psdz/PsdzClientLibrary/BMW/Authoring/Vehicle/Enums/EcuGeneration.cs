using System.ComponentModel;
using BMW.Rheingold.CoreFramework;

namespace BMW.Authoring.Vehicle.Enums
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public enum EcuGeneration
    {
        Classic,
        Next,
        Unknown
    }
}
