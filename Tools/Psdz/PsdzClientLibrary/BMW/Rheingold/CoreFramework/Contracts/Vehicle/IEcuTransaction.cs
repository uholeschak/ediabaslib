using System;
using System.ComponentModel;
using BMW.Rheingold.CoreFramework.DatabaseProvider;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    public interface IEcuTransaction : INotifyPropertyChanged
    {
        DateTime? transactionEnd { get; }

        bool transactionFinishStatus { get; }

        string transactionId { get; }

        string transactionName { get; }

        string transactionResult { get; }

        DateTime? transactionStart { get; }

        StateType transactionStatus { get; }
    }
}
