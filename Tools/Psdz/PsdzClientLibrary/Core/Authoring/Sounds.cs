using PsdzClient.Core;
using System.ComponentModel;
using BMW.Rheingold.CoreFramework;

namespace BMW.Authoring.Helper
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public enum Sounds
    {
        PositiveFeedback,
        NegativeFeedback
    }
}
