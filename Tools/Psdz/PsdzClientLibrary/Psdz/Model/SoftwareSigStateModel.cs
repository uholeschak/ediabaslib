using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public enum SoftwareSigStateModel
    {
        [EnumMember(Value = "Accepted")]
        Accepted,
        [EnumMember(Value = "Imported")]
        Imported,
        [EnumMember(Value = "Invalid")]
        Invalid,
        [EnumMember(Value = "NotAvailable")]
        NotAvailable,
        [EnumMember(Value = "Rejected")]
        Rejected
    }
}