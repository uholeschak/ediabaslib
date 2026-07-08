using System.ComponentModel;
using System;

namespace PsdzClient.Core
{
    public interface IInteractionModel : INotifyPropertyChanged
    {
        Guid Guid { get; }

        string Title { get; }

        bool IsCloseButtonEnabled { get; }

        bool IsPrintButtonVisible { get; }

        int DialogSize { get; }

        bool IsCustomDialogSize { get; }

        int DialogWidth { get; }

        int DialogHeight { get; }

        int DialogOffset { get; }

        event EventHandler ModelClosedByUser;
    }
}