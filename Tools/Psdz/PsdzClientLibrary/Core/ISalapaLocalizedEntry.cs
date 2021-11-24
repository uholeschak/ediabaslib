using System.ComponentModel;

namespace PsdzClient.Core
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ISalapaLocalizedEntry : INotifyPropertyChanged
    {
        string BENENNUNG { get; }

        string FAHRZEUGART { get; }

        string ISO_SPRACHE { get; }

        uint Index { get; }

        string VERTRIEBSSCHLUESSEL { get; }
    }
}
