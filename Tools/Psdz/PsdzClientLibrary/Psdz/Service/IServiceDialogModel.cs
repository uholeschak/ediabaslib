using System.Collections.Generic;
using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.ServiceProgram
{
    public interface IServiceDialogModel : INotifyPropertyChanged, IModuleExecutionStep
    {
        string Id { get; }

        bool IsInputDone { get; }

        string Title { get; }

        IEnumerable<IServiceProgramCustomButton> CustomButtons { get; }

        bool IsMainButtonBarVisible { get; }
    }
}
