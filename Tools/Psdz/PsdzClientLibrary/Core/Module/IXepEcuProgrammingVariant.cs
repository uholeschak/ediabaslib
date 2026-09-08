namespace BMW.ISPI.TRIC.ISTA.Contracts.Interfaces
{
    public interface IXepEcuProgrammingVariant
    {
        decimal EcuVariantId { get; }

        string Name { get; }

        decimal Id { get; }

        decimal? FlashLimit { get; }
    }
}
