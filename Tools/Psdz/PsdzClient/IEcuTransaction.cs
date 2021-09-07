using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    //[AuthorAPI(SelectableTypeDeclaration = true)]
    public enum StateType
    {
        stopped,
        running,
        finished,
        error,
        unknown,
        idle
    }

    //[AuthorAPI(SelectableTypeDeclaration = false)]
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
