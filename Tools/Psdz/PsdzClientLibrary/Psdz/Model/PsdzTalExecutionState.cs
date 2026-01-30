namespace BMW.Rheingold.Psdz.Model.Tal
{
    public enum PsdzTalExecutionState
    {
        AbortedByError,
        AbortedByUser,
        Executable,
        Finished,
        FinishedForHardwareTransactions,
        FinishedForHardwareTransactionsWithError,
        FinishedForHardwareTransactionsWithWarnings,
        FinishedWithError,
        FinishedWithWarnings,
        Running
    }
}