namespace PsdzClient.Core
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
