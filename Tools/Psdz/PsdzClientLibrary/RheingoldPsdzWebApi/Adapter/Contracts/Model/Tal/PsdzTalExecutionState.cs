namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal
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