namespace BMW.ISPI.TRIC.ISTA.Contracts.Interfaces
{
    public interface IXepEcuCliques
    {
        decimal ID { get; }

        string CLIQUENKURZBEZEICHNUNG { get; }

        decimal? ECUREPID { get; }

        string Title { get; }

        bool IsValid { get; }

        decimal? TITLEID { get; }
    }
}
