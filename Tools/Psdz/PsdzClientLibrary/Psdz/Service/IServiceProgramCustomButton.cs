using System;
using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.ServiceProgram
{
    public interface IServiceProgramCustomButton : INotifyPropertyChanged
    {
        bool IsVisible { get; }

        bool IsEnabled { get; }

        Guid Id { get; }

        string LocalizedName { get; }

        string Name { get; }
    }
}
