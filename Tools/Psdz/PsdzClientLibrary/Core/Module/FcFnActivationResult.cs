using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum FcFnActivationResult
    {
        Success,
        ErrorPsdzNotAvailable,
        ErrorUnexpected,
        ErrorNoConnection,
        ErrorRetrievingSvt,
        ErrorNoTargetSelectorFound,
        ErrorTalGeneration,
        ErrorTalExecution,
        ErrorInvalidArguments
    }
}
