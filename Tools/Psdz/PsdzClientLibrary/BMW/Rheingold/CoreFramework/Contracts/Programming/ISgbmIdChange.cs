using System;
using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [Obsolete("Use BMW.Authoring.Programming.API.Models.Interfaces.ISgbmIdChange instead.")]
    public interface ISgbmIdChange : INotifyPropertyChanged
    {
        string Actual { get; }

        string Target { get; }
    }
}