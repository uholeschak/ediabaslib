using System.ComponentModel;
using BMW.Rheingold.CoreFramework;
using PsdzClient.Core;

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
