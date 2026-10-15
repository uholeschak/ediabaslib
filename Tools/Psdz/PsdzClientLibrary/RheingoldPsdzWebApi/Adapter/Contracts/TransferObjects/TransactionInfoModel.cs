using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public enum TransactionInfoModel
    {
        [EnumMember(Value = "ACTION_STARTED")]
        ACTION_STARTED,
        [EnumMember(Value = "ACTION_REPEATING")]
        ACTION_REPEATING,
        [EnumMember(Value = "ACTION_FINISHED")]
        ACTION_FINISHED,
        [EnumMember(Value = "ACTION_FINISHED_WITH_ERROR")]
        ACTION_FINISHED_WITH_ERROR,
        [EnumMember(Value = "ACTION_PROGRESSINFO")]
        ACTION_PROGRESSINFO
    }
}