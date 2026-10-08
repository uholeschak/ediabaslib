using System;
using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum StateType
    {
        stopped,
        running,
        finished,
        error,
        unknown,
        idle
    }

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
