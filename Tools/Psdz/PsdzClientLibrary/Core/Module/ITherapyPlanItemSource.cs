using PsdzClient.Core;
using PsdzClient.Programming;
using System.Collections.Generic;
using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ITherapyPlanItemSource : INotifyPropertyChanged
    {
        string Title { get; }

        string InfoType { get; }

        string StateText { get; }

        IList<ISgbmIdChange> SgbmId { get; }

        IList<IStateResultObject> StateResultList { get; }
    }
}
