using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public enum IdLightTaTypeModel
    {
        [EnumMember(Value = "IdBackup")]
        IdBackup,
        [EnumMember(Value = "IdRestore")]
        IdRestore
    }
}