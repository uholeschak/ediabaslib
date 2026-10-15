namespace BMW.ISPI.TRIC.ISTA.Contracts.Interfaces
{
    public interface IDtcVehicle
    {
        string ecuAddress { get; set; }

        string dtcId { get; set; }

        bool? Relevance { get; set; }

        decimal? fOrt { get; set; }

        string EcuDTCType { get; set; }
    }
}
