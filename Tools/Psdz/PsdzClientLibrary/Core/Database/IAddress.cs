namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public interface IAddress
    {
        string country { get; }

        string postalCode { get; }

        string street1 { get; }

        string street2 { get; }

        string town1 { get; }

        string town2 { get; }
    }
}
