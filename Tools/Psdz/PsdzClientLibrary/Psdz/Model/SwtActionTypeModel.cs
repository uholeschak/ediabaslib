using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
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