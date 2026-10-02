using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum TherapyPlanItemOrigin
    {
        unknown,
        Diagnosis,
        Logistics,
        System,
        User
    }
}
