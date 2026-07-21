using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum TechnicalActionState
    {
        Open,
        Active,
        Closed
    }
}
