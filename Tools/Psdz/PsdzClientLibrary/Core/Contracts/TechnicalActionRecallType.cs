using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum TechnicalActionRecallType
    {
        None,
        Safety,
        Emission,
        NonCompliant
    }
}
