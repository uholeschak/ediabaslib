using System;
using System.Collections.Generic;
using System.ComponentModel;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IFa : INotifyPropertyChanged
    {
        bool AlreadyDone { get; }

        string BR { get; }

        string C_DATE { get; }

        DateTime? C_DATETIME { get; }

        [Obsolete("No longer used in diagnoses")]
        IEnumerable<string> DealerInstalledSA { get; }

        IEnumerable<string> E_WORT { get; }

        IEnumerable<string> HO_WORT { get; }

        string LACK { get; }

        string LACK_TEXT { get; }

        string POLSTER { get; }

        string POLSTER_TEXT { get; }

        string FAHRZEUG_KATEGORIE { get; }

        string KONTROLL_KLASSE { get; }

        IEnumerable<string> SA { get; }

        string STANDARD_FA { get; }

        string TYPE { get; }

        string VERSION { get; }

        IEnumerable<string> ZUSBAU_WORT { get; }

        ICollection<LocalizedSAItem> SaLocalizedItems { get; }

        string ExtractEreihe();
        string ExtractType();
    }
}