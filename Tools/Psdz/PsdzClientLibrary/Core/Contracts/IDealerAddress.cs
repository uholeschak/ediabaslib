namespace BMW.Rheingold.CoreFramework.Contracts
{
    public interface IDealerAddress
    {
        string Name { get; }

        string Street { get; }

        string City { get; }

        string CountryCode { get; }

        string ZipCode { get; }
    }
}
