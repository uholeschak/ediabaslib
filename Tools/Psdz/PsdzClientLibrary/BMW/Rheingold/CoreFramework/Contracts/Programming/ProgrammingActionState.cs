
namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum ProgrammingActionState
    {
        ActionPlanned = 4,
        ActionInProcess = 32,
        ActionSuccessful = 1,
        ActionFailed = 16,
        MissingPrerequisitesForAction = 8,
        ActionWarning = 2
    }
}
