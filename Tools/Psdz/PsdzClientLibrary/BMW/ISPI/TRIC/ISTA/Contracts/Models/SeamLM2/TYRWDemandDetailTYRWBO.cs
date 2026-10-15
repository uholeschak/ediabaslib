using BMW.ISPI.TRIC.ISTA.Contracts.Models.SeamLM2.Enums;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Models.SeamLM2
{
    public sealed class TYRWDemandDetailTYRWBO
    {
        public string tyreId { get; set; }

        public double? actualProfileDepth { get; set; }

        public TyreSeason? season { get; set; }

        public bool? isWinterCapable { get; set; }

        public int? tireMileage { get; set; }
    }
}
