using PsdzClient.Core;
using PsdzClient.Programming;
using System;
using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IStateResultObject : INotifyPropertyChanged
    {
        string ErrorCode { get; set; }

        string ErrorMessage { get; set; }

        DateTime Timestamp { get; set; }

        string Description { get; set; }

        ProgrammingActionState ActionState { get; set; }
    }
}
