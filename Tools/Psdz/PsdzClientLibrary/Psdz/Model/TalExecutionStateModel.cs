using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public enum TalExecutionStateModel
    {
        [EnumMember(Value = "AbortedByError")]
        AbortedByError,
        [EnumMember(Value = "AbortedByUser")]
        AbortedByUser,
        [EnumMember(Value = "Executable")]
        Executable,
        [EnumMember(Value = "Finished")]
        Finished,
        [EnumMember(Value = "FinishedForHardwareTransactions")]
        FinishedForHardwareTransactions,
        [EnumMember(Value = "FinishedForHardwareTransactionsWithError")]
        FinishedForHardwareTransactionsWithError,
        [EnumMember(Value = "FinishedForHardwareTransactionsWithWarnings")]
        FinishedForHardwareTransactionsWithWarnings,
        [EnumMember(Value = "FinishedWithError")]
        FinishedWithError,
        [EnumMember(Value = "FinishedWithWarnings")]
        FinishedWithWarnings,
        [EnumMember(Value = "Running")]
        Running
    }
}