using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public enum FscCertStateModel
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