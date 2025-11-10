using PsdzClient.Core;
using PsdzClient.Programming;
using System.Collections.Generic;
using System.ComponentModel;

namespace PsdzClient.Programming
{
    public interface ITherapyPlanAction : INotifyPropertyChanged
    {
        string Title { get; }

        string InfoType { get; }

        typeDiagObjectState State { get; }

        IList<LocalizedText> GetLocalizedObjectTitle(IList<string> lang);
    }
}