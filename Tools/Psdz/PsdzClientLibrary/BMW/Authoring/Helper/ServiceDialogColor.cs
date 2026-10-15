using System.ComponentModel;
using BMW.Rheingold.CoreFramework;

namespace BMW.Authoring.Helper
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public enum ServiceDialogColor
    {
        Black,
        White,
        Lightgray,
        Gray,
        Green,
        LightGreen,
        Red,
        Orange,
        Yellow,
        Blue
    }
}
