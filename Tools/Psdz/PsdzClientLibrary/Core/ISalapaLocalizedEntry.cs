using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
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
