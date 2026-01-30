namespace BMW.Rheingold.Psdz.Model.Tal
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