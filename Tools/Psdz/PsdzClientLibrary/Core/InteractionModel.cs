using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System;

namespace PsdzClient.Core
{
    [DataContract]
    public abstract class InteractionModel : IInteractionModel, INotifyPropertyChanged, IDisposable
    {
        private string title;
        private bool isCloseButtonEnabled;
        private InteractionResponse responseCloseButton;
        private bool isPrintButtonVisible;
        private int dialogSize;
        private bool isCustomDialogSize;
        private int dialogWidth;
        private int dialogHeight;
        private int dialogOffset;
        [DataMember]
        public Guid Guid { get; private set; }
        public bool IsDisposing { get; set; }

        [DataMember]
        public int DialogSize
        {
            get
            {
                return dialogSize;
            }

            set
            {
                dialogSize = value;
                OnPropertyChanged("DialogSize");
            }
        }

        [DataMember]
        public int DialogWidth
        {
            get
            {
                return dialogWidth;
            }

            set
            {
                dialogWidth = value;
                OnPropertyChanged("DialogWidth");
            }
        }

        [DataMember]
        public int DialogHeight
        {
            get
            {
                return dialogHeight;
            }

            set
            {
                dialogHeight = value;
                OnPropertyChanged("DialogHeight");
            }
        }

        [DataMember]
        public int DialogOffset
        {
            get
            {
                return dialogOffset;
            }

            set
            {
                dialogOffset = value;
                OnPropertyChanged("DialogOffset");
            }
        }

        [DataMember]
        public bool IsCustomDialogSize
        {
            get
            {
                return isCustomDialogSize;
            }

            set
            {
                isCustomDialogSize = value;
                OnPropertyChanged("IsCustomDialogSize");
            }
        }

        [DataMember]
        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                title = value;
                OnPropertyChanged("Title");
            }
        }

        [DataMember]
        public bool IsCloseButtonEnabled
        {
            get
            {
                return isCloseButtonEnabled;
            }

            set
            {
                isCloseButtonEnabled = value;
                OnPropertyChanged("IsCloseButtonEnabled");
            }
        }

        [DataMember]
        public InteractionResponse ResponseCloseButton
        {
            get
            {
                return responseCloseButton;
            }

            set
            {
                responseCloseButton = value;
                OnPropertyChanged("ResponseCloseButton");
            }
        }

        [DataMember]
        public bool IsPrintButtonVisible
        {
            get
            {
                return isPrintButtonVisible;
            }

            set
            {
                isPrintButtonVisible = value;
                OnPropertyChanged("IsPrintButtonVisible");
            }
        }

        public event EventHandler Disposed;
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler ModelClosedByUser;
        protected InteractionModel()
        {
            Guid = Guid.NewGuid();
            IsDisposing = false;
            title = "";
            isCloseButtonEnabled = false;
            responseCloseButton = null;
            isPrintButtonVisible = true;
        }

        public override bool Equals(object obj)
        {
            if (obj is InteractionModel interactionModel && Guid.Equals(interactionModel.Guid))
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }

        public virtual void Dispose()
        {
            if (!IsDisposing)
            {
                IsDisposing = true;
                this.Disposed?.Invoke(this, new EventArgs());
            }
        }

        internal virtual void OnRegistered()
        {
        }

        public virtual void LogMessage()
        {
            Log.Warning("InteractionModel.LogMessage", "[TestAutomationInteraction] Model is not handled for Test Automation. Type: " + GetType().Name);
        }

        public virtual void LogResponseMessage(object response)
        {
            Log.Info("InteractionModel.LogResponseMessage", "[TestAutomationInteraction] Model is not handled for Test Automation response message. Type: " + GetType().Name);
        }

        internal virtual void OnDeregistered()
        {
        }

        internal void SetCloseButtonEnableState(bool state)
        {
            IsCloseButtonEnabled = state;
        }

        internal virtual void OnClosing()
        {
            Log.Info("InteractionModel.OnClosing()", "Model was closed by button.");
            this.ModelClosedByUser?.Invoke(this, new EventArgs());
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}