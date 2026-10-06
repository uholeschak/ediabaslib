using PsdzClient.Programming;
using System.Collections.Generic;
using System.ComponentModel;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    public interface ITherapyPlanAction : INotifyPropertyChanged
    {
        string Title { get; }

        string InfoType { get; }

        typeDiagObjectState State { get; }

        IList<LocalizedText> GetLocalizedObjectTitle(IList<string> lang);
    }
}