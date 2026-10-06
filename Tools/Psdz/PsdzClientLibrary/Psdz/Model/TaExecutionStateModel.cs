using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjectss
{
    public enum TaExecutionStateModel
    {
        [EnumMember(Value = "Executable")]
        Executable,
        [EnumMember(Value = "Inactive")]
        Inactive,
        [EnumMember(Value = "NotExecutable")]
        NotExecutable,
        [EnumMember(Value = "AbortedByError")]
        AbortedByError,
        [EnumMember(Value = "AbortedByUser")]
        AbortedByUser,
        [EnumMember(Value = "Finished")]
        Finished,
        [EnumMember(Value = "FinishedWithError")]
        FinishedWithError,
        [EnumMember(Value = "FinishedWithWarnings")]
        FinishedWithWarnings,
        [EnumMember(Value = "Running")]
        Running,
        [EnumMember(Value = "Repeat")]
        Repeat,
        [EnumMember(Value = "NotRequired")]
        NotRequired
    }
}