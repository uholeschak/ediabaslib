namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum TalFilterOptions
    {
        Allowed,
        Empty,
        Must,
        MustNot,
        Only
    }
}