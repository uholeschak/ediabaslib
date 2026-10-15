using BMW.Rheingold.CoreFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using BMW.Rheingold.CoreFramework.ServiceProgram;

namespace BMW.Rheingold.ISTA.CoreFramework.ServiceDialoge
{
    [DataContract]
    public abstract class ServiceDialogModelBase : IServiceDialogModel, INotifyPropertyChanged, IModuleExecutionStep
    {
        [DataMember]
        private readonly string id = Guid.NewGuid().ToString();
        [DataMember]
        private IEnumerable<IServiceProgramCustomButton> customButtons;
        [DataMember]
        private bool isDialogShown;
        [DataMember]
        private bool isInputDone;
        [DataMember]
        private bool isMainButtonBarVisible;
        [DataMember]
        private string title;
        public IEnumerable<IServiceProgramCustomButton> CustomButtons
        {
            get
            {
                return customButtons;
            }

            set
            {
                if (!object.Equals(customButtons, value))
                {
                    customButtons = value;
                    OnPropertyChanged("CustomButtons");
                }
            }
        }

        public bool IsMainButtonBarVisible
        {
            get
            {
                return isMainButtonBarVisible;
            }

            set
            {
                if (!object.Equals(isMainButtonBarVisible, value))
                {
                    isMainButtonBarVisible = value;
                    OnPropertyChanged("IsMainButtonBarVisible");
                }
            }
        }

        public string Id => id;

        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                if (!object.Equals(title, value))
                {
                    title = value;
                    OnPropertyChanged("Title");
                }
            }
        }

        public bool IsInputDone
        {
            get
            {
                return isInputDone;
            }

            set
            {
                if (!object.Equals(isInputDone, value))
                {
                    isInputDone = value;
                    OnPropertyChanged("IsInputDone");
                }
            }
        }

        public bool IsDialogShown
        {
            get
            {
                return isDialogShown;
            }

            set
            {
                if (!object.Equals(isDialogShown, value))
                {
                    isDialogShown = value;
                    OnPropertyChanged("IsDialogShown");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected ServiceDialogModelBase()
        {
            isMainButtonBarVisible = true;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return string.Equals(id, (obj as IServiceDialogModel)?.Id);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}