using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public enum RootCertStatusModel
    {
        [EnumMember(Value = "NotAvailable")]
        NotAvailable,
        [EnumMember(Value = "Accepted")]
        Accepted,
        [EnumMember(Value = "Rejected")]
        Rejected,
        [EnumMember(Value = "Invalid")]
        Invalid
    }
}