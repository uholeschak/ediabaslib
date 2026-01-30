namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    public interface IPsdzDiagAddressCto
    {
        int INVALID_OFFSET { get; }

        int MAX_OFFSETT { get; }

        int MIN_OFFSET { get; }

        bool IsValid { get; }

        string OffsetSetAsHex { get; }

        int OffsetSetAsInt { get; }

        string OffsetSetAsString { get; }
    }
}
