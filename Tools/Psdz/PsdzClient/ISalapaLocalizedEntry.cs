using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
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
