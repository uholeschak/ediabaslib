namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding
{
    public interface IPsdzCoding1NcdEntry
    {
        int BlockAdress { get; }

        byte[] UserData { get; }

        bool IsWriteable { get; }
    }
}
