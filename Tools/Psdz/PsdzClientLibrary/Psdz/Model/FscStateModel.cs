using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
{
    public enum FscStateModel
    {
        [EnumMember(Value = "Accepted")]
        Accepted,
        [EnumMember(Value = "Cancelled")]
        Cancelled,
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