using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects
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