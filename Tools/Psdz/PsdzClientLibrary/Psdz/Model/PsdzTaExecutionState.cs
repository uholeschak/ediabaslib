namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal
{
    public enum PsdzTaExecutionState
    {
        Executable,
        Inactive,
        NotExecutable,
        AbortedByError,
        AbortedByUser,
        Finished,
        FinishedWithError,
        FinishedWithWarnings,
        Running,
        Repeat,
        NotRequired
    }
}