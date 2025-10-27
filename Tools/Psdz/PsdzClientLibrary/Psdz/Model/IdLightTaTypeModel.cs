using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public enum IdLightTaTypeModel
    {
        [EnumMember(Value = "IdBackup")]
        IdBackup,
        [EnumMember(Value = "IdRestore")]
        IdRestore
    }
}