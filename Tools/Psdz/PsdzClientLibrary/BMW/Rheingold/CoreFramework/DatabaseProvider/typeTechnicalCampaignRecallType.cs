using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum typeTechnicalCampaignRecallType
    {
        NONE,
        SAFETY,
        EMISSION,
        NONCOMPLIANT
    }
}
