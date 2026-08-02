using System.ComponentModel;

namespace PsdzClient.Core;

[AuthorAPI(SelectableTypeDeclaration = true)]
[EditorBrowsable(EditorBrowsableState.Always)]
public enum ProductType
{
    P,
    M
}
