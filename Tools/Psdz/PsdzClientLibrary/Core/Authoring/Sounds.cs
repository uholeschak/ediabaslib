using PsdzClient.Core;
using System.ComponentModel;

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
