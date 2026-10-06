using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.Psdz
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