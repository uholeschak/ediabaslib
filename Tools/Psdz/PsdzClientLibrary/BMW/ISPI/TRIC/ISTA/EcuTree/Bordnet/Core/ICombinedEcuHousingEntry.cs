namespace BMW.ISPI.TRIC.ISTA.EcuTree.Bordnet.Core
{
    public interface ICombinedEcuHousingEntry
    {
        int Column { get; }

        int Row { get; }

        int EcuCount { get; }

        int[] RequiredEcuAddresses { get; }

        int? ColumnSpan { get; }

        int? RowSpan { get; }

        bool ExtendedWidth { get; }
    }
}
