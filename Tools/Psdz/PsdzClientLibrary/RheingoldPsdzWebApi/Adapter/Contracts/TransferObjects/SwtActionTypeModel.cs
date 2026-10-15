using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public enum SwtActionTypeModel
    {
        [EnumMember(Value = "ActivateStore")]
        ActivateStore,
        [EnumMember(Value = "ActivateUpdate")]
        ActivateUpdate,
        [EnumMember(Value = "ActivateUpgrade")]
        ActivateUpgrade,
        [EnumMember(Value = "Deactivate")]
        Deactivate,
        [EnumMember(Value = "ReturnState")]
        ReturnState,
        [EnumMember(Value = "WriteVin")]
        WriteVin
    }
}